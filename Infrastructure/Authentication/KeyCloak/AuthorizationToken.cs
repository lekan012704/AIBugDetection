using System.Text.Json.Serialization;

namespace Infrastructure.Authentication.KeyCloak
{
    internal sealed class AuthorizationToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;
    }
}
