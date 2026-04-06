using Application.Abstractions.Authentication.KeyCloak;
using Application.Abstractions.EntityRepositories.Users;
using Application.Abstractions.Messaging;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Handlers.Commands.Users.ForgotPassword;

internal sealed class ForgotPasswordCommandHandler(
    IUserRespository userRepository,
    IKeyCloakService keyCloakService,
    ILogger<ForgotPasswordCommandHandler> logger)
    : ICommandHandler<ForgotPasswordCommand, string>
{
    public async Task<ErrorOr<string>> Handle(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
           
            if (string.IsNullOrWhiteSpace(command.Email))
            {
                logger.LogWarning("Email cannot be empty for forgot password.");
                return Errors.Common.Validation(
                    "Email",
                    "Email cannot be empty");
            }

       
            var user = await userRepository
                .GetByUserEmailAsync(command.Email.Trim(), cancellationToken);

            if (user is null)
            {
              
                logger.LogWarning(
                    "Forgot password requested for non-existent email {Email}",
                    command.Email);

                return "If this email exists you will receive a password reset link shortly.";
            }

            if (user.IsDeleted || !user.IsAppEnabled)
            {
                logger.LogWarning(
                    "Forgot password requested for disabled user {Email}",
                    command.Email);

      
                return "If this email exists you will receive a password reset link shortly.";
            }

            
            var result = await keyCloakService.SendForgotPasswordEmailAsync(
                user.KeycloakId,
                cancellationToken);

            if (result.IsError)
            {
                logger.LogError(
                    "Failed to send forgot password email for {Email}: {Errors}",
                    command.Email, result.Errors);

                return Errors.Infrastructure.DatabaseError(
                    "ForgotPassword.Failed",
                    "Failed to send password reset email. Please try again.");
            }

            logger.LogInformation(
                "Forgot password email sent for {Email}", command.Email);

            return "If this email exists you will receive a password reset link shortly.";
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Error processing forgot password for {Email}", command.Email);
            return Errors.Infrastructure.DatabaseError(
                "ForgotPassword.Failed",
                ex.ToString());
        }
    }
}