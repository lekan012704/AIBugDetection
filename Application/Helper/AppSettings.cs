namespace Application.Helper;

public sealed class AppSettings
{
    // ── Claude ────────────────────────────────────────────
    public string ClaudeOpenApiUrl { get;   set ; } = string.Empty;
    public string ClaudeOpenApiKey { get; set; } = string.Empty;
    public string ClaudeModel { get; set; } = string.Empty;
    public string AnthropicVersion { get; set; } = string.Empty;

    // ── OpenAI ────────────────────────────────────────────
    public string OpenAiApiUrl { get; set; } = string.Empty;
    public string OpenAiApiKey { get; set; } = string.Empty;
    public string OpenAiModel { get; set; } = string.Empty;

    // ── Gemini ────────────────────────────────────────────
    public string GeminiApiKey { get; set; } = string.Empty;
    public string GeminiApiUrl { get; set; } = string.Empty;
    public string GeminiModel { get; set; } = string.Empty;

    // ── App ───────────────────────────────────────────────
    public string ApplicationBaseUrl { get; set; } = string.Empty;
    public bool UseMockAiResponse { get; set; } = false;
    public bool UseSmtp { get; set; } = true;

}