namespace Infrastructure.Authentication.Custom.ApiKey
{
    public class AuthHeaderSettings
    {
        public const string SectionName = "RequiredHeaders";

        public required string MerchantCode { get; init; }
        public required string ApiSecretKey { get; init; }
    }
}