using SharedKernel;

namespace Domain.Application.Entities.Users;

public sealed record UserRegisteredDomainEvent(string UserId) : IDomainEvent;

//https://mail.google.com/mail/u/0/#inbox/FMfcgzQbfLdkSbqgCjvlhCgwRDvrHPDt

//https://www.milanjovanovic.tech/blog/how-to-use-domain-events-to-build-loosely-coupled-systems?utm_source=newsletter&utm_medium=email&utm_campaign=tnw143

