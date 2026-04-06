using Application.Abstractions.AI;
using Application.Helper;
using Domain.Application.Dtos.BugDetection;
using ErrorOr;
using Infrastructure.AI.Models;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;
using SharedKernel.Helpers.GenericHttpClientService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public sealed class ClaudeService : IClaudeService
    {
        private readonly string _apiUrl;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ClaudeService> _logger;
        private readonly IGenericHttpClientHandlerService _genericlient;

        public ClaudeService(ILogger<ClaudeService> logger, IOptions<AppSettings> appSettings, IGenericHttpClientHandlerService genericlient)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
            _apiUrl = _appSettings.ClaudeOpenApiUrl;
            _genericlient = genericlient;
        }

        public async Task<ErrorOr<AiInitialAnalysisResult>> RunInitialAnalysisAsync(
            string code,
            string? fileName,
            CancellationToken cancellationToken)
        {
            var prompt = BugDetectionPrompts
                .BuildInitialScanPrompt(code, fileName);

            var result = await CallClaudeAsync(prompt, cancellationToken);
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
                        "Claude.ParseFailed",
                        "Failed to parse initial analysis response");

                parsed.AiProvider = "Claude";
                parsed.FallbackUsed = false;
                return parsed;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "Failed to parse Claude initial analysis response");
                return Errors.Infrastructure.DatabaseError(
                    "Claude.ParseFailed", ex.Message);
            }
        }
        public async Task<ErrorOr<AiDeepAnalysisResult>>
       RunDeepAnalysisAsync(
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
                    "Running Claude deep analysis. " +
                    "File: {FileName} CodeLength: {Length}",
                    fileName ?? "snippet", code.Length);

                var rawResult = await CallClaudeAsync(
                    prompt, cancellationToken);

                if (rawResult.IsError)
                    return rawResult.Errors;

                var parsed = ParseResponse<AiDeepAnalysisResult>(
                    rawResult.Value);

                if (parsed.IsError)
                    return parsed.Errors;

                parsed.Value.AiProvider = "Claude";
                parsed.Value.FallbackUsed = false;

                _logger.LogInformation(
                    "Claude deep analysis complete. " +
                    "Quality: {Quality}",
                    parsed.Value.OverallCodeQuality);

                return parsed.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Claude deep analysis failed");
                return Errors.Infrastructure.DatabaseError(
                    "Claude.DeepAnalysis", ex.Message);
            }
        }

        private async Task<ErrorOr<string>> CallClaudeAsync(
            string prompt,
            CancellationToken cancellationToken)
        {
            try
            {
                var apiKey = _appSettings.ClaudeOpenApiKey;
                if (string.IsNullOrWhiteSpace(apiKey))
                    return Errors.Infrastructure.DatabaseError(
                        "Claude.Config",
                        "Claude API key not configured. " +
                        "Add AI:ClaudeApiKey to appsettings.");

                var request = new ClaudeRequest
                {
                    Model = _appSettings.ClaudeModel,
                    MaxTokens = 8096,
                    Messages = new List<ClaudeMessage>
                {
                    new ClaudeMessage
                    {
                        Role = "user",
                        Content = prompt
                    }
                }
                };

                var headers = new Dictionary<string, string>
                {
                    ["x-api-key"] = apiKey,
                    ["anthropic-version"] = _appSettings.AnthropicVersion
                };

                _logger.LogInformation(
                    "Calling Claude API. Model: {Model}",
                    _appSettings.ClaudeModel);

                var response = await _genericlient
                    .PostAsync<ClaudeRequest, ClaudeResponse>(
                        _apiUrl,
                        request,
                        authToken: string.Empty,
                        headers: headers);

                if (response is null)
                {
                    _logger.LogError(
                        "Claude API returned null response");
                    return Errors.Infrastructure.ExternalServiceFailure(
                        "Claude",
                        "Null response from Claude API. " +
                        "Check API key and network.");
                }

                if (response.Error is not null)
                {
                    _logger.LogError(
                        "Claude API error. Type: {Type} Message: {Message}",
                        response.Error.Type,
                        response.Error.Message);
                    return Errors.Infrastructure.ExternalServiceFailure(
                        "Claude",
                        $"Claude error: {response.Error.Type} — " +
                        $"{response.Error.Message}");
                }

                if (!response.IsValid())
                {
                    _logger.LogError(
                        "Claude returned empty or invalid response. " +
                        "StopReason: {StopReason}",
                        response.StopReason);
                    return Errors.Infrastructure.ExternalServiceFailure(
                        "Claude",
                        $"Empty response. Stop reason: {response.StopReason}");
                }

                var text = response.GetText()!;

                _logger.LogInformation(
                    "Claude API call successful. " +
                    "InputTokens: {Input} OutputTokens: {Output}",
                    response.Usage?.InputTokens ?? 0,
                    response.Usage?.OutputTokens ?? 0);

                return CleanJson(text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Claude API call failed");
                return Errors.Infrastructure.ExternalServiceFailure(
                    "Claude", ex.Message);
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
                        "Failed to deserialize Claude response. " +
                        "JSON preview: {Json}",
                        json[..Math.Min(500, json.Length)]);

                    return Errors.Infrastructure.DatabaseError(
                        "Claude.Parse",
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
                    "Claude.Parse",
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
