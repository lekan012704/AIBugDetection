namespace Application.Abstractions.Authentication.Custom;

public interface IUserContext
{
    Guid UserId { get; }

    string IdentityId { get; }
    string UserName { get; }
    string UserRole { get; }
    string Email { get; }
}
