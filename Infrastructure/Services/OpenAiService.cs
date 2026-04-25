using Application.Abstractions.AI;
using Application.Helper;
using Domain.Application.Dtos.BugDetection;
using ErrorOr;
using Infrastructure.AI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate.Cfg;
using SharedKernel;
using SharedKernel.Helpers.GenericHttpClientService;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Services
{
    public sealed class OpenAiService : IOpenAiService
    {
        private readonly string _apiUrl;
        private readonly Application.Helper.AppSettings _appSettings;
        private readonly ILogger<OpenAiService> _logger;
        private readonly IGenericHttpClientHandlerService _genericlient;

        public OpenAiService(
            ILogger<OpenAiService> logger,
            IOptions<Application.Helper.AppSettings> appSettings,
            IGenericHttpClientHandlerService genericlient)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
            _apiUrl = _appSettings.OpenAiApiUrl;
            _genericlient = genericlient;
        }

        public async Task<ErrorOr<AiInitialAnalysisResult>> RunInitialAnalysisAsync(
            string code,
            string? fileName,
            CancellationToken cancellationToken)
        {
            var prompt = BugDetectionPrompts
                .BuildInitialScanPrompt(code, fileName);

            var result = await CallOpenAiAsync(prompt, cancellationToken);
            if (result.IsError)
                return result.Errors;

            try
            {
                var parsed = JsonSerializer.Deserialize<AiInitialAnalysisResult>(
                    result.Value,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (parsed is null)
                    return Errors.Infrastructure.DatabaseError(
                        "OpenAI.ParseFailed",
                        "Failed to parse initial analysis response");

                parsed.AiProvider = "OpenAI";
                parsed.FallbackUsed = true;
                return parsed;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "Failed to parse OpenAI initial analysis response");
                return Errors.Infrastructure.DatabaseError(
                    "OpenAI.ParseFailed", ex.Message);
            }
        }

        public async Task<ErrorOr<AiDeepAnalysisResult>> RunDeepAnalysisAsync(
            string code,
            string initialObservations,
            string userAnswers,
            string? fileName,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return Errors.Common.Validation(
                        "Code", "Code cannot be empty");

                if (string.IsNullOrWhiteSpace(initialObservations))
                    return Errors.Common.Validation(
                        "InitialObservations",
                        "Run initial analysis first");

                if (string.IsNullOrWhiteSpace(userAnswers))
                    return Errors.Common.Validation(
                        "UserAnswers",
                        "Please answer the follow-up questions");

                var prompt = BugDetectionPrompts.BuildDeepAnalysisPrompt(
                    code, initialObservations, userAnswers, fileName);

                _logger.LogInformation(
                    "Running OpenAI deep analysis. " +
                    "File: {FileName} CodeLength: {Length}",
                    fileName ?? "snippet", code.Length);

                var rawResult = await CallOpenAiAsync(
                    prompt, cancellationToken);

                if (rawResult.IsError)
                    return rawResult.Errors;

                var parsed = ParseResponse<AiDeepAnalysisResult>(
                    rawResult.Value);

                if (parsed.IsError)
                    return parsed.Errors;

                parsed.Value.AiProvider = "OpenAI";
                parsed.Value.FallbackUsed = true;

                _logger.LogInformation(
                    "OpenAI deep analysis complete. " +
                    "Quality: {Quality}",
                    parsed.Value.OverallCodeQuality);

                return parsed.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "OpenAI deep analysis failed");
                return Errors.Infrastructure.DatabaseError(
                    "OpenAI.DeepAnalysis", ex.Message);
            }
        }


private async Task<ErrorOr<string>> CallOpenAiAsync(
    string prompt,
    CancellationToken cancellationToken)
    {
        try
        {
            var apiKey = _appSettings.OpenAiApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
                return Errors.Infrastructure.DatabaseError(
                    "OpenAI.Config",
                    "OpenAI API key not configured.");

            var request = new OpenAiRequest
            {
                Model = _appSettings.OpenAiModel,
                MaxTokens = 2000,
                Temperature = 0.1,
                ResponseFormat = new OpenAiResponseFormat
                {
                    Type = "json_object"
                },
                Messages = new List<OpenAiMessage>
            {
                new OpenAiMessage
                {
                    Role = "system",
                    Content = "You are a senior software architect. " +
                              "Always respond with valid JSON only. " +
                              "No markdown. No explanation. Just JSON."
                },
                new OpenAiMessage
                {
                    Role = "user",
                    Content = prompt
                }
            }
            };

            _logger.LogInformation(
                "Calling OpenAI API. Model: {Model}",
                _appSettings.OpenAiModel);

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var json = JsonSerializer.Serialize(
                request,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null, // 🔥 CRITICAL FIX
                    DefaultIgnoreCondition =
                        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

            using var content =
                new StringContent(json, Encoding.UTF8, "application/json");

            var responseMessage = await httpClient.PostAsync(
                _apiUrl,
                content,
                cancellationToken);

            var rawBody = await responseMessage.Content
                .ReadAsStringAsync(cancellationToken);

            if (!responseMessage.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "OpenAI raw error: {Status} {Body}",
                    responseMessage.StatusCode,
                    rawBody);

                return Errors.Infrastructure.ExternalServiceFailure(
                    "OpenAI",
                    rawBody);
            }

            var response = JsonSerializer.Deserialize<OpenAiResponse>(
                rawBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (response is null || !response.IsValid())
            {
                _logger.LogError(
                    "OpenAI returned invalid response: {Body}",
                    rawBody);

                return Errors.Infrastructure.ExternalServiceFailure(
                    "OpenAI",
                    "Invalid response from OpenAI");
            }

            var text = response.GetText()!;

            _logger.LogInformation(
                "OpenAI success. Tokens: {Total}",
                response.Usage?.TotalTokens ?? 0);

            return CleanJson(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI API call failed");
            return Errors.Infrastructure.ExternalServiceFailure(
                "OpenAI",
                ex.Message);
        }
    }

    private ErrorOr<T> ParseResponse<T>(string json)
            where T : class, new()
        {
            try
            {
                var result = JsonSerializer.Deserialize<T>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition =
                            JsonIgnoreCondition.WhenWritingNull
                    });

                if (result is null)
                {
                    _logger.LogError(
                        "Failed to deserialize OpenAI response. " +
                        "JSON preview: {Json}",
                        json[..Math.Min(500, json.Length)]);

                    return Errors.Infrastructure.DatabaseError(
                        "OpenAI.Parse",
                        "Failed to parse AI response. " +
                        "AI may have returned unexpected format.");
                }

                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "JSON deserialization failed. " +
                    "JSON preview: {Json}",
                    json[..Math.Min(500, json.Length)]);

                return Errors.Infrastructure.DatabaseError(
                    "OpenAI.Parse",
                    $"JSON parse error: {ex.Message}");
            }
        }

        private static string CleanJson(string text)
        {
            var cleaned = text.Trim();

            if (cleaned.StartsWith("```json"))
                cleaned = cleaned[7..];
            else if (cleaned.StartsWith("```"))
                cleaned = cleaned[3..];

            if (cleaned.EndsWith("```"))
                cleaned = cleaned[..^3];

            return cleaned.Trim();
        }
    }
}