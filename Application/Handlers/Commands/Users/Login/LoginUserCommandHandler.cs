using Application.Abstractions.Authentication.KeyCloak;
using Application.Abstractions.Authorization;
using Application.Abstractions.EntityRepositories.Users;
using Application.Abstractions.Messaging;
using Domain.Application.Entities.Users;
using Domain.ExternalEntities.Dtos;
using Domain.Models;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Handlers.Commands.Users.Login;

internal sealed class LoginUserCommandHandler(
    IUserRespository userRepository,
    IKeyCloakService keyCloakService,
    IPermissionRepository permissionRepository,
    ILogger<LoginUserCommandHandler> logger)
    : ICommandHandler<LoginUserCommand, MessageClass>
{
    public async Task<ErrorOr<MessageClass>> Handle(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: Basic validation
            if (string.IsNullOrWhiteSpace(command.Email))
            {
                logger.LogWarning("Email cannot be empty for login attempt.");
                return Errors.Common.Validation("Email", "Email cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(command.Password))
            {
                logger.LogWarning("Password cannot be empty for {Email}.", command.Email);
                return Errors.Common.Validation("Password", "Password cannot be empty");
            }

            // Step 2: Authenticate against Keycloak
            // Keycloak validates password — we never touch the password after this point
            var keycloakResult = await keyCloakService.AuthenticateUserAsync(
                command.Email.Trim(),
                command.Password.Trim(),
                cancellationToken);

            if (keycloakResult.IsError)
            {
                logger.LogWarning(
                    "Keycloak authentication failed for {Email}: {Errors}",
                    command.Email, keycloakResult.Errors);
                return Errors.Common.Unauthorized("User", "Invalid login attempt.");
            }

            // Step 3: Destructure Keycloak result
            var (keycloakToken, keycloakUser) = keycloakResult.Value;

            // Step 4: Find user in YOUR DB by KeycloakId
            var user = await userRepository.GetByKeycloakIdAsync(
                keycloakUser.IdentityId, cancellationToken);

            if (user is null)
            {
                // First time login — auto provision app profile
                logger.LogInformation(
                    "First login detected for KeycloakId {KeycloakId}. Creating app profile.",
                    keycloakUser.IdentityId);

                var newUser = new User
                {
                    KeycloakId = keycloakUser.IdentityId,
                    Email = command.Email,
                    FirstName = keycloakUser.FirstName,
                    LastName = keycloakUser.LastName,
                    UserName = command.Email,
                    IsAppEnabled = true,
                    IsDeleted = false,
                    FirstLoginAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                await userRepository.AddAsync(newUser, cancellationToken);
                await userRepository.SaveChangesAsync(cancellationToken);

                user = newUser;
            }
            else
            {
                // Returning user — update last login
                user.LastLoginAt = DateTime.UtcNow;
                await userRepository.UpdateAsync(user, cancellationToken);
            }

            // Step 5: Check if user is enabled in YOUR app
            // Note: Keycloak handles its own enabled/locked state
            // This is your app-level disable e.g. banned by admin
            if (!user.IsAppEnabled || user.IsDeleted)
            {
                logger.LogWarning(
                    "User {Email} is disabled or deleted in the app.",
                    command.Email);
                return Errors.Common.Unauthorized(
                    "User",
                    "Your account has been disabled. Kindly contact admin.");
            }

            // Step 6: Get permissions from YOUR DB
            var userPermissions = await permissionRepository
                .GetPermissionsByUserIdAsync(user.Id);

            //if (!userPermissions.Any())
            //{
            //    logger.LogWarning(
            //        "User {Email} has no permissions assigned.", command.Email);
            //    return Errors.Common.NotFound(
            //        "User",
            //        $"User {command.Email} has no permissions assigned. Kindly contact admin.");
            //}

            // Step 7: Get roles from YOUR DB
            var userRoles = await permissionRepository.GetRolesForUserAsync(user.Id);

            //if (userRoles is null || !userRoles.Permissions.Any())
            //{
            //    logger.LogWarning(
            //        "User {Email} has no roles assigned.", command.Email);
            //    return Errors.Common.NotFound(
            //        "User",
            //        $"User {command.Email} has no roles assigned. Kindly contact admin.");
            //}

            // Step 8: Build user profile
            var userProfile = new UserProfile
            {
                Id = user.Id,
                IdentityId = user.KeycloakId,
                UserName = user.UserName ?? user.Email,
                Email = user.Email, 
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePicture = user.ProfilePicture
            };

            // Step 9: Build response using Keycloak token directly
            // No need to create our own JWT — Keycloak already issued one
            var userLoginResponse = new UserLoginResponse
            {
                Token = keycloakToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                RefreshToken = null,
                RefreshTokenExpiresAt = DateTime.Now,
                UserName = user.UserName ?? user.Email,
                User = userProfile,
                Permissions = userPermissions
            };

            logger.LogInformation(
                "User {Email} logged in. AppId: {Id} KeycloakId: {KeycloakId}",
                command.Email, user.Id, user.KeycloakId);

            return new MessageClass
            {
                Data = userLoginResponse,
                Message = "Login successful",
                StatusId = 1
            };
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Error processing login for {Email}", command.Email);
            return Errors.Infrastructure.DatabaseError(
                $"Failed to process login request {command.Email}", ex.ToString());
        }
    }
}