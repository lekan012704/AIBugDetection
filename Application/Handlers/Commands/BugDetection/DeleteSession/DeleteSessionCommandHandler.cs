using Application.Abstractions.Authentication.Custom;
using Application.Abstractions.EntityRepositories.BugDetection;
using Application.Abstractions.Messaging;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Handlers.Commands.BugDetection.DeleteSession;

internal sealed class DeleteSessionCommandHandler(
    ICodeAnalysisSessionRepository sessionRepository,
    IUserContext userContext,
    ILogger<DeleteSessionCommandHandler> logger)
    : ICommandHandler<DeleteSessionCommand, string>
{
    public async Task<ErrorOr<string>> Handle(
        DeleteSessionCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            if (command.SessionId == Guid.Empty)
                return Errors.Common.Validation(
                    "SessionId",
                    "SessionId cannot be empty");

            var session = await sessionRepository
                .GetByIdAsync(
                    command.SessionId,
                    cancellationToken);

            if (session is null)
                return Errors.Common.NotFound(
                    "Session",
                    command.SessionId.ToString());

            var keycloakId = userContext.IdentityId;
            if (session.UserId != keycloakId)
                return Errors.Common.Unauthorized(
                    "Session",
                    "You do not have access to this session");

            await sessionRepository.DeleteAsync(
                session, cancellationToken);

            logger.LogInformation(
                "Session {SessionId} deleted by {UserId}",
                command.SessionId, keycloakId);

            return $"Session {command.SessionId} deleted successfully.";
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Error deleting session {SessionId}",
                command.SessionId);
            return Errors.Infrastructure.DatabaseError(
                "DeleteSession.Failed", ex.ToString());
        }
    }
}