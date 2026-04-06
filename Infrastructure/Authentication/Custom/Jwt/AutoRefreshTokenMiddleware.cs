using Application.Abstractions.Authentication.Custom;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication.Custom.Jwt
{
    public class AutoRefreshTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IJwtTokenProvider _iJwtTokenProvider;

        public AutoRefreshTokenMiddleware(RequestDelegate next, IJwtTokenProvider iJwtTokenProvider)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _iJwtTokenProvider = iJwtTokenProvider ?? throw new ArgumentNullException(nameof(iJwtTokenProvider));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers.Authorization
                .FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var principal = _iJwtTokenProvider.GetPrincipalFromExpiredToken(token);
                if (principal != null)
                {
                    var tokenType = principal.FindFirst("token_type")?.Value;
                    if (tokenType == "access")
                    {
                        // Token is structurally valid but expired
                        // You could implement logic here to check if refresh is needed
                        context.Items["ExpiredToken"] = true;
                    }
                }
            }

            await _next(context);
        }
    }
}
