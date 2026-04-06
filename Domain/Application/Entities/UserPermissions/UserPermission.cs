using Domain.Application.Entities.Permissions;
using Domain.Application.Entities.Users;
using SharedKernel;

namespace Domain.Application.Entities.UserPermissions
{
    public sealed class UserPermission : Entity<UserPermissionId>
    {
        public required string PermissionId { get; init; }

        public required string UserId { get; init; }

        public bool IsActive { get; init; }

        public Permission Permission { get; init; } = null!;
        public User User { get; init; } = null!;
    }
}
