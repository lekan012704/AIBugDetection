using Application.Abstractions.Authentication.Custom;
using Application.Abstractions.EntityRepositories.Users;
using Application.Abstractions.Messaging;
using Domain.Application.Entities.Users;
using ErrorOr;
using SharedKernel;

namespace Application.Handlers.Queries.Users.GetByEmail;

internal sealed class GetUserByEmailQueryHandler(IUserRespository _iuserRespository, IUserContext userContext)
    : IQueryHandler<GetUserByEmailQuery, UserResponse>
{
    public async Task<ErrorOr<UserResponse>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        try
        {
            User? userDetails = await _iuserRespository.GetByUserEmailAsync(query.Email, cancellationToken);

            if (userDetails is null)
            {
                return Errors.Common.NotFound("User", query.Email.ToString());
            }

            UserResponse user = new()
            {
                Id = userDetails.Id.ToString(),
                FirstName = userDetails.FirstName,
                LastName = userDetails.LastName,
                Email = userDetails.Email
            };

            if (user.Id != userContext.UserId.ToString())
            {
                return Errors.Common.Unauthorized(
                        "User",
                        "Invalid email supplied.");
            }

            return user;
        }
        catch (Exception ex)
        {
            return Errors.Infrastructure.DatabaseError($"Failed to process request {query.Email}", ex.ToString());
        }
    }
}
