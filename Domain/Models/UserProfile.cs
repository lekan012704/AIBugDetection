namespace Domain.Models
{
    public class UserProfile
    {
        public string? IdentityId { get; set; }
        public string Id { get; set; } = string.Empty;
        public required string UserName { get; set; }
        public required string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
