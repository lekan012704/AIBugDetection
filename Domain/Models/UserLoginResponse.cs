using Domain.Application.Dtos;
using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class UserLoginResponse
    {
        public string? Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? UserName { get; set; }
        [JsonIgnore]
        public string RefreshToken { get; set; } = string.Empty;
        [JsonIgnore]
        public DateTime RefreshTokenExpiresAt { get; set; }
        [JsonIgnore]
        public string SecretKey { get; set; } = string.Empty;
        public UserProfile? User { get; set; }
        public List<MenuSetup> Permissions { get; set; } = new();
    }
}
