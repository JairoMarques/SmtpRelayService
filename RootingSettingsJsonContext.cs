using System.Text.Json.Serialization;
using SmtpRelayService;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(RootSettings))]
public partial class RootSettingsJsonContext : JsonSerializerContext
{
}
