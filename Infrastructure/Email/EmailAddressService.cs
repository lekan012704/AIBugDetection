//using Application.Abstractions.EntityRepositories.Users;
//using Application.Emailrequest;
//using Application.Helper;
//using Application.Models;
//using Dapper;
//using Domain.Application.Entities.Users;
//using EmailSenderUtility;
//using EmailSenderUtility.Core;
//using EmailSenderUtility.Core.Message;
//using Infrastructure.EntityRepositories.Users;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using MimeKit;
//using SharedKernel.Helpers;
//using System.Text.Encodings.Web;

//namespace Infrastructure.Email
//{
//    public class EmailAddressService(IEmailService emailService, IOptions<SmtpOptions> smtpOptions, IHttpContextAccessor httpContextAccessor, ILogger<UserRepository> logger,
//        UserManager<User> userManager, IOptions<AppSettings> appSettings, IUserRespository _iUserRespository) : IEmailAddressService
//    {
//        private readonly IEmailService _emailService = emailService;
//        private readonly SmtpOptions _smtpOptions = smtpOptions.Value;
//        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
//        private readonly ILogger<UserRepository> _logger = logger;
//        private readonly UserManager<User> _userManager = userManager;
//        private readonly AppSettings _appSettings = appSettings.Value;
//        private readonly IUserRespository _iUserRespository = _iUserRespository;
      

//        public async Task SendAsync(string to, string subject, string html, string? from = null, List<IFormFile>? files = null)
//        {
//            var email = new EmailMessage
//            {
//                From = new NameValue(_smtpOptions.DisplayName, _smtpOptions.Username),
//                To = [new("", to)],
//                Body = html,
//                Subject = subject,
//                IsHtml = true,
//            };
//            await SendMail(to, email);
             
//        }

//        public Task<bool> SendEmailsAsync(string receiverAddress, string subject, string message, List<string>? attachments = null)
//        {
//            throw new NotImplementedException();
//        }

//        public Task SendEmailWithAttachmentAsync(string to, string subject, string? from = null, string? attachedMent = null, string? saveFileName = null)
//        {
//            throw new NotImplementedException();
//        }

//        public Task SendExceptionAsEmailAsync(Application.Models.Email? recipient, string subject, string body)
//        {
//            throw new NotImplementedException();
//        }

      
//        private async Task SendRecoveryEmailAsync(string templatePath, string msgSubject, string link, User user)
//        {
//            try
//            {
//                var userName = string.Empty;
           

//                var userEmail = user.Email!.Trim();

//                var filename = templatePath;

//                var mailbody = File.ReadAllText(filename);
//                mailbody = mailbody.Replace("##FirstName##", userName.Trim());
//                mailbody = mailbody.Replace("##UserName##", user.Email!.Trim());
//                var confirmLink =
//                    $"<a href='{HtmlEncoder.Default.Encode(link)}'>click here</a>.";
//                mailbody = mailbody.Replace("##ActivationLink##", "'" + confirmLink + "'");

//                if (!StringFormatter.IsValidEmailAddress(userEmail))
//                {
//                    _logger.LogError($"Error sending mail to {user.UserName} specified email address; due to incorrect mail address {userEmail}");
//                }
//                else
//                {
//                    var email = new EmailMessage
//                    {
//                        From = new NameValue(_smtpOptions.DisplayName, _smtpOptions.Username),
//                        To = [new(userName, userEmail)],
//                        Body = mailbody,
//                        Subject = msgSubject,
//                        IsHtml = true,
//                    };

//                    await SendMail(userEmail, email);

//                    user.PasswordRecoveryRequest = true;
//                    user.PasswordRecoveryRequestAt = DateTime.Now;
//                    await _userManager.UpdateAsync(user);
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex.Message, $"{nameof(SendRecoveryEmailAsync)} failed", $"Email  {user.Email}");
//            }
//        }
//        public async Task ValidateTransactionAndSendMail(string templatePath, string sendMail, string sentTitle)
//        {
//            var title = string.Empty;
//            var usedMail = string.Empty;

//            try
//            {
//                if (string.IsNullOrEmpty(sendMail))
//                    _logger.LogError($"Entered email cannot be empty, please check and try again or contact administrator");
//                var user = _iUserRespository.Get(x => x.Email == sendMail.Trim()).FirstOrDefault();

