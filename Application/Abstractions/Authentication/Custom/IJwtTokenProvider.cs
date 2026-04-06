using Domain.Application.Entities.Users;
using System.Security.Claims;

namespace Application.Abstractions.Authentication.Custom;

public interface IJwtTokenProvider
{
    TokenResponse CreateToken(TokenRequest tokenRequest);
    string Create(User user);
    string CreateUserToken(User user, HashSet<string> permissions);
    string GenerateToken(string userId, string[] permissions);
    ClaimsPrincipal GetClaimsFromToken(string token);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
