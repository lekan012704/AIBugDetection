using Application.Abstractions.AI;
using Application.Helper;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;
using SharedKernel.Helpers.GenericHttpClientService;
using System.Text.RegularExpressions;

namespace Infrastructure.AI;

public sealed class GitHubService : IGitHubService
{
    private readonly AppSettings _appSettings;
    private readonly ILogger<GitHubService> _logger;
    private readonly IGenericHttpClientHandlerService _genericlient;

    public GitHubService(
        ILogger<GitHubService> logger,
        IOptions<AppSettings> appSettings,
        IGenericHttpClientHandlerService genericlient)
    {
        _logger = logger;
        _appSettings = appSettings.Value;
        _genericlient = genericlient;
    }

    public async Task<ErrorOr<string>> FetchCodeAsync(
        string url,
        CancellationToken cancellationToken)
    {
        try
        {
            // ✅ Validate URL
            if (string.IsNullOrWhiteSpace(url))
                return Errors.Common.Validation(
                    "GitHubUrl",
                    "GitHub URL cannot be empty");

            if (!url.Contains("github.com") &&
                !url.Contains("raw.githubusercontent.com"))
                return Errors.Common.Validation(
                    "GitHubUrl",
                    "URL must be a valid GitHub URL");

            var rawUrl = ConvertToRawUrl(url);
            if (string.IsNullOrWhiteSpace(rawUrl))
                return Errors.Common.Validation(
                    "GitHubUrl",
                    "Invalid GitHub URL format. " +
                    "Expected: https://github.com/user/repo/blob/branch/path/file.cs");

            _logger.LogInformation(
                "Fetching code from GitHub. " +
                "Original: {Original} Raw: {Raw}",
                url, rawUrl);

            var headers = new Dictionary<string, string>
            {
                ["User-Agent"] = "AIBugDetection/1.0",
                //["Authorization"] = $"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",
                ["Accept"] = "text/plain"
            };
           

                var code = await _genericlient.GetAsync<string>(
                rawUrl,
                authToken: string.Empty,
                headers: headers);

            if (code is null)
            {
                _logger.LogError(
                    "GitHub returned null response for {Url}",
                    rawUrl);
                return Errors.Common.NotFound(
                    "GitHub",
                    "File not found. Check the URL is correct " +
                    "and the repository is public.");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogError(
                    "GitHub returned empty content for {Url}",
                    rawUrl);
                return Errors.Common.Validation(
                    "GitHub",
                    "The file appears to be empty");
            }

            _logger.LogInformation(
                "Successfully fetched {Length} characters from GitHub",
                code.Length);

            return code;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "GitHub HTTP request failed for {Url}", url);
            return Errors.Infrastructure.ExternalServiceFailure(
                "GitHub",
                $"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex,
                "GitHub request timed out for {Url}", url);
            return Errors.Infrastructure.ExternalServiceFailure(
                "GitHub",
                "Request timed out fetching file from GitHub");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error fetching GitHub code");
            return Errors.Infrastructure.ExternalServiceFailure(
                "GitHub",
                ex.Message);
        }
    }

    private static string? ConvertToRawUrl(string url)
        {
        // ✅ Already a raw URL — return as is
        if (url.Contains("raw.githubusercontent.com"))
            return url;

        // ✅ Trim whitespace first
            url = url.Trim();

        // ✅ Must start with https://github.com/
        if (!url.StartsWith("https://github.com/"))
            return null;

        // ✅ Standard GitHub blob URL
        // https://github.com/{user}/{repo}/blob/{branch}/{path}
        // →
        // https://raw.githubusercontent.com/{user}/{repo}/{branch}/{path}
        if (url.Contains("/blob/"))
        {
            return url
                .Replace(
                    "https://github.com/",
                    "https://raw.githubusercontent.com/")
                .Replace("/blob/", "/");
        }

       
        // https://github.com/{user}/{repo}/{branch}/{path}
        // →
        // https://raw.githubusercontent.com/{user}/{repo}/{branch}/{path}
        if (Regex.IsMatch(url,
            @"^https://github\.com/[\w\-\.]+/[\w\-\.]+/.+"))
        {
            return url.Replace(
                "https://github.com/",
                "https://raw.githubusercontent.com/");
        }

        return null;
    }
}