using Domain.ExternalEntities.Dtos;
using ErrorOr;

namespace Application.Abstractions.Authentication.KeyCloak
{
    public interface IKeyCloakService
    {
        Task<ErrorOr<string>> GetAccessTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

        Task<ErrorOr<string>> CreateUserAsync(KeycloakUser keycloakUser, CancellationToken cancellationToken);
        Task<ErrorOr<string>> ProfileUserAsync(KeycloakUser keycloakUser, CancellationToken cancellationToken);

        Task<ErrorOr<(string token, KeycloakUser user)>> AuthenticateUserAsync(string username, string password, CancellationToken cancellationToken);
        Task<ErrorOr<string>> TryAuthenticateUserAsync(KeycloakUser keycloakUser, CancellationToken cancellationToken);

        Task<ErrorOr<KeycloakUser>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken);
        Task<ErrorOr<bool>> SendVerificationEmailAsync(
       string keycloakUserId, CancellationToken cancellationToken);
        Task<ErrorOr<bool>> SendForgotPasswordEmailAsync(
            string keycloakUserId, CancellationToken cancellationToken);
        Task<ErrorOr<bool>> ChangePasswordAsync(
            string keycloakUserId, string newPassword, CancellationToken cancellationToken);
        Task<ErrorOr<bool>> UpdateUserAsync(
            string keycloakUserId, KeycloakUser updatedUser, CancellationToken cancellationToken);
        Task<ErrorOr<bool>> DeleteUserAsync(
            string keycloakUserId, CancellationToken cancellationToken);
    }
}
