using Microsoft.AspNetCore.Http;

namespace Application.Models;

public abstract class EmailRequest
{
    protected EmailRequest(string to, string subject, string body, string from, List<IFormFile> files)
    {
        To = to ?? throw new ArgumentNullException(nameof(to));
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        From = from ?? throw new ArgumentNullException(nameof(from));
        Files = files ?? throw new ArgumentNullException(nameof(files));
    }

    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string From { get; set; }
    public List<IFormFile> Files { get; set; }
}

public abstract record Email(string Value);