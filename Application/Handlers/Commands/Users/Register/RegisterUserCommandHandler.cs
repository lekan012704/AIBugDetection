using Application.Abstractions.Authentication.KeyCloak;
using Application.Abstractions.Data;
using Application.Abstractions.EntityRepositories.Users;
using Application.Abstractions.Messaging;
using Domain.Application.Entities.Users;
using Domain.ExternalEntities.Dtos;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System.Diagnostics.Metrics;
using System.Runtime.Intrinsics.Arm;

namespace Application.Handlers.Commands.Users.Register;

internal sealed class RegisterUserCommandHandler(
    IUserRespository userRepository,
    IUnitOfWork unitOfWork,
    IKeyCloakService keyCloakService,
    IDateTimeProvider dateTimeProvider,
    ILogger<RegisterUserCommandHandler> logger)
    : ICommandHandler<RegisterUserCommand, string>
{
    public async Task<ErrorOr<string>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: Check email doesn't already exist in YOUR DB
            if (await userRepository.GetAnyByUserEmailAsync(
                command.Email, cancellationToken))
            {
                logger.LogError(
                    "Email {Email} already exists in app DB.", command.Email);
                return Errors.Common.Conflict("User",
                    $"The provided email is not unique {command.Email}");
            }

            // Step 2: Create user in Keycloak
            var keycloakUser = new KeycloakUser
            {
                Email = command.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                Password = command.Password
            };

            var keycloakResult = await keyCloakService
                .CreateUserAsync(keycloakUser, cancellationToken);

            if (keycloakResult.IsError)
            {
                logger.LogError(
                    "Failed to create user in Keycloak for {Email}: {Errors}",
                    command.Email, keycloakResult.Errors);
                return keycloakResult.Errors;
            }

            // Step 3: Get the KeycloakId for the newly created user
            var keycloakUserDetails = await keyCloakService
                .GetUserByUsernameAsync(command.Email, cancellationToken);

            if (keycloakUserDetails.IsError)
            {
                logger.LogError(
                    "User created in Keycloak but failed to retrieve ID for {Email}",
                    command.Email);
                return keycloakUserDetails.Errors;
            }

            // Step 4: Validate KeycloakId is not empty
            if (string.IsNullOrWhiteSpace(keycloakUserDetails.Value.IdentityId))
            {
                logger.LogError(
                    "Keycloak returned empty ID for user {Email}", command.Email);
                return Errors.Common.Validation(
                    "Keycloak",
                    "Failed to retrieve user identity from Keycloak.");
            }

            // Step 5: Save user profile in YOUR DB
            var user = new User
            {
                KeycloakId = keycloakUserDetails.Value.IdentityId,
                Email = command.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                UserName = command.Email,
                IsAppEnabled = true,
                IsDeleted = false,
                FirstLoginAt = null,
                LastLoginAt = null,
                CreatedBy = "System",
                CreatedAt = dateTimeProvider.UtcNow
            };

            await userRepository.AddAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

            logger.LogInformation(
                "User registered. AppId: {Id} KeycloakId: {KeycloakId}",
                user.Id, user.KeycloakId);

            // Step 6: Auto-send verification email
            // ✅ Don't fail registration if email fails — just log warning
            var verificationResult = await keyCloakService
                .SendVerificationEmailAsync(
                    keycloakUserDetails.Value.IdentityId,
                    cancellationToken);

            if (verificationResult.IsError)
            {

                logger.LogWarning(
                    "User {Email} registered but verification email failed: {Errors}",
                    command.Email, verificationResult.Errors);
            }
            else
            {
                logger.LogInformation(
                    "Verification email sent to {Email}", command.Email);
            }

            return user.Id;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Error registering user {Email}", command.Email);
            return Errors.Infrastructure.DatabaseError(
                $"Failed to process request {command.Email}", ex.ToString());
        }
    }
}
