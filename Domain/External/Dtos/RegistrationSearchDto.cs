using System.Text.Json.Serialization;

namespace Domain.ExternalEntities.Dtos
{

    public class KeycloakUser
    {
        [JsonPropertyName("id")]
        public string IdentityId { get; set; } = string.Empty;   
        public string MerchantCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string OtherNames { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
