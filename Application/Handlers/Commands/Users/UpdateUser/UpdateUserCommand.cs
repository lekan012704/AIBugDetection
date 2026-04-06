using Application.Abstractions.Messaging;

namespace Application.Handlers.Commands.Users.UpdateUser;

public sealed record UpdateUserCommand(
    string? FirstName,
    string? LastName
) : ICommand<string>;