//using Domain.Models;
//using Infrastructure.Authentication.Custom.Jwt;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.IdentityModel.JsonWebTokens;
//using System.Security.Claims;


//namespace Infrastructure.Authorization
//{
//    internal sealed class CustomClaimsTransformation(IServiceProvider serviceProvider) : IClaimsTransformation
//    {
//        private readonly IServiceProvider _serviceProvider = serviceProvider;

//        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
//        {
//            if (principal.Identity is not { IsAuthenticated: true } ||
//                principal.HasClaim(claim => claim.Type == ClaimTypes.Role) &&
//                principal.HasClaim(claim => claim.Type == JwtRegisteredClaimNames.Sub))
//            {
//                return principal;
//            }
//            using IServiceScope scope = _serviceProvider.CreateScope();

//            PermissionProvider permissionProvider = scope.ServiceProvider.GetRequiredService<PermissionProvider>();

//            string identityId = principal.GetIdentityId();

//            UserRolesResponse userRoles = await permissionProvider.GetRolesForUserAsync(identityId);

//            if (userRoles is null || userRoles == null)
//            {
//                //return null;  //I still beleive we should return null
//                return principal;
//            }

//            var claimsIdentity = new ClaimsIdentity();

//            claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, userRoles.UserId!.ToString()));

//            foreach (IdentityRole role in userRoles.Roles)
//            {
//                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Name!));
//            }

//            principal.AddIdentity(claimsIdentity);

//            return principal;
//        }
//    }
//}
