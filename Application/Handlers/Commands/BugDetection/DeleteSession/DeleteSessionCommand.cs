using Application.Abstractions.Messaging;

namespace Application.Handlers.Commands.BugDetection.DeleteSession;

public sealed record DeleteSessionCommand(
    Guid SessionId
) : ICommand<string>;