namespace Application.Models
{
    public class ResetPasswordRequest
    {
        public required string VerificationToken { get; set; }
        public required string NewPassword { get; set; }
        public bool AdminRequest { get; set; }
        public bool? MustChangePassword { get; set; }
    }
}
