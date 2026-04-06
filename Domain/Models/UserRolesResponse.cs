using Domain.Application.Entities.Permissions;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    public sealed class UserRolesResponse
    {
        public string? UserId { get; init; } = string.Empty;
        public ICollection<Permission> Permissions { get; set; }
            = new HashSet<Permission>();
    }
}
            