using Application.Abstractions.Authentication.Custom;
using Application.Abstractions.Authentication.KeyCloak;
using Application.Abstractions.Data;
using Application.Abstractions.EntityRepositories.Users;
using Application.Abstractions.Messaging;
using Domain.ExternalEntities.Dtos;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Handlers.Commands.Users.UpdateUser;

internal sealed class UpdateUserCommandHandler(
    IUserRespository userRepository,
    IUnitOfWork unitOfWork,
    IKeyCloakService keyCloakService,
    IUserContext userContext,
    ILogger<UpdateUserCommandHandler> logger)
    : ICommandHandler<UpdateUserCommand, string>
{
    public async Task<ErrorOr<string>> Handle(
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: Validate at least one field is being updated
               if( string.IsNullOrWhiteSpace(command.FirstName))
                {
                return Errors.Common.Validation(
                    "FirstName",
                    "At least one field must be provided to update");
            }

           if( string.IsNullOrWhiteSpace(command.LastName))
            {
                return Errors.Common.Validation(
                    "LastName",
                    "At least one field must be provided to update");
            }

            // Step 2: Get current user from JWT token
            var keycloakId = userContext.IdentityId;
            if (string.IsNullOrWhiteSpace(keycloakId))
            {
                logger.LogWarning("UserId not found in token claims.");
                return Errors.Common.Unauthorized(
                    "User",
                    "User identity not found. Please login again.");
            }

            
            var user = await userRepository
                .GetByKeycloakIdAsync(keycloakId, cancellationToken);

            if (user is null)
            {
                logger.LogWarning(
                    "User not found for KeycloakId {KeycloakId}", keycloakId);
                return Errors.Common.NotFound(
                    "User",
                    "User not found");
            }

            
            if (user.IsDeleted || !user.IsAppEnabled)
            {
                return Errors.Common.Unauthorized(
                    "User",
                    "Your account has been disabled. Kindly contact admin.");
            }

            
            bool keycloakNeedsUpdate =
                !string.IsNullOrWhiteSpace(command.FirstName) ||
                !string.IsNullOrWhiteSpace(command.LastName);

            if (keycloakNeedsUpdate)
            {
                var keycloakUser = new KeycloakUser
                {
                    Email = user.Email,
                    FirstName = command.FirstName ?? user.FirstName,
                    LastName = command.LastName ?? user.LastName
                };

                var keycloakResult = await keyCloakService.UpdateUserAsync(
                    user.KeycloakId,
                    keycloakUser,
                    cancellationToken);

                if (keycloakResult.IsError)
                {
                    logger.LogError(
                        "Failed to update user in Keycloak for {Email}: {Errors}",
                        user.Email, keycloakResult.Errors);
                    return keycloakResult.Errors;
                }
            }

            
            if (!string.IsNullOrWhiteSpace(command.FirstName))
                user.FirstName = command.FirstName.Trim();

            if (!string.IsNullOrWhiteSpace(command.LastName))
                user.LastName = command.LastName.Trim();


            await userRepository.UpdateAsync(user, cancellationToken);

            logger.LogInformation(
                "User {Email} updated successfully. KeycloakId: {KeycloakId}",
                user.Email, user.KeycloakId);

            return "User updated successfully.";
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error updating user");
            return Errors.Infrastructure.DatabaseError(
                "UpdateUser.Failed",
                ex.ToString());
        }
    }
}