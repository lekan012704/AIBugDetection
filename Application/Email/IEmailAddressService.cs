//using Application.Models;
//using Dapper;
//using Microsoft.AspNetCore.Http;

//namespace Application.Emailrequest
//{
//    public interface IEmailAddressService
//    {

//        Task ValidateTransactionAndSendMail(string templatePath, string sendMail, string sentTitle);
//        Task SendAsync(string to, string subject, string html, string? from = null, List<IFormFile>? files = null);
//        Task<bool> SendHtmlAttachmentAsync(string recipientEmail, string recipientName, string subject, string? attachmentFullPath = null,
//            string? body = null, DynamicParameters? dynamicParameters = null, string? attachmentName = null, string contentType = "application/pdf",
//            string? fileExtention = "pdf", CancellationToken cancellationToken = default);
//        Task SendEmailWithAttachmentAsync(string to, string subject, string? from = null, string? attachedMent = null, string? saveFileName = null);
//        Task SendExceptionAsEmailAsync(Models.Email? recipient, string subject, string body);
//        Task<bool> SendEmailsAsync(string receiverAddress, string subject, string message, List<string>? attachments = null);
    

//    }
//}
