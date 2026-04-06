using Domain.Application.Entities.Users;

namespace Application.Abstractions.Authentication.Custom;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
    bool VerifyPassword(User user, string password);
    string HashPassword(User user, string password);
}
