using Domain.Application.Entities.UserPermissions;
using Microsoft.AspNetCore.Identity;

namespace Domain.Application.Entities.Permissions
{
    public sealed class Permission
    {
        public  string? PermissionId { get; init; }

        public  string? PermissionCode { get; init; }

        public  string? PermissionName { get; init; }

        public  string? ParentPermissionCode { get; init; }


        public string? MenuFileName { get; init; }

        public string? PermissionUrl { get; init; }

        public string? ImgClass { get; init; }

        public  string? InstitutionCode { get; init; }

        public string? SectionImgClass { get; init; }

        public string? SectionName { get; init; }

        public int? PermissionOrder { get; init; }

        public bool IsMainTaxAgent { get; init; }

        public bool IsFinancialInstitution { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedAt { get; init; }

        public string? CreatedBy { get; init; }
        public ICollection<UserPermission> UserPermissions { get; init; } = new HashSet<UserPermission>();
    }
}
