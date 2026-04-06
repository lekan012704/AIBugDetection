namespace Application.Helper;

public sealed class AppSettings
{
   public string ClaudeOpenApiUrl { get; init; } = string.Empty;
   public string ClaudeOpenApiKey { get; init; } = string.Empty;
   public string ClaudeModel { get; init; } = string.Empty;
   public string AnthropicVersion { get; init; } = string.Empty;
   public string OpenAiApiUrl { get; init; } = string.Empty;    
   public string OpenAiApiKey { get; init; } = string.Empty;    
   public string OpenAiModel { get; init; } = string.Empty;    

}           