using System.Text.Json.Serialization;

namespace Domain.ExternalEntities.Dtos
{
    public class RegistrationSearchDto
    {
        public string MerchantCode { get; set; } = string.Empty;
        public string TaxPayerReferenceNumber { get; set; } = string.Empty;
        public string TaxAgentReferenceNumber { get; set; } = string.Empty;
        public string PayerUtin { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Othernames { get; set; } = string.Empty;
        public string JTBTin { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TelephoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PayerType { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string ShortStateName { get; set; } = string.Empty;
        public int MerchantId { get; set; }
    }

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
