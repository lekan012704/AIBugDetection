namespace Application.Handlers.Queries.Users.GetByEmail;

public sealed record UserResponse
{
    public string Id { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;
}
