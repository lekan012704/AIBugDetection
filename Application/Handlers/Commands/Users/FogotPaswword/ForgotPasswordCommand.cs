using Application.Abstractions.Messaging;

namespace Application.Handlers.Commands.Users.ForgotPassword;

public sealed record ForgotPasswordCommand(
    string Email
) : ICommand<string>;