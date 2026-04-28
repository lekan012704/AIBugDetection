using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Handlers.Commands.Users.Login;

public sealed record LoginUserCommand(string Email, string Password, bool RememberMe = false) : ICommand<MessageClass>;
