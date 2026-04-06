using Application.Abstractions.Messaging;

namespace Application.Handlers.Commands.Users.Register;

public sealed record RegisterUserCommand(
    string Email,
    string FirstName,
    string? LastName,
    string Password
) : ICommand<string>;