using Application.Abstractions.Messaging;

namespace Application.Handlers.Queries.Users.GetById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserResponse>;
