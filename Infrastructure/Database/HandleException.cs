//using Application.Abstractions.Data;
//using Application.Emailrequest;
//using Microsoft.Extensions.Logging;
//using System.Globalization;
//using System.Text;

//namespace Infrastructure.Database
//{
//    public sealed class HandleException : IHandleException
//    {
//        private readonly IEmailAddressService _emailService;
//        private readonly ILogger<HandleException> _logger;
//        public HandleException(IEmailAddressService emailService, ILogger<HandleException> logger)
//        {
//            _emailService = emailService;
//            _logger = logger;
//        }

//        private static string CreateExceptionBody(Exception ex)
//        {
//            string? errorLineNo = ex.StackTrace?.Substring(ex.StackTrace.Length - 7, 7);
//            string errorMessage = ex.Message;

//            var sb = new StringBuilder();
//            sb.AppendLine("Dear Team,");
//            sb.AppendLine();
//            sb.AppendLine($"An exception occurred in an application with the following details:");
//            sb.AppendLine();
//            sb.AppendLine($"Exception Type: {ex.GetType().Name}");
//            sb.AppendLine();
//            sb.AppendLine($"Error Message Line No: {errorLineNo}");
//            sb.AppendLine();
//            sb.AppendLine($"Message: {errorMessage}");
//            sb.AppendLine();
//            sb.AppendLine($"Source: {ex.Source}");
//            sb.AppendLine();
//            sb.AppendLine($"TargetSite: {ex.TargetSite}");
//            sb.AppendLine();
//            sb.AppendLine($"Stack Trace: {ex.StackTrace}");
//            sb.AppendLine();
//            sb.AppendLine($"Occurred at: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");


//            // If there are inner exceptions, include those details as well
//            Exception inner = ex.InnerException;
//            while (inner != null)
//            {
//                sb.AppendLine();
//                sb.AppendLine("Inner Exception:");
//                sb.AppendLine();
//                sb.AppendLine($"Exception Type: {inner.GetType().Name}");
//                sb.AppendLine();
//                sb.AppendLine($"Message: {inner.Message}");
//                sb.AppendLine();
//                sb.AppendLine($"Source: {inner.Source}");
//                sb.AppendLine();
//                sb.AppendLine($"TargetSite: {inner.TargetSite}");
//                sb.AppendLine();
//                sb.AppendLine($"Stack Trace: {inner.StackTrace}");
//            }

//            sb.AppendLine();
//            sb.AppendLine("Thanks and Regards");
//            sb.AppendLine("Application Admin");


//            return sb.ToString();
//        }

//        public Task LogEvents(Exception exception)
//        {
//            throw new NotImplementedException();
//        }

//        public async Task SendExceptionAsEmailAsync(Exception exception)
//        {
//            try
//            {
//                var messageBody = CreateExceptionBody(exception);
//                await _emailService.SendExceptionAsEmailAsync(null, "Unhandled Exception Notification.",
//                    messageBody);
//            }
//            catch (Exception ex)
//            {
//                //Log the error and send to sentry
//                _logger.LogError("Failed to send email notification: {Message}", ex.Message);
//            }
//        }
//    }
//}
