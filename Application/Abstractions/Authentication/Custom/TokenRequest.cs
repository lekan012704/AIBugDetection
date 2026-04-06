namespace Application.Abstractions.Authentication.Custom
{
    public class TokenRequest
    {
        public required string Id { get; init; }

        public required string Email { get; init; }
        public required string Role { get; init; }
    }
}
