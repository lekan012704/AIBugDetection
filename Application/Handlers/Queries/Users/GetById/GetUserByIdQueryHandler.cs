using Application.Abstractions.Authentication.Custom;
using Application.Abstractions.EntityRepositories.Users;
using Application.Abstractions.Messaging;
using Domain.Application.Entities.Users;
using ErrorOr;
using SharedKernel;

namespace Application.Handlers.Queries.Users.GetById;

internal sealed class GetUserByIdQueryHandler(IUserRespository _iuserRespository, IUserContext userContext)
    : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<ErrorOr<UserResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            if (query.UserId != userContext.UserId)
            {
                return Errors.Common.Unauthorized(
                        "Authentication",
                        "User not recognized");
            }

            User? userDetails = await _iuserRespository.GetByIdAsync(query.UserId.ToString(), cancellationToken);

            if (userDetails is null)
            {
                return Errors.Common.NotFound("User", query.UserId.ToString());
            }

            UserResponse user = new()
            {
                Id = userDetails.Id,
                FirstName = userDetails.FirstName,
                LastName = userDetails.LastName,
                Email = userDetails.Email
            };

            return user;
        }
        catch (Exception ex)
        {
            return Errors.Infrastructure.DatabaseError($"Failed to process request {query.UserId.ToString()}", ex.ToString());
        }
    }
}
