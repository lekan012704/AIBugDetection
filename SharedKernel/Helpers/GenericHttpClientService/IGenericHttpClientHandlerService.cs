namespace SharedKernel.Helpers.GenericHttpClientService
{
    /// <summary>
    /// Provides standardized HTTP client methods for making API requests with proper error handling and logging.
    /// </summary>
    public interface IGenericHttpClientHandlerService
    {
        /// <summary>
        /// Sends a GET request to the specified URI and returns the deserialized response.
        /// </summary>
        /// <typeparam name="T">The type of the expected response object.</typeparam>
        /// <param name="uri">The URI to send the request to.</param>
        /// <param name="authToken">The authorization token to include in the request headers.</param>
        /// <param name="headers">Additional headers to include in the request.</param>
        /// <returns>The deserialized response object, or default if the request fails.</returns>
        Task<T?> GetAsync<T>(string uri, string authToken = "", Dictionary<string, string>? headers = null);

        /// <summary>
        /// Sends a POST request to the specified URI with the provided data and returns the deserialized response.
        /// </summary>
        /// <typeparam name="TRequest">The type of the data being sent.</typeparam>
        /// <typeparam name="TResponse">The type of the expected response object.</typeparam>
        /// <param name="uri">The URI to send the request to.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <param name="authToken">The authorization token to include in the request headers.</param>
        /// <param name="headers">Additional headers to include in the request.</param>
        /// <returns>The deserialized response object, or default if the request fails.</returns>
        Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data, string authToken = "", Dictionary<string, string>? headers = null);

        /// <summary>
        /// Sends a PUT request to the specified URI with the provided data and returns the deserialized response.
        /// </summary>
        /// <typeparam name="TRequest">The type of the data being sent.</typeparam>
        /// <typeparam name="TResponse">The type of the expected response object.</typeparam>
        /// <param name="uri">The URI to send the request to.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <param name="authToken">The authorization token to include in the request headers.</param>
        /// <param name="headers">Additional headers to include in the request.</param>
        /// <returns>The deserialized response object, or default if the request fails.</returns>
        Task<TResponse?> PutAsync<TRequest, TResponse>(string uri, TRequest data, string authToken = "", Dictionary<string, string>? headers = null);

        /// <summary>
        /// Sends a DELETE request to the specified URI and returns the deserialized response.
        /// </summary>
        /// <typeparam name="T">The type of the expected response object.</typeparam>
        /// <param name="uri">The URI to send the request to.</param>
        /// <param name="authToken">The authorization token to include in the request headers.</param>
        /// <param name="headers">Additional headers to include in the request.</param>
        /// <returns>The deserialized response object, or default if the request fails.</returns>
        Task<T?> DeleteAsync<T>(string uri, string authToken = "", Dictionary<string, string>? headers = null);

        /// <summary>
        /// Sends a DELETE request to the specified URI.
        /// </summary>
        /// <param name="uri">The URI to send the request to.</param>
        /// <param name="authToken">The authorization token to include in the request headers.</param>
        /// <param name="headers">Additional headers to include in the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(string uri, string authToken = "", Dictionary<string, string>? headers = null);
    }
}
