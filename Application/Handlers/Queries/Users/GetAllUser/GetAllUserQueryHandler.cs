//using Application.Abstractions.Authentication.Custom;
//using Application.Abstractions.EntityRepositories.Users;
//using Application.Abstractions.Messaging;
//using Domain.Application.Entities.Users;
//using ErrorOr;
//using SharedKernel;
//using Microsoft.Extensions.Logging;
//using Application.Helper;


//namespace Application.Handlers.Queries.Users.GetAllUser;

//internal sealed class GetAllUserQueryHandler(IUserRespository _iuserRespository, IUserContext userContext, ILogger<GetAllUserQueryHandler> _logger)
//    : IQueryHandler<GetAllUserQuery, Response<IQueryable<User>>>
//{
//    public async Task<ErrorOr<Response<IQueryable<User>>>> Handle(GetAllUserQuery query, CancellationToken cancellationToken)
//    {
//        try
//        {
//            var paginatedUsers = await _iuserRespository.GetPagedAsync(query.PageNumber,query.PageSize);
//            return new Response<IQueryable<User>>(paginatedUsers.AsQueryable(),true);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error occurred while fetching users.");
//            return Errors.Infrastructure.DatabaseError(string.Empty,"Failed to process request");
//        }
//    }
//}

