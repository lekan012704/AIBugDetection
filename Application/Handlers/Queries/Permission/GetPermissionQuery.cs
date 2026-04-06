using Application.Abstractions.Authorization;
using Application.Abstractions.Messaging;
using Domain.Application.Dtos;
using ErrorOr;

namespace Application.Handlers.Queries.Permission
{
    public class GetPermissionQuery : IQuery<List<MenuSetup>>
    {
        public required string UserId { get; set; }
    }

    internal class GetPermissionQueryHandler : IQueryHandler<GetPermissionQuery, List<MenuSetup>>
    {
        private readonly IPermissionRepository _permissionRepository;
        public GetPermissionQueryHandler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }
        public async Task<ErrorOr<List<MenuSetup>>> Handle(GetPermissionQuery request, CancellationToken cancellationToken)
        {
            var permissions = await _permissionRepository.GetPermissionsByUserIdAsync(request.UserId);
            return permissions;
        }
    }
}
