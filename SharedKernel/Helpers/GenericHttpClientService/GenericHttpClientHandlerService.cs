using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace SharedKernel.Helpers.GenericHttpClientService
{
    /// <summary>
    /// Implementation of the IGenericHttpClientHandlerService interface for making HTTP requests.
    /// </summary>
    public class GenericHttpClientHandlerService : IGenericHttpClientHandlerService
    {
        private readonly ILogger<GenericHttpClientHandlerService> _logger;
        private readonly IHttpClientFactory _factory;
        private const int MaxRetryAttempts = 5;

        public GenericHttpClientHandlerService(ILogger<GenericHttpClientHandlerService> logger, IHttpClientFactory factory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string uri, string authToken = "", Dictionary<string, string>? headers = null)
        {
            try
            {
                _logger.LogInformation("Sending GET request to {Uri}", uri);

                HttpClient  httpClient = CreateHttpClient(uri, authToken, headers);

                var responseMessage = await ExecuteWithRetryAsync(() => httpClient.GetAsync(uri));

                return await ProcessResponseAsync<T>(responseMessage, uri, "GET");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending GET request to {Uri}", uri);
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data, string authToken = "", Dictionary<string, string>? headers = null)
        {
            try
            {
                var content = CreateJsonContent(data);

                _logger.LogInformation("Sending POST request to {Uri} with data: {Data}", uri, JsonConvert.SerializeObject(data));

                HttpClient httpClient = CreateHttpClient(uri, authToken, headers);

                var responseMessage = await ExecuteWithRetryAsync(() => httpClient.PostAsync(uri, content));

                return await ProcessResponseAsync<TResponse>(responseMessage, uri, "POST");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending POST request to {Uri}", uri);
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string uri, TRequest data, string authToken = "", Dictionary<string, string>? headers = null)
        {
            try
            {
                var content = CreateJsonContent(data);

                _logger.LogInformation("Sending PUT request to {Uri} with data: {Data}", uri, JsonConvert.SerializeObject(data));

                HttpClient httpClient = CreateHttpClient(uri, authToken, headers);

                var responseMessage = await ExecuteWithRetryAsync(() => httpClient.PutAsync(uri, content));

                return await ProcessResponseAsync<TResponse>(responseMessage, uri, "PUT");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending PUT request to {Uri}", uri);
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task<T?> DeleteAsync<T>(string uri, string authToken = "", Dictionary<string, string>? headers = null)
        {
            try
            {
                _logger.LogInformation("Sending DELETE request to {Uri}", uri);

                HttpClient httpClient = CreateHttpClient(uri, authToken, headers);

                var responseMessage = await ExecuteWithRetryAsync(() => httpClient.DeleteAsync(uri));

                return await ProcessResponseAsync<T>(responseMessage, uri, "DELETE");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending DELETE request to {Uri}", uri);
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string uri, string authToken = "", Dictionary<string, string>? headers = null)
        {
            try
            {
                _logger.LogInformation("Sending DELETE request to {Uri}", uri);

                HttpClient httpClient = CreateHttpClient(uri, authToken, headers);

                var responseMessage = await ExecuteWithRetryAsync(() => httpClient.DeleteAsync(uri));

                if (responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogDebug("DELETE request to {Uri} succeeded", uri);
                }
                else
                {
                    _logger.LogWarning("DELETE request to {Uri} failed with status code: {StatusCode}",
                        uri, responseMessage.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending DELETE request to {Uri}", uri);
            }
        }

        /// <summary>
        /// Creates an instance of HttpClient with the provided authorization token and headers.
        /// </summary>
        /// <param name="authToken">The authorization token to include in the request headers.</param>
        /// <param name="headers">Additional headers to include in the request.</param>
        /// <param name="clientName">The name of the client to create from the factory.</param>
        /// <returns>An instance of HttpClient configured with the provided headers and authorization token.</returns>
        private HttpClient CreateHttpClient(string clientName, string authToken =  "", Dictionary<string, string>? headers = null)
        {
            var httpClient = _factory.CreateClient(clientName);

            httpClient.DefaultRequestHeaders.Accept.Clear();

            if (!string.IsNullOrEmpty(authToken))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", authToken);
            }

            if (headers != null)
            {
                foreach (var (key, value) in headers)
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
                }
            }

            return httpClient;
        }

        /// <summary>
        /// Creates a StringContent object for JSON request bodies.
        /// </summary>
        /// <typeparam name="T">The type of the data to serialize.</typeparam>
        /// <param name="data">The data to serialize.</param>
        /// <returns>A StringContent object with the serialized data and appropriate content type.</returns>
        private static StringContent CreateJsonContent<T>(T data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return content;
        }

        /// <summary>
        /// Processes the HTTP response and deserializes the content if successful.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response content to.</typeparam>
        /// <param name="response">The HTTP response message to process.</param>
        /// <param name="uri">The URI of the request.</param>
        /// <param name="method">The HTTP method used for the request.</param>
        /// <returns>The deserialized response object, or default if the response was not successful.</returns>
        private async Task<T?> ProcessResponseAsync<T>(HttpResponseMessage response, string uri, string method)
        {
            if (response.IsSuccessStatusCode)
            {
                var jsonResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.LogDebug("{Method} request to {Uri} succeeded with response: {Response}", method, uri, jsonResult);
                return JsonConvert.DeserializeObject<T>(jsonResult);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest)
            {
                var jsonResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.LogWarning("{Method} request to {Uri} returned {StatusCode} with response: {Response}",
                    method, uri, response.StatusCode, jsonResult);

                // Only try to deserialize if we have content
                if (!string.IsNullOrWhiteSpace(jsonResult))
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<T>(jsonResult);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize response for {Method} request to {Uri}", method, uri);
                    }
                }
            }
            else
            {
                _logger.LogError("{Method} request to {Uri} failed with status code: {StatusCode}",
                    method, uri, response.StatusCode);
            }

            return default;
        }

        /// <summary>
        /// Executes an HTTP request with a retry policy.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the HTTP request.</typeparam>
        /// <param name="action">The HTTP request action to execute.</param>
        /// <returns>The result of the HTTP request.</returns>
        private async Task<TResult> ExecuteWithRetryAsync<TResult>(Func<Task<TResult>> action)
        {
            return await Policy
                .Handle<HttpRequestException>() 
                .Or<TaskCanceledException>()
                .Or<WebException>()
                .WaitAndRetryAsync(
                    MaxRetryAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, timeSpan, retryCount, _) =>
                    {
                        _logger.LogWarning(ex,
                            "Request failed with {ExceptionType}. Retrying in {RetryTimeSpan}s. Attempt {RetryCount}/{MaxRetries}",
                            ex.GetType().Name, timeSpan.TotalSeconds, retryCount, MaxRetryAttempts);
                    })
                .ExecuteAsync(action);
        }
    }
}
