using Application.Abstractions.Authentication.KeyCloak;
using Domain.ExternalEntities.Dtos;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Infrastructure.Authentication.KeyCloak
{
    internal sealed class KeyCloakService : IKeyCloakService
    {
        private readonly HttpClient _httpClient;
        private readonly KeycloakOptions _keycloakOptions;
        private ILogger<KeyCloakService> _logger;

        public KeyCloakService(HttpClient httpClient, IOptions<KeycloakOptions> keycloakOptions,
            ILogger<KeyCloakService> logger)
        {
            _httpClient = httpClient;
            _keycloakOptions = keycloakOptions.Value;
            _logger = logger;
        }

        public async Task<ErrorOr<(string token, KeycloakUser user)>> AuthenticateUserAsync(string username, string password, CancellationToken cancellationToken)
        {
            try
            {
                // Step 1: Get access token
                var authRequestParameters = new KeyValuePair<string, string>[]
                {
                    new("client_id", _keycloakOptions.AuthClientId),
                    new("client_secret", _keycloakOptions.AuthClientSecret),
                    new("scope", "openid email"),
                    new("grant_type", "password"),
                    new("username", username),
                    new("password", password)
                };

                using var authorizationRequestContent = new FormUrlEncodedContent(authRequestParameters);

                HttpResponseMessage response = await _httpClient.PostAsync(
                    _keycloakOptions.TokenUrl,
                    authorizationRequestContent,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                AuthorizationToken? authorizationToken = await response
                    .Content
                    .ReadFromJsonAsync<AuthorizationToken>(cancellationToken);

                if (authorizationToken is null)
                {
                    _logger.LogError("Failed to acquire access token for user {Username}", username);
                    return Errors.Common.Validation("Keycloak.AuthenticationFailed", "Failed to acquire access token due to authentication failure");
                }

                // Step 2: Get user info by username //this step might be remove and return from identity user table as we only need to keep username, password, identityId and issuance of token.
                var userResult = await GetUserByUsernameAsync(username, cancellationToken);
                if (userResult.IsError)
                {
                    _logger.LogError("Failed to retrieve user information for {Username}: {Error}", username, userResult.Errors);
                    return userResult.Errors;
                }

                return (authorizationToken.AccessToken, userResult.Value);
            }
            catch (HttpRequestException)
            {
                return Errors.Infrastructure.DatabaseError("Keycloak.AuthenticationFailed", "Failed to process request");
            }
        }

        public async Task<ErrorOr<string>> TryAuthenticateUserAsync(KeycloakUser keycloakUser, CancellationToken cancellationToken)
        {
            var authParameters = new Dictionary<string, string>
            {
                ["client_id"] = _keycloakOptions.AuthClientId,
                ["client_secret"] = _keycloakOptions.AuthClientSecret,
                ["scope"] = "openid email",
                ["grant_type"] = "password",
                ["username"] = keycloakUser.Email,
                ["password"] = keycloakUser.Password
            };

            using var content = new FormUrlEncodedContent(authParameters);
            var response = await _httpClient.PostAsync(_keycloakOptions.TokenUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to authenticate user {Email}: {StatusCode}", keycloakUser.Email, response.StatusCode);
                return Errors.Common.Validation("Keycloak.AuthenticationFailed",
                    "Failed to authenticate user");
            }

            var token = await response.Content.ReadFromJsonAsync<AuthorizationToken>(cancellationToken);
            if (token?.AccessToken is null)
            {
                _logger.LogError("Failed to parse authentication token for user {Email}", keycloakUser.Email);
                return Errors.Common.Validation("Keycloak.TokenParsingFailed",
                    "Failed to parse authentication token");
            }

            return token.AccessToken;
        }

        public async Task<ErrorOr<string>> ProfileUserAsync(KeycloakUser keycloakUser, CancellationToken cancellationToken)
        {
            try
            {
                var authRequestParameters = new KeyValuePair<string, string>[]
                {
                    new("client_id", _keycloakOptions.AuthClientId),
                    new("client_secret", _keycloakOptions.AuthClientSecret),
                    new("scope", "openid email"),
                    new("grant_type", "password"),
                    new("username", keycloakUser.Email),
                    new("password", keycloakUser.Password)
                };

                using var authorizationRequestContent = new FormUrlEncodedContent(authRequestParameters);

                HttpResponseMessage response = await _httpClient.PostAsync(
                    "",
                    authorizationRequestContent,
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    // User already exists, return the existing user's access token
                    AuthorizationToken? existingToken = await response
                        .Content
                        .ReadFromJsonAsync<AuthorizationToken>(cancellationToken);
                    if (existingToken is null)
                    {
                        _logger.LogError("Failed to acquire access token for user {Email}", keycloakUser.Email);
                        return Errors.Common.Validation("Keycloak.AuthenticationFailed", "Failed to acquire access token due to authentication failure");
                    }
                    return existingToken.AccessToken;
                }
                else
                {
                    // User does not exist, create a new user
                    var createUserParameters = new KeyValuePair<string, string>[]
                    {
                        new("username", keycloakUser.Email),
                        new("email", keycloakUser.Email),
                        new("firstName", keycloakUser.FirstName),
                        new("lastName", keycloakUser.LastName),
                        new("enabled", "true"),
                        new("credentials[0].type", "password"),
                        new("credentials[0].value", keycloakUser.Password),
                        new("credentials[0].temporary", "false")
                    };
                    using var createUserContent = new FormUrlEncodedContent(createUserParameters);
                    HttpResponseMessage createResponse = await _httpClient.PostAsync(
                        $"{_keycloakOptions.AdminUrl}/users",
                        createUserContent,
                        cancellationToken);
                    if (!createResponse.IsSuccessStatusCode)
                    {
                        _logger.LogError("Failed to create user {Email}: {StatusCode}", keycloakUser.Email, createResponse.StatusCode);
                        return Errors.Common.Validation("Keycloak.UserCreationFailed", "Failed to create user in Keycloak");
                    }

                    // Return success message or token after user creation
                    return "User created successfully";
                }
            }
            catch (HttpRequestException)
            {
                _logger.LogCritical("Failed to process request for user {Email}", keycloakUser.Email);
                return Errors.Infrastructure.DatabaseError("Keycloak.AuthenticationFailed", "Failed to process request");
            }
        }

        public async Task<ErrorOr<string>> CreateUserAsync(KeycloakUser keycloakUser, CancellationToken cancellationToken)
        {
            try
            {
                // First, attempt to authenticate existing user
                var authToken = await TryAuthenticateUserAsync(keycloakUser, cancellationToken);
                if (authToken.IsError)
                {
                    // If authentication fails, try to create new user
                    var createResult = await CreateNewUserAsync(keycloakUser, cancellationToken);
                    if (createResult.IsError)
                    {
                        _logger.LogError("Failed to create user {Email}: {Error}", keycloakUser.Email, createResult.Errors);
                        return Errors.Common.Validation("Keycloak.UserCreationFailed",
                            "Failed to create user in Keycloak");
                    }

                    // After successful creation, authenticate the new user
                    //authToken = await TryAuthenticateUserAsync(keycloakUser, cancellationToken);
                    //if (authToken.IsError)
                    //{
                    //    _logger.LogError("User created but failed to authenticate {Email}: {Error}", keycloakUser.Email, authToken.Errors);
                    //    return Errors.Common.Validation("Keycloak.PostCreationAuthFailed",
                    //        "User created but failed to authenticate");
                    //}
                }

                return authToken.Value;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogCritical(ex, "Failed to process Keycloak request for user {Email}", keycloakUser.Email);
                return Errors.Infrastructure.DatabaseError("Keycloak.RequestFailed",
                    $"Failed to process Keycloak request: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                _logger.LogCritical("Keycloak request timed out for user {Email}", keycloakUser.Email);
                return Errors.Infrastructure.DatabaseError("Keycloak.Timeout",
                    "Request to Keycloak timed out");
            }
        }

        public async Task<ErrorOr<KeycloakUser>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            try
            {
                // Step 1: Get admin access token   
                var tokenRequestParameters = new KeyValuePair<string, string>[]
                {
                    new("client_id", _keycloakOptions.AdminClientId),
                    new("client_secret", _keycloakOptions.AdminClientSecret),
                    new("grant_type", "client_credentials")
                };

                using var tokenRequestContent = new FormUrlEncodedContent(tokenRequestParameters);

                HttpResponseMessage tokenResponse = await _httpClient.PostAsync(
                    _keycloakOptions.TokenUrl,
                    tokenRequestContent,
                    cancellationToken);

                tokenResponse.EnsureSuccessStatusCode();

                AuthorizationToken? adminToken = await tokenResponse
                    .Content
                    .ReadFromJsonAsync<AuthorizationToken>(cancellationToken);

                if (adminToken is null || string.IsNullOrWhiteSpace(adminToken.AccessToken))
                {
                    _logger.LogError("Failed to acquire admin access token for Keycloak.");
                    return Errors.Common.Validation("Keycloak.AdminTokenFailed", "Failed to acquire admin access token");
                }

                // Step 2: Query user by username
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{_keycloakOptions.AdminUrl}/users?username={Uri.EscapeDataString(username)}");

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken.AccessToken);

                HttpResponseMessage userResponse = await _httpClient.SendAsync(request, cancellationToken);
                userResponse.EnsureSuccessStatusCode();

                var users = await userResponse.Content.ReadFromJsonAsync<List<KeycloakUser>>(cancellationToken);
                if (users == null || users.Count == 0)
                {
                    _logger.LogWarning("No users found with username {Username} in Keycloak.", username);
                    return Errors.Common.NotFound("Keycloak.UserNotFound", $"User '{username}' not found in Keycloak.");
                }

                // Return the first matched user
                return users[0];
            }
            catch (HttpRequestException)
            {
                _logger.LogCritical("Failed to process request to Keycloak for user {Username}", username);
                return Errors.Infrastructure.DatabaseError("Keycloak.UserFetchFailed", "Failed to process request to Keycloak.");
            }
        }

        public async Task<ErrorOr<string>> GetAccessTokenAsync(string email, string password, CancellationToken cancellationToken)
        {
            try
            {
                var authRequestParameters = new KeyValuePair<string, string>[]
                {
                new("client_id", _keycloakOptions.AuthClientId),
                new("client_secret", _keycloakOptions.AuthClientSecret),
                new("scope", "openid email"),
                new("grant_type", "password"),
                new("username", email),
                new("password", password)
                };

                using var authorizationRequestContent = new FormUrlEncodedContent(authRequestParameters);

                HttpResponseMessage response = await _httpClient.PostAsync(
                    "",
                    authorizationRequestContent,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                AuthorizationToken? authorizationToken = await response
                    .Content
                    .ReadFromJsonAsync<AuthorizationToken>(cancellationToken);

                if (authorizationToken is null)
                {
                    _logger.LogError("Failed to acquire access token for user {Email}", email);
                    return Errors.Common.Validation("Keycloak.AuthenticationFailed", "Failed to acquire access token do to authentication failure");
                }

                return authorizationToken.AccessToken;
            }
            catch (HttpRequestException)
            {
                _logger.LogCritical("Failed to process request for user {Email}", email);
                return Errors.Infrastructure.DatabaseError("Keycloak.AuthenticationFailed", "Failed to process request");
            }
        }

        private async Task<ErrorOr<bool>> CreateNewUserAsync(KeycloakUser keycloakUser, CancellationToken cancellationToken)
        {
            // Get admin token first
            var adminToken = await GetAdminTokenAsync(cancellationToken);
            if (adminToken.IsError)
            {
                return adminToken.Errors;
            }

            var userPayload = new
            {
                username = keycloakUser.Email,
                email = keycloakUser.Email,
                firstName = keycloakUser.FirstName,
                lastName = keycloakUser.LastName,
                enabled = true,
                credentials = new[]
                {
            new
            {
                type = "password",
                value = keycloakUser.Password,
                temporary = false
            }
        }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_keycloakOptions.AdminUrl}/users");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken.Value);
            request.Content = JsonContent.Create(userPayload);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create user {Email} in Keycloak: {StatusCode}", keycloakUser.Email, response.StatusCode);
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return Errors.Common.Validation("Keycloak.UserCreationFailed",
                    $"Failed to create user: {response.StatusCode} - {errorContent}");
            }

            return true;
        }

        private async Task<ErrorOr<string>> GetAdminTokenAsync(CancellationToken cancellationToken)
        {
            var adminAuthParameters = new Dictionary<string, string>
            {
                ["client_id"] = _keycloakOptions.AdminClientId,
                ["client_secret"] = _keycloakOptions.AdminClientSecret,
                ["grant_type"] = "client_credentials"
            };

            using var content = new FormUrlEncodedContent(adminAuthParameters);
            var response = await _httpClient.PostAsync(_keycloakOptions.TokenUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get admin token: {StatusCode}", response.StatusCode);
                return Errors.Common.Validation("Keycloak.AdminAuthFailed",
                    "Failed to get admin token");
            }

            var token = await response.Content.ReadFromJsonAsync<AuthorizationToken>(cancellationToken);
            if (token?.AccessToken is null)
            {
                _logger.LogError("Failed to parse admin token from Keycloak response.");
                return Errors.Common.Validation("Keycloak.AdminTokenParsingFailed",
                    "Failed to parse admin token");
            }

            return token.AccessToken;
        }

    
    // Send verification email for a user
public async Task<ErrorOr<bool>> SendVerificationEmailAsync(
    string keycloakUserId,
    CancellationToken cancellationToken)
        {
            try
            {
                var adminToken = await GetAdminTokenAsync(cancellationToken);
                if (adminToken.IsError)
                    return adminToken.Errors;

                using var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"{_keycloakOptions.AdminUrl}/users/{keycloakUserId}/send-verify-email");

                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", adminToken.Value);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError(
                        "Failed to send verification email for {UserId}: {Error}",
                        keycloakUserId, error);
                    return Errors.Common.Validation(
                        "Keycloak.VerificationEmailFailed",
                        "Failed to send verification email");
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogCritical(ex,
                    "Failed to send verification email for {UserId}", keycloakUserId);
                return Errors.Infrastructure.DatabaseError(
                    "Keycloak.VerificationEmailFailed",
                    "Failed to send verification email");
            }
        }

        // Forgot password — sends reset email
        public async Task<ErrorOr<bool>> SendForgotPasswordEmailAsync(
            string keycloakUserId,
            CancellationToken cancellationToken)
        {
            try
            {
                var adminToken = await GetAdminTokenAsync(cancellationToken);
                if (adminToken.IsError)
                    return adminToken.Errors;

              
                var actions = new[] { "UPDATE_PASSWORD" };

                using var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"{_keycloakOptions.AdminUrl}/users/{keycloakUserId}/execute-actions-email");

                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", adminToken.Value);
                request.Content = JsonContent.Create(actions);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError(
                        "Failed to send password reset email for {UserId}: {Error}",
                        keycloakUserId, error);
                    return Errors.Common.Validation(
                        "Keycloak.ForgotPasswordFailed",
                        "Failed to send password reset email");
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogCritical(ex,
                    "Failed to send password reset email for {UserId}", keycloakUserId);
                return Errors.Infrastructure.DatabaseError(
                    "Keycloak.ForgotPasswordFailed",
                    "Failed to process request");
            }
        }

        // Change password — directly sets new password
        public async Task<ErrorOr<bool>> ChangePasswordAsync(
            string keycloakUserId,
            string newPassword,
            CancellationToken cancellationToken)
        {
            try
            {
                var adminToken = await GetAdminTokenAsync(cancellationToken);
                if (adminToken.IsError)
                    return adminToken.Errors;

                var passwordPayload = new
                {
                    type = "password",
                    value = newPassword,
                    temporary = false
                };

                using var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"{_keycloakOptions.AdminUrl}/users/{keycloakUserId}/reset-password");

                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", adminToken.Value);
                request.Content = JsonContent.Create(passwordPayload);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError(
                        "Failed to change password for {UserId}: {Error}",
                        keycloakUserId, error);
                    return Errors.Common.Validation(
                        "Keycloak.ChangePasswordFailed",
                        "Failed to change password");
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogCritical(ex,
                    "Failed to change password for {UserId}", keycloakUserId);
                return Errors.Infrastructure.DatabaseError(
                    "Keycloak.ChangePasswordFailed",
                    "Failed to process request");
            }
        }

        // Update user — updates Keycloak AND your DB
        public async Task<ErrorOr<bool>> UpdateUserAsync(
            string keycloakUserId,
            KeycloakUser updatedUser,
            CancellationToken cancellationToken)
        {
            try
            {
                var adminToken = await GetAdminTokenAsync(cancellationToken);
                if (adminToken.IsError)
                    return adminToken.Errors;

                var userPayload = new
                {
                    email = updatedUser.Email,
                    firstName = updatedUser.FirstName,
                    lastName = updatedUser.LastName,
                    username = updatedUser.Email
                };

                using var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"{_keycloakOptions.AdminUrl}/users/{keycloakUserId}");

                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", adminToken.Value);
                request.Content = JsonContent.Create(userPayload);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    // ✅ Read exact error from Keycloak
                    var errorContent = await response.Content
                        .ReadAsStringAsync(cancellationToken);

                    _logger.LogError(
                        "Keycloak change password failed. " +
                        "Status: {Status} " +
                        "UserId: {UserId} " +
                        "Error: {Error}",
                        response.StatusCode,
                        keycloakUserId,
                        errorContent);

                    return Errors.Common.Validation(
                        "Keycloak.ChangePasswordFailed",
                        errorContent); 
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogCritical(ex,
                    "Failed to update user {UserId}", keycloakUserId);
                return Errors.Infrastructure.DatabaseError(
                    "Keycloak.UpdateUserFailed",
                    "Failed to process request");
            }
        }

        // Delete user — removes from Keycloak AND your DB
        public async Task<ErrorOr<bool>> DeleteUserAsync(
            string keycloakUserId,
            CancellationToken cancellationToken)
        {
            try
            {
                var adminToken = await GetAdminTokenAsync(cancellationToken);
                if (adminToken.IsError)
                    return adminToken.Errors;

                using var request = new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"{_keycloakOptions.AdminUrl}/users/{keycloakUserId}");

                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", adminToken.Value);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError(
                        "Failed to delete user {UserId}: {Error}",
                        keycloakUserId, error);
                    return Errors.Common.Validation(
                        "Keycloak.DeleteUserFailed",
                        "Failed to delete user");
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogCritical(ex,
                    "Failed to delete user {UserId}", keycloakUserId);
                return Errors.Infrastructure.DatabaseError(
                    "Keycloak.DeleteUserFailed",
                    "Failed to process request");
            }
        }

    }
}
