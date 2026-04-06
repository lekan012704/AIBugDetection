using Application.Abstractions.Messaging;
using Application.Helper;
using Domain.Application.Entities.Users;

namespace Application.Handlers.Queries.Users.GetAllUser;

public sealed class GetAllUserQuery : IQuery<Response<IQueryable<User>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
