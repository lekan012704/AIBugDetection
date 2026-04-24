namespace Infrastructure.Authentication.KeyCloak
{
    public sealed class KeycloakOptions
    {
        public string AdminUrl { get; set; } = string.Empty; // e.g., "https://keycloak.example.com/auth/realms/your-realm/protocol/openid-connect/token"

        public string TokenUrl { get; set; } = string.Empty; // e.g., "https://keycloak.example.com/auth/admin/realms/your-realm"

        public string AdminClientId { get; set; } = string.Empty;

        public string AdminClientSecret { get; set; } = string.Empty;

        public string AuthClientId { get; set; } = string.Empty;

        public string AuthClientSecret { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Authority { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }
}
