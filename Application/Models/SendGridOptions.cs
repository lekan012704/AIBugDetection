namespace Application.Models
{
    public class SendGridOptions
    {
        public const string SectionName = "SendGrid";
        public string ApiKey { get; set; } = null!;
        public string TestEmail { get; set; } = null!;
        public bool SetAsDefaultProvider { get; set; }
    }
}
