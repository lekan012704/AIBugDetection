using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication.Custom.ApiKey
{
    public interface IAuthHeaderValidationService
    {
        ErrorOr<(string MerchantCode, string ApiSecretKey)> ValidateHeader(IHeaderDictionary headers);

        ErrorOr<AuthResponse> ValidateAuthHeader(IHeaderDictionary headers);
    }

    public record AuthResponse(string MerchantCode, string ApiSecretKey);
}