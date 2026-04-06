using Application.Abstractions.Authentication.KeyCloak;
using Application.Abstractions.EntityRepositories.Users;
using Application.Abstractions.Messaging;
using Application.Abstractions.Authentication.Custom;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Handlers.Commands.Users.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    IUserRespository userRepository,
    IKeyCloakService keyCloakService,
    IUserContext userContext,
    ILogger<ChangePasswordCommandHandler> logger)
    : ICommandHandler<ChangePasswordCommand, string>
{
    public async Task<ErrorOr<string>> Handle(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            
            if (string.IsNullOrWhiteSpace(command.CurrentPassword))
                return Errors.Common.Validation(
                    "CurrentPassword",
                    "Current password cannot be empty");

            if (string.IsNullOrWhiteSpace(command.NewPassword))
                return Errors.Common.Validation(
                    "NewPassword",
                    "New password cannot be empty");

            if (string.IsNullOrWhiteSpace(command.ConfirmNewPassword))
                return Errors.Common.Validation(
                    "ConfirmNewPassword",
                    "Confirm password cannot be empty");

           
            if (command.NewPassword != command.ConfirmNewPassword)
            {
                logger.LogWarning("New password and confirm password do not match.");
                return Errors.Common.Validation(
                    "ConfirmNewPassword",
                    "New password and confirm password do not match");
            }

            
            if (command.CurrentPassword == command.NewPassword)
            {
                return Errors.Common.Validation(
                    "NewPassword",
                    "New password must be different from current password");
            }

           
            var passwordValidation = ValidatePasswordStrength(command.NewPassword);
            if (passwordValidation.IsError)
                return passwordValidation.Errors;


            var keycloakId = userContext.IdentityId;

            if (string.IsNullOrWhiteSpace(keycloakId))
            {
                logger.LogWarning("IdentityId not found in token claims.");
                return Errors.Common.Unauthorized(
                    "User",
                    "User identity not found. Please login again.");
            }

            
            var user = await userRepository
                .GetByKeycloakIdAsync(keycloakId, cancellationToken);

            if (user is null)
            {
                logger.LogWarning(
                    "User not found for KeycloakId {keycloakId}", keycloakId);
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

           
            var verifyResult = await keyCloakService.AuthenticateUserAsync(
                user.Email,
                command.CurrentPassword.Trim(),
                cancellationToken);

            if (verifyResult.IsError)
            {
                logger.LogWarning(
                    "Current password verification failed for {Email}",
                    user.Email);
                return Errors.Common.Validation(
                    "CurrentPassword",
                    "Current password is incorrect");
            }

           
            var changeResult = await keyCloakService.ChangePasswordAsync(
                user.KeycloakId,
                command.NewPassword.Trim(),
                cancellationToken);

            if (changeResult.IsError)
            {
                logger.LogError(
                    "Failed to change password for {Email}: {Errors}",
                    user.Email, changeResult.Errors);
                return changeResult.Errors;
            }

            logger.LogInformation(
                "Password changed successfully for {Email}", user.Email);

            return "Password changed successfully. Please login with your new password.";
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Error changing password for user");
            return Errors.Infrastructure.DatabaseError(
                "ChangePassword.Failed",
                ex.ToString());
        }
    }
        

    private static ErrorOr<string> ValidatePasswordStrength(string password)
    {
        if (password.Length < 8)
            return Errors.Common.Validation(
                "NewPassword",
                "Password must be at least 8 characters long");

        if (!password.Any(char.IsUpper))
            return Errors.Common.Validation(
                "NewPassword",
                "Password must contain at least one uppercase letter");

        if (!password.Any(char.IsLower))
            return Errors.Common.Validation(
                "NewPassword",
                "Password must contain at least one lowercase letter");

        if (!password.Any(char.IsDigit))
            return Errors.Common.Validation(
                "NewPassword",
                "Password must contain at least one number");

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            return Errors.Common.Validation(
                "NewPassword",
                "Password must contain at least one special character");

        return "valid";
    }
}