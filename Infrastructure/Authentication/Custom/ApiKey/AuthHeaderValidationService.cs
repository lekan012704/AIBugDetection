using Application.Abstractions.Authentication.Custom;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Infrastructure.Authentication.Custom.ApiKey
{
    public class AuthHeaderValidationService : IAuthHeaderValidationService
    {
        private readonly AuthHeaderSettings _settings;
        private const string ApiKeyHeaderName = "Api-Secret-Key";
        private const string MerchantCodeHeaderName = "X-Merchant-Code";
        private readonly IJwtTokenProvider _tokenProvider;

        public AuthHeaderValidationService(IOptions<AuthHeaderSettings> settings, IJwtTokenProvider tokenProvider)
        {
            _settings = settings.Value;
            _tokenProvider = tokenProvider;
        }

        public ErrorOr<AuthResponse> ValidateAuthHeader(IHeaderDictionary headers)
        {
            if (!headers.TryGetValue(ApiKeyHeaderName, out var apiSecretKey))
            {
                return Errors.Common.Validation("Auth.MissingApiKey", "API key header is required.");
            }

            if (!headers.TryGetValue(MerchantCodeHeaderName, out var merchantCode))
            {
                return Errors.Common.Validation("Auth.MissingMerchantCode", "Merchant Code header is required.");
            }

            if (!_settings.ApiSecretKey.Equals(apiSecretKey, StringComparison.Ordinal))
            {
                return Errors.Common.Validation("Auth.InvalidApiKey", "Invalid API key provided.");
            }

            if (!_settings.MerchantCode.Equals(merchantCode, StringComparison.Ordinal))
            {
                return Errors.Common.Validation("Auth.InvalidMerchantCode", "Invalid Merchant Code provided.");
            }

            var authResponse = new AuthResponse(
                merchantCode!,
            apiSecretKey!);

            //var validateToken = _tokenProvider.GetClaimsFromToken(apiSecretKey!);
            //var appId = validateToken.FindFirst(ClaimTypes.Hash)?.Value;

            return authResponse;
        }

        public ErrorOr<(string MerchantCode, string ApiSecretKey)> ValidateHeader(IHeaderDictionary headers)
        {
            if (!headers.TryGetValue(ApiKeyHeaderName, out var apiSecretKey))
            {
                return Errors.Common.Validation("Auth.MissingApiKey", "API key header is required.");
            }

            if (!headers.TryGetValue(MerchantCodeHeaderName, out var merchantCode))
            {
                return Errors.Common.Validation("Auth.MissingMerchantCode", "Merchant Code header is required.");
            }

            if (!_settings.ApiSecretKey.Equals(apiSecretKey, StringComparison.Ordinal))
            {
                return Errors.Common.Validation("Auth.InvalidApiKey", "Invalid API key provided.");
            }

            if (!_settings.MerchantCode.Equals(merchantCode, StringComparison.Ordinal))
            {
                return Errors.Common.Validation("Auth.InvalidMerchantCode", "Invalid Merchant Code provided.");
            }

            return (merchantCode!, apiSecretKey!);
        }
    }
}