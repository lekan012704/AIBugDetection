using Domain.Application.Entities.UserPermissions;
using SharedKernel;

namespace Domain.Application.Entities.Users
{
    public sealed class User   
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string KeycloakId { get; set; }

       
        public required string Email { get; set; }
        public required string FirstName { get; set; }  
        public string? LastName { get; set; }
        public string? UserName { get; set; }

        public bool IsFinancialInstitution { get; set; }
        public string? ProfilePicture { get; set; }
        public bool IsDeleted { get; set; }

    
        public bool IsAppEnabled { get; set; } = true;
        public DateTime? AppDisabledAt { get; set; }
        public string? AppDisabledBy { get; set; }

    
        public DateTime? FirstLoginAt { get; set; }
        public DateTime? LastLoginAt { get; set; }


        public byte[]? RowVersion { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public ICollection<UserPermission> UserPermissions { get; set; }
            = new HashSet<UserPermission>();
    }
}