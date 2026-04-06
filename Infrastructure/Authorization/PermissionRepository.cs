using Application.Abstractions.Authorization;
using Application.Caching;
using Domain.Application.Dtos;
using Domain.Application.Entities.Permissions;
using Domain.Models;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authorization
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly ILogger<PermissionRepository> _logger;
        private static readonly TimeSpan PermissionExpiration = TimeSpan.FromHours(1);

        public PermissionRepository(
            ApplicationDbContext context,
            ICacheService cacheService,
            ILogger<PermissionRepository> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        // ✅ Called by your authorization handler on every request
        // Uses YOUR DB userId (not KeycloakId)
        public async Task<HashSet<string>> GetPermissionsForUserAsync(string userId)
        {
            string cacheKey = $"auth:permissions-{userId}";

            return await _cacheService.GetOrSetAsync(
                key: cacheKey,
                factory: async () => await GetUserPermissionsFromDatabaseAsync(userId),
                expiration: PermissionExpiration);
        }

        public async Task<HashSet<string>> GetUserPermissionsAsync(string userId)
        {
            string cacheKey = $"auth:permissions-{userId}";

            return await _cacheService.GetOrSetAsync(
                key: cacheKey,
                factory: async () => await GetPermissionsFromDatabaseAsync(userId),
                expiration: PermissionExpiration);
        }

        public async Task<List<MenuSetup>> GetPermissionsByUserIdAsync(string userId)
        {
            string cacheKey = $"auth:menupermissions-{userId}";

            return await _cacheService.GetOrSetAsync(
                key: cacheKey,
                factory: async () => await GetPermissionsAsync(userId),
                expiration: PermissionExpiration);
        }

        // ✅ Now uses YOUR own Roles table, not IdentityRole
        public async Task<UserRolesResponse> GetRolesForUserAsync(string userId)
        {
            string cacheKey = $"auth:roles-{userId}";

            return await _cacheService.GetOrSetAsync(
                key: cacheKey,
                factory: async () => await GetUserRolesFromDatabaseAsync(userId),
                expiration: PermissionExpiration);
        }

        private async Task<HashSet<string>> GetUserPermissionsFromDatabaseAsync(string userId)
        {
            try
            {
                var menuPermissions = await _context.UserPermissions
                    .Include(x => x.User)
                    .AsSplitQuery()
                    .Where(urp =>
                        urp.User.Id.ToString() == userId &&
                        urp.IsActive == true &&
                        urp.Permission.IsActive == true)
                    .Select(urp => new
                    {
                        urp.PermissionId,
                        urp.Permission.PermissionUrl,
                        urp.Permission.PermissionName
                    })
                    .Distinct()
                    .ToListAsync();

                var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var menu in menuPermissions)
                {
                    if (!string.IsNullOrEmpty(menu.PermissionId))
                        permissions.Add($"MENUID_{menu.PermissionId}");
                    if (!string.IsNullOrEmpty(menu.PermissionUrl))
                        permissions.Add($"MENULINK_{menu.PermissionUrl}");
                    if (!string.IsNullOrEmpty(menu.PermissionName))
                        permissions.Add($"MENUNAME_{menu.PermissionName}");
                }

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for user: {UserId}", userId);
                return [];
            }
        }

        private async Task<HashSet<string>> GetPermissionsFromDatabaseAsync(string userId)
        {
            try
            {
                // ✅ Uses your own UserRoles + Roles tables (not Identity)
                var menuPermissions = await (
                    from up in _context.UserPermissions
                    join permission in _context.Permissions
                        on up.PermissionId equals permission.PermissionId
                    where up.UserId == userId &&
                          up.IsActive == true &&
                          permission.IsActive == true
                    select new MenuSetup
                    {
                        PermissionId = permission.PermissionId,
                        PermissionUrl = permission.PermissionUrl,
                        PermissionName = permission.PermissionName,
                        MenuFileName = permission.MenuFileName,
                        ImgClass = permission.ImgClass,
                        SectionImgClass = permission.SectionImgClass,
                        SectionName = permission.SectionName,
                        ParentPermissionCode = permission.ParentPermissionCode,
                        PermissionOrder = permission.PermissionOrder,
                        IsFinancialInstitution = permission.IsFinancialInstitution
                    })
                    .Distinct()
                    .OrderBy(x => x.PermissionId)
                    .ToListAsync();

                var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var menu in menuPermissions)
                {
                    if (!string.IsNullOrEmpty(menu.PermissionId))
                        permissions.Add($"MENUID_{menu.PermissionId}");
                    if (!string.IsNullOrEmpty(menu.PermissionUrl))
                        permissions.Add($"MENULINK_{menu.PermissionUrl}");
                    if (!string.IsNullOrEmpty(menu.PermissionName))
                        permissions.Add($"MENUNAME_{menu.PermissionName}");
                }

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for user: {UserId}", userId);
                return [];
            }
        }

        private async Task<List<MenuSetup>> GetPermissionsAsync(string userId)
        {
            try
            {
                return await (
                    from up in _context.UserPermissions
                    join permission in _context.Permissions
                        on up.PermissionId equals permission.PermissionId
                    where up.UserId == userId &&
                          permission.IsActive == true
                    select new MenuSetup
                    {
                        PermissionId = permission.PermissionId,
                        PermissionUrl = permission.PermissionUrl,
                        PermissionName = permission.PermissionName,
                        MenuFileName = permission.MenuFileName,
                        ImgClass = permission.ImgClass,
                        SectionImgClass = permission.SectionImgClass,
                        SectionName = permission.SectionName,
                        ParentPermissionCode = permission.ParentPermissionCode,
                        PermissionOrder = permission.PermissionOrder,
                        IsFinancialInstitution = permission.IsFinancialInstitution,
                        PermissionCode = permission.PermissionCode
                    })
                    .Distinct()
                    .OrderBy(x => x.PermissionId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for user: {UserId}", userId);
                return [];
            }
        }


        private async Task<UserRolesResponse> GetUserRolesFromDatabaseAsync(string userId)
        {
            try
            {
                var roles = await _context.UserPermissions
                    .Where(up => up.UserId == userId && up.IsActive) 
                    .Select(up => new Permission 
                    {
                        PermissionId = up.Permission.PermissionId,
                        PermissionName = up.Permission.PermissionName
                    })
                    .Distinct()
                    .ToListAsync();

                return new UserRolesResponse
                {
                    UserId = userId,
                    Permissions = roles
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user: {UserId}", userId);
                return new UserRolesResponse { UserId = userId };
            }
        }

        public async Task<List<PermissionInfo>> GetUserPermissionDetailsAsync(string userId)
        {
            try
            {
                return await (
                    from up in _context.UserPermissions
                    join permission in _context.Permissions
                        on up.PermissionId equals permission.PermissionId
                    where up.UserId == userId &&
                          up.IsActive == true &&
                          permission.IsActive == true
                    select new PermissionInfo
                    {
                        PermissionId = permission.PermissionId,
                        PermissionName = permission.PermissionName,
                        PermissionUrl = permission.PermissionUrl
                    })
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting permission details for user: {UserId}", userId);
                return [];
            }
        }

        public async Task<bool> UserHasPermissionAsync(string userId, string permission)
        {
            var userPermissions = await GetPermissionsFromDatabaseAsync(userId);
            return userPermissions.Contains(permission);
        }
    }
}
