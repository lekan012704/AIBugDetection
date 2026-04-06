namespace Domain.Models
{
    public class PermissionInfo
    {
        public string PermissionId { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string? PermissionUrl { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public string? RoleName { get; set; }
    }
}
