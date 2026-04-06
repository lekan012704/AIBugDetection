using System.Text.Json.Serialization;

namespace Application.Abstractions.Authentication.Custom
{
    public class TokenResponse
    {
        public string Token { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }
        //[JsonIgnore]
        public string RefreshToken { get; init; } = string.Empty;
        [JsonIgnore]
        public DateTime RefreshTokenExpiresAt { get; init; }
        [JsonIgnore]
        public string SecretKey { get; init; } = string.Empty;
    }
}
