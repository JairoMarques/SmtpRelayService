using System.Text.Json;
using SmtpRelayService;

const string appSettingsPath = "appsettings.json";
var builder = Host.CreateApplicationBuilder(args);
// Leitura do appsettings.json
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile(appSettingsPath, optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Configuration.AddCommandLine(args);

// Injeta o Settings
string json = File.ReadAllText(appSettingsPath);

var rootSettings = JsonSerializer.Deserialize(json, RootSettingsJsonContext.Default.RootSettings) ?? throw new InvalidOperationException("Failed to load Settings from configuration.");

builder.Services.AddSingleton(rootSettings.Settings);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
