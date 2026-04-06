namespace SharedKernel.Helpers
{
    public class EmailConfiguration
    {
        public string From { get; init; } = string.Empty;
        public string SmtpServer { get; init; } = string.Empty;
        public int Port { get; init; }
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string EnableSsl { get; init; } = string.Empty;
        public string Sender { get; init; } = string.Empty;
        public string ToMail { get; init; } = string.Empty;
    }
}
