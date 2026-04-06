using Application.Abstractions.Authentication.KeyCloak;
using Application.Abstractions.Data;
using Application.Abstractions.EntityRepositories.Users;
using Application.Abstractions.Messaging;
using Application.Abstractions.Authentication.Custom;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Handlers.Commands.Users.DeleteUser;

internal sealed class DeleteUserCommandHandler(
    IUserRespository userRepository,
    IUnitOfWork unitOfWork,
    IKeyCloakService keyCloakService,
    IUserContext userContext,
    ILogger<DeleteUserCommandHandler> logger)
    : ICommandHandler<DeleteUserCommand, string>
{
    public async Task<ErrorOr<string>> Handle(
        DeleteUserCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            
            if (string.IsNullOrWhiteSpace(command.UserId))
            {
                return Errors.Common.Validation(
                    "UserId",
                    "UserId cannot be empty");
            }

           
            var requestingUserKeycloakId = userContext.IdentityId;
            if (string.IsNullOrWhiteSpace(requestingUserKeycloakId))
            {
                return Errors.Common.Unauthorized(
                    "User",
                    "User identity not found. Please login again.");
            }

            
            var userToDelete = await userRepository
                .GetByIdAsync(command.UserId, cancellationToken);

            if (userToDelete is null)
            {
                logger.LogWarning(
                    "User {UserId} not found for deletion", command.UserId);
                return Errors.Common.NotFound(
                    "User",
                    $"User with ID {command.UserId} not found");
            }

            
            if (userToDelete.KeycloakId == requestingUserKeycloakId)
            {
                logger.LogWarning(
                    "User {UserId} attempted to delete their own account",
                    command.UserId);
                return Errors.Common.Validation(
                    "DeleteUser",
                    "You cannot delete your own account");
            }

           
            if (userToDelete.IsDeleted)
            {
                return Errors.Common.Validation(
                    "DeleteUser",
                    "User has already been deleted");
            }

        
            var keycloakResult = await keyCloakService.DeleteUserAsync(
                userToDelete.KeycloakId,
                cancellationToken);

            if (keycloakResult.IsError)
            {
                logger.LogError(
                    "Failed to delete user {Email} from Keycloak: {Errors}",
                    userToDelete.Email, keycloakResult.Errors);
                return keycloakResult.Errors;
            }

            logger.LogInformation(
                "User {Email} deleted from Keycloak successfully",
                userToDelete.Email);

            userToDelete.IsDeleted = true;
            userToDelete.IsAppEnabled = false;
            userToDelete.DeletedAt = DateTime.UtcNow;
            userToDelete.DeletedBy = userContext.UserName;

            await userRepository.UpdateAsync(userToDelete, cancellationToken);

            logger.LogInformation(
                "User {Email} soft deleted in app DB. " +
                "DeletedBy: {DeletedBy}",
                userToDelete.Email,
                userContext.UserName);

            return $"User {userToDelete.Email} has been deleted successfully.";
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Error deleting user {UserId}", command.UserId);
            return Errors.Infrastructure.DatabaseError(
                "DeleteUser.Failed",
                ex.ToString());
        }
    }
}