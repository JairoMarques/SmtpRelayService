namespace SmtpRelayService;

public class RootSettings
{
    public RootSettings() { }

    public Settings Settings { get; set; } = new Settings();
}
public class Settings
{
    public SmtpSettings SmtpSettings { get; set; } = new SmtpSettings();
    public string AllowedHosts {get; set; } = "";
    public Settings() {}
}
public class SmtpSettings
{
    public SmtpSettings() {}
    public string Host { get; set; } = "";
    public int Port { get; set; } = 0;
    public string SecureSocketOptions { get; set; } = "None";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}
