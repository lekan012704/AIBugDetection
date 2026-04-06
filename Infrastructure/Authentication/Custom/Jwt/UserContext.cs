using Application.Abstractions.Authentication.Custom;
using Microsoft.AspNetCore.Http;
using Quartz.Util;

namespace Infrastructure.Authentication.Custom.Jwt;

internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdString = _httpContextAccessor
                .HttpContext?
                .User
                .GetUserId();

            if (string.IsNullOrWhiteSpace(userIdString.ToString()))
                return Guid.Empty;

            return Guid.TryParse(userIdString.ToString(), out var guid)
                ? guid
                : Guid.Empty;
        }
    }

    // ✅ IdentityId is Keycloak's "sub" claim
    public string IdentityId
    {
        get
        {
            var identityId = _httpContextAccessor
                .HttpContext?
                .User
                .GetIdentityId();

            return string.IsNullOrWhiteSpace(identityId)
                ? string.Empty
                : identityId;  
        }
    }

    public string UserName =>
        _httpContextAccessor
            .HttpContext?
            .User
            .GetUsername() ?? "System";

    public string UserRole =>
        _httpContextAccessor
            .HttpContext?
            .User
            .GetUserRoleClaim() ?? string.Empty; // ✅ Return empty instead of throwing

    public string Email =>
        _httpContextAccessor
            .HttpContext?
            .User
            .GetEmailClaim() ?? string.Empty; // ✅ Return empty instead of throwing
}