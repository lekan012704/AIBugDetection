namespace Infrastructure.Authentication.KeyCloak
{
    public sealed class KeycloakOptions
    {
        public string AdminUrl { get; set; } = string.Empty; // e.g., "https://keycloak.example.com/auth/realms/your-realm/protocol/openid-connect/token"

        public string TokenUrl { get; set; } = string.Empty; // e.g., "https://keycloak.example.com/auth/admin/realms/your-realm"

        public string AdminClientId { get; init; } = string.Empty;

        public string AdminClientSecret { get; init; } = string.Empty;

        public string AuthClientId { get; init; } = string.Empty;

        public string AuthClientSecret { get; init; } = string.Empty;
    }
}
