using Application.Abstractions.Messaging;

namespace Application.Handlers.Commands.Users.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
) : ICommand<string>;