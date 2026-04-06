namespace Application.Models
{
    public class SmtpOptions
    {
        public const string SectionName = "Smtp";
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string LicenseKey { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public bool SetAsDefaultProvider { get; set; }
        public string TestEmail { get; set; } = null;
    }
}
