using Application.Abstractions.Messaging;

namespace Application.Handlers.Queries.Users.GetByEmail;

public sealed record GetUserByEmailQuery(string Email) : IQuery<UserResponse>;