//                if (user != null)
//                {
//                    var formulatelink = $"{_httpContextAccessor.HttpContext!.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}{_httpContextAccessor.HttpContext.Request.PathBase.Value}{appSettings.Value.PasswordResetCallBackUrl}?verificationToken{"="}{user.VerificationToken}";

//                    if (!string.IsNullOrEmpty(user.UserName.Trim()) && !string.IsNullOrEmpty(sentTitle.Trim()))
//                    {
//                        title = sentTitle;
//                    }

//                    if (!string.IsNullOrEmpty(sentTitle) && sentTitle == ApplicationConstant.RecoveryPasswordTitle)
//                    {
//                        await SendRecoveryEmailAsync(templatePath, title, formulatelink, user);
//                    }

//                    if (!string.IsNullOrEmpty(sentTitle) && (sentTitle == ApplicationConstant.GetOTPTitle || sentTitle == ApplicationConstant.GetOTPTitle2))
//                    {
//                        await SendGetOneTimePasswordEmailAsync(templatePath, title, formulatelink, user);
//                    }
//                }
//                else
//                {
//                    _logger.LogError($"No record found for supplied email {sendMail}");
//                }

//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex.Message, $"{nameof(ValidateTransactionAndSendMail)} failed", $"email {sendMail}");
//            }
//        }

//        private async Task SendGetOneTimePasswordEmailAsync(string templatePath, string msgSubject, string link, User user)
//        {
//            try
//            {
//                var username = string.Empty;
//                var filename = templatePath;
//                var userEmail = user.Email!.Trim();

//                username = user.UserName;

//                var mailbody = File.ReadAllText(filename);
//                mailbody = mailbody.Replace("##FirstName##", username.Trim() ?? null);
//                mailbody = mailbody.Replace("##UserEmail##", userEmail);
//                var confirmLink =
//                    $"<a href='{HtmlEncoder.Default.Encode(link)}'>click here</a>.";
//                mailbody = mailbody.Replace("##ActivationLink##", "'" + confirmLink + "'");

//                if (!StringFormatter.IsValidEmailAddress(user.Email.Trim()))
//                {
//                    _logger.LogError($"Error sending mail to {username} specified email address; due to incorrect mail address {userEmail}");
//                }
//                else
//                {
//                    var email = new EmailMessage
//                    {
//                        From = new NameValue(_smtpOptions.DisplayName, _smtpOptions.Username),
//                        To = [new(username, user.Email)],
//                        Body = mailbody,
//                        Subject = msgSubject,
//                        IsHtml = true,
//                    };

//                    await SendMail(userEmail, email);
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"{nameof(SendGetOneTimePasswordEmailAsync)} failed", $"Email {user.Email}");
//            }
//        }

//        private async Task SendMail(string sendMail, EmailMessage email)
//        {
//            if (await _emailService.SendEmailAsync(email))
//            {
//                _logger.LogInformation(message: $"{nameof(SendRecoveryEmailAsync)} Sent Successfully for {sendMail}");
//            }
//            else
//            {
//                _logger.LogError($"{nameof(SendRecoveryEmailAsync)} Sent Successfully for {sendMail}");
//            }
//        }

//        public Task<bool> SendHtmlAttachmentAsync(string recipientEmail, string recipientName, string subject, string? attachmentFullPath = null, string? body = null, DynamicParameters? dynamicParameters = null, string? attachmentName = null, string contentType = "application/pdf", string? fileExtention = "pdf", CancellationToken cancellationToken = default)
//        {
//            throw new NotImplementedException();
//        }
//        public class Message
//        {
//            public List<MailboxAddress> To { get; set; }
//            public string Subject { get; set; }
//            public string Content { get; set; }

//            public IFormFileCollection Attachments { get; set; }

//            public Message(IEnumerable<string> to, string subject, string content, IFormFileCollection attachments)
//            {
//                To = new List<MailboxAddress>();

//                To.AddRange(to.Select(x => new MailboxAddress("", x)));
//                Subject = subject;
//                Content = content;
//                Attachments = attachments;
//            }
//        }
//    }
//}
