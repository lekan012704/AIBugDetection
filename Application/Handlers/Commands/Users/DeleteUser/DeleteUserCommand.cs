using Application.Abstractions.Messaging;

namespace Application.Handlers.Commands.Users.DeleteUser;

public sealed record DeleteUserCommand(
    string UserId
) : ICommand<string>;