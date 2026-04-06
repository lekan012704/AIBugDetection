using Domain.Application.Entities.Users;
using Microsoft.IdentityModel.JsonWebTokens;
using NHibernate.Loader.Custom;
using System.Security.Claims;

namespace Infrastructure.Authentication.Custom.Jwt;

internal static class ClaimsPrincipalExtensions
{
   
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        string? userId = principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
            return Guid.Empty;

        return Guid.TryParse(userId, out Guid parsedUserId)
            ? parsedUserId
            : Guid.Empty; 
    }

    public static string GetIdentityId(this ClaimsPrincipal? principal)
    {
        return
            principal?.FindFirstValue("sub") ??         
            principal?.FindFirstValue(ClaimTypes.NameIdentifier) ??
            string.Empty;
    }


    public static string GetUsername(this ClaimsPrincipal? principal)
    {
        return
            principal?.FindFirstValue("preferred_username") ??
            principal?.FindFirstValue(ClaimTypes.Name) ??
            "System";
    }

    public static string GetUsernameOrDefault(this ClaimsPrincipal? principal)
    {
        return
            principal?.FindFirstValue("preferred_username") ??
            principal?.FindFirstValue(ClaimTypes.Name) ??
            string.Empty;
    }

   
    public static string GetEmailClaim(this ClaimsPrincipal? principal)
    {
        return
            principal?.FindFirstValue("email") ??
            principal?.FindFirstValue(ClaimTypes.Email) ??
            string.Empty; 
    }

    public static string GetUsernameFromEmail(this ClaimsPrincipal? principal)
    {
        return
            principal?.FindFirstValue("email") ??
            principal?.FindFirstValue(ClaimTypes.Email) ??
            string.Empty; 
    }

    
    public static string GetUserRoleClaim(this ClaimsPrincipal? principal)
    {
        return
            principal?.FindFirstValue("roles") ??
            principal?.FindFirstValue(ClaimTypes.Role) ??
            string.Empty; 
    }

  
    public static string GetUtin(this ClaimsPrincipal? principal)
    {
        return
            principal?.FindFirstValue("utin") ??
            string.Empty;
    }

    public static string GetUsernameFromCustomClaim(this ClaimsPrincipal? principal)
    {
        return
            principal?.FindFirstValue("custom_username_claim") ??
            string.Empty; // ✅ Return empty instead of throwing
    }
}
