using Domain.Application.Dtos;
using Domain.Models;

namespace Application.Abstractions.Authorization
{
    public interface IPermissionRepository
    {
        //Task<ErrorOr<HashSet<string>>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default);

        //Task<ErrorOr<HashSet<string>>> GetPermissionsForUserAsync(string identityId, CancellationToken ct = default);

        //Task<ErrorOr<UserRolesResponse>> GetRolesForUserAsync(string identityId, CancellationToken ct = default);

        Task<HashSet<string>> GetUserPermissionsAsync(string userId);

        Task<UserRolesResponse> GetRolesForUserAsync(string identityId);

        Task<HashSet<string>> GetPermissionsForUserAsync(string identityId);
        Task<List<MenuSetup>> GetPermissionsByUserIdAsync(string userId);
        Task<bool> UserHasPermissionAsync(string userId, string permission);
        Task<List<PermissionInfo>> GetUserPermissionDetailsAsync(string userId);
    }
}
