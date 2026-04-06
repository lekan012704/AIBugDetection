using Application.Abstractions.Authentication.Custom;
using Application.Abstractions.Authorization;
using Domain.Application.Entities.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace Infrastructure.Authentication.Custom.Jwt;

internal sealed class JwtTokenProvider(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IPermissionRepository permissionRepository) : IJwtTokenProvider
{
    private readonly string secretKey = configuration["Jwt:Secret"]!;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IPermissionRepository _permissionRepository = permissionRepository;

    public string Create(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, user.Email!)
            ]),
            Expires = expires,
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var handler = new JsonWebTokenHandler();

        string token = handler.CreateToken(tokenDescriptor);

        return token;
    }

    public string CreateUserToken(User user, HashSet<string> permissions)
    {
        //get the remote IP address
        //var remoteIP = GetRemoteIPAddress(_httpContextAccessor);

        // Generate Refresh Token
        //var refreshToken = GenerateRefreshToken(remoteIP.ToString());

        string secretKey = configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            //new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Exp, Convert.ToString(DateTime.Now.AddDays(configuration.GetValue<int>("Jwt:RefreshTokenExpirationInDays")).ToString("dd MMM yyyy", DateTimeFormatInfo.InvariantInfo))),
            //new(ClaimTypes.Expiration, Convert.ToString(DateTime.Now.AddDays(configuration.GetValue<int>("Jwt:RefreshTokenExpirationInDays")).ToString("dd MMM yyyy", DateTimeFormatInfo.InvariantInfo))),
            //  new(ClaimTypes.UserData, refreshToken.Token)

            //new(ClaimTypes.Role, user.rol),
            //not good to expose users info as this can be seen on jwt.io website
            //new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, user.Email)
        };

        // Add permissions as claims
        claims.AddRange(permissions.Select(permission =>
            new Claim("permission", permission)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }

    public string GenerateToken(string userId, string[] permissions)
    {
        string secretKey = configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };

        // Add permissions as claims
        foreach (string permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public TokenResponse CreateToken(TokenRequest tokenRequest)
    {
        var refreshTokenId = Guid.NewGuid().ToString();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(configuration.GetValue<int>("Jwt:RefreshTokenExpirationInDays"));
        var (Token, Expires) = CreateAccessToken(tokenRequest, refreshTokenId);
        var refreshToken = CreateRefreshToken(tokenRequest, refreshTokenId, refreshTokenExpiry);

        return new TokenResponse
        {
            Token = Token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = Expires,
            RefreshTokenExpiresAt = refreshToken.Expires
        };
    }

    public TokenResponse RefreshToken(string refreshToken)
    {
        // Validate refresh token
        var principal = GetPrincipalFromToken(refreshToken, validateLifetime: true) ?? throw new SecurityTokenException("Invalid refresh token");

        // Extract user information from refresh token
        var userIdClaim = principal.FindFirst("user_id")?.Value; //principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        var emailClaim = principal.FindFirst("user_email")?.Value; //principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
        var tokenTypeClaim = principal.FindFirst("token_type")?.Value;
        var userRole = principal.FindFirst("user_role")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(emailClaim) || tokenTypeClaim != "refresh")
            throw new SecurityTokenException("Invalid refresh token structure");

        var tokenRequest = new TokenRequest
        {
            Id = userIdClaim,
            Email = emailClaim,
            Role = userRole!
        };

        // Generate new token pair
        return CreateToken(tokenRequest);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        return GetPrincipalFromToken(token, validateLifetime: false);
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = validateLifetime,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero // Remove default 5 minute clock skew
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token algorithm");
            }

            return principal;
        }
        catch (Exception ex) when (ex is SecurityTokenException || ex is ArgumentException)
        {
            return null;
        }
    }

    private (string Token, DateTime Expires) CreateAccessToken(TokenRequest tokenRequest, string refreshTokenId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var useLocalTime = configuration.GetValue<bool>("Jwt:UseLocalTime", false);
        var now = useLocalTime ? DateTime.Now : DateTime.UtcNow;
        var expires = now.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, tokenRequest.Id.ToString()),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, tokenRequest.Email!),
                new Claim("user_id", tokenRequest.Id.ToString()),
                new Claim("user_email", tokenRequest.Email!),
                new Claim("user_role", tokenRequest.Role!),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("token_type", "access"),
                new Claim("refresh_token_id", refreshTokenId) // Link to refresh token
            ]),
            Expires = expires,
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var handler = new JsonWebTokenHandler();
        string token = handler.CreateToken(tokenDescriptor);

        return (token, expires);
    }

    private (string Token, DateTime Expires) CreateRefreshToken(TokenRequest tokenRequest, string refreshTokenId, DateTime expires)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, tokenRequest.Id.ToString()),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, tokenRequest.Email!),
                new Claim("user_id", tokenRequest.Id.ToString()),
                new Claim("user_email", tokenRequest.Email!),

                new Claim("user_role", tokenRequest.Role!),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, refreshTokenId),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("token_type", "refresh") // Distinguish from access token
            ]),
            Expires = expires,
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var handler = new JsonWebTokenHandler();
        string token = handler.CreateToken(tokenDescriptor);

        return (token, expires);
    }

    private RefreshTokens CreateRefreshToken(string ipAddress)
    {
        return new RefreshTokens
        {
            Token = GenerateSecureRandomToken(),
            Expires = DateTime.UtcNow.AddDays(configuration.GetValue<int>("Jwt:RefreshTokenExpirationInDays")),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress,
        };
    }

    private string GenerateSecureRandomToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[64];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public IPAddress GetRemoteIPAddress(IHttpContextAccessor context)
    {
        var remoteIp = context.HttpContext?.Connection.RemoteIpAddress;
        if (remoteIp == null)
            throw new InvalidOperationException("Remote IP Address is not available.");

        // Check if the IP is IPv4 - return it directly
        if (remoteIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            return remoteIp;

        // If it's an IPv6 address that can be mapped to IPv4, return the IPv4 version
        if (remoteIp.IsIPv4MappedToIPv6)
            return remoteIp.MapToIPv4();

        // Otherwise, search for IPv4 addresses in other connection properties
        // Typical places to check would be X-Forwarded-For header or other custom headers
        // This is a simplified example - you might need to customize based on your network setup
        var forwardedHeader = context.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            var addresses = forwardedHeader.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var address in addresses)
            {
                if (IPAddress.TryParse(address.Trim(), out var ip) &&
                    ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip;
            }
        }

        // If we can't find any IPv4 address, return the original IP (which is IPv6)
        return remoteIp;
    }

    public ClaimsPrincipal GetClaimsFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);

        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            return principal;
        }
        catch (Exception)
        {
            return null;
        }
    }


    private string GetIpAddress()
    {
        return _httpContextAccessor.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For")
            ? _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].ToString()
            : _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
    }
}
