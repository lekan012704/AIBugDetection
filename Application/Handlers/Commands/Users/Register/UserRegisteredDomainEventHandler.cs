using Domain.Application.Entities.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Commands.Users.Register;

internal sealed class UserRegisteredDomainEventHandler : INotificationHandler<UserRegisteredDomainEvent>
{
    private readonly ILogger<UserRegisteredDomainEventHandler> _logger;

    public UserRegisteredDomainEventHandler(ILogger<UserRegisteredDomainEventHandler> logger)
    {
        _logger = logger;
    }
    public Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        // TODO: Send an email verification link, etc.

        //_logger.LogInformation(
        //    "User registered: {UserId}, Email: {Email}, Name: {FirstName} {LastName}",
        //    notification.UserId,
        //    notification.Email,
        //    notification.FirstName,
        //    notification.LastName);

        // Here you could add additional logic like:
        // - Send welcome notification
        //await _emailService.SendWelcomeEmail(user.Email);
        // - Create user profile
        // - Add to mailing list
        // - etc.

        return Task.CompletedTask;
    }
}
