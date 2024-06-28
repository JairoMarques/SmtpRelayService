using System.Buffers;
using System.Net;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace SmtpRelayService;

public class Worker(ILogger<Worker> logger, Settings settings) : BackgroundService
{
    private readonly string _queuePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "queue");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"Starting SMTP Relay Service. Version 1.0.0");
        Directory.CreateDirectory(_queuePath);

        var options = new SmtpServerOptionsBuilder()
            .ServerName("localhost")
            .Port(25)
            .Build();

        var serviceProvider = new SmtpServer.ComponentModel.ServiceProvider();
        serviceProvider.Add(new MailboxFilter(logger, settings));
        serviceProvider.Add(new MyMessageStore(_queuePath));

        var smtpServer = new SmtpServer.SmtpServer(options, serviceProvider);

        _ = Task.Run(() => smtpServer.StartAsync(stoppingToken), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            ProcessQueue();
        }
    }

    private void ProcessQueue()
    {
        foreach (var file in Directory.GetFiles(_queuePath))
        {
            try
            {
                var message = MimeKit.MimeMessage.Load(file);
                using var client = new MailKit.Net.Smtp.SmtpClient();
                MailKit.Security.SecureSocketOptions secureSocketOptions = Enum.Parse<MailKit.Security.SecureSocketOptions>(settings!.SmtpSettings.SecureSocketOptions, true);
                client.Connect(settings!.SmtpSettings.Host, settings!.SmtpSettings.Port, MailKit.Security.SecureSocketOptions.None);
                client.Authenticate(settings!.SmtpSettings.UserName, settings!.SmtpSettings.Password);
                client.Send(message);
                client.Disconnect(true);
                File.Delete(file);
                logger.LogInformation($"Success send email: {file}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error sending email: {file}");
            }
        }
    }

    private class MailboxFilter(ILogger<Worker> logger, Settings settings) : IMailboxFilter
    {
        public Task<bool> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox from, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        Task<bool> IMailboxFilter.CanAcceptFromAsync(ISessionContext context, IMailbox from, int size, CancellationToken cancellationToken)
        {
            var allowedIPs = settings!.AllowedHosts.Split("|");
            var remoteEndPoint = context.Properties["EndpointListener:RemoteEndPoint"];
            var remoteIPAddress = "0.0.0.0";
            if (remoteEndPoint != null)
            {
                remoteIPAddress = remoteEndPoint.ToString()!.Split(":")[0];
            }

            logger.LogDebug($"RemoteIpAddress: {remoteIPAddress}");
            if (allowedIPs.Contains(remoteIPAddress))
            {
                return Task.FromResult(true);
            }
            logger.LogInformation($"IP {remoteIPAddress} address not allowed.");
            return Task.FromResult(false);
        }
    }

    private class MyMessageStore(string queuePath) : MessageStore
    {
        private readonly string _queuePath = queuePath;

        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            var path = Path.Combine(_queuePath, $"{Guid.NewGuid()}.eml");
            using (var stream = File.Create(path))
            {
                foreach (var segment in buffer)
                {
                    stream.Write(segment.Span);
                }
            }
            return Task.FromResult(new SmtpResponse(SmtpReplyCode.Ok));
        }
    }
}
