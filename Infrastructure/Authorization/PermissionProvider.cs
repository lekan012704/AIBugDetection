using Application.Abstractions.Authorization;
using Domain.Models;

namespace Infrastructure.Authorization;

internal sealed class PermissionProvider(IPermissionRepository repository)
{
    private readonly IPermissionRepository _repository = repository;

    public Task<HashSet<string>> GetForUserIdAsync(string userId) =>
        _repository.GetUserPermissionsAsync(userId);

    //public async Task<ErrorOr<HashSet<string>>> GetForUserIdAsync(Guid userId) =>
    //    await _repository.GetUserPermissionsAsync(userId, default);

    public Task<HashSet<string>> GetPermissionsForUserAsync(string identityId) =>
        _repository.GetPermissionsForUserAsync(identityId);

    //public async Task<ErrorOr<HashSet<string>>> GetPermissionsForUserAsync(string identityId) =>
    //    await _repository.GetPermissionsForUserAsync(identityId, default);

    public Task<UserRolesResponse> GetRolesForUserAsync(string identityId) =>
        _repository.GetRolesForUserAsync(identityId);

    //public async Task<ErrorOr<UserRolesResponse>> GetRolesForUserAsync(string identityId) =>
    //     await _repository.GetRolesForUserAsync(identityId, default);
}
