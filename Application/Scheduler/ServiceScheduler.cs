//using Application.Abstractions.Data;
//using Application.Abstractions.EntityRepositories.Users;
//using Application.Emailrequest;
//using Application.Helper;
//using Application.Models;
//using Domain.Application.Entities;
//using Domain.Application.Entities.Users;
//using Domain.Enums;
//using Hangfire;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Newtonsoft.Json;
//using Serilog;
//using SharedKernel;
//using SharedKernel.Helpers;
//using SharedKernel.Helpers.GenericHttpClientService;
//using System.Diagnostics;
//using System.Globalization;

//namespace Application.Scheduler
//{
//    public class ServiceScheduler
//    {
//        private readonly IEmailAddressService _iEmailAddressService;
//        private readonly IGenericHttpClientHandlerService _genericHttpClientHandlerService;
//        private readonly ILogger<ServiceScheduler> _logger;
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IUserRespository _userRespository;
//        private readonly AppSettings _appSettings;
//        private readonly IEmailAddressService _emailService;

//        public ServiceScheduler(IEmailAddressService iEmailAddressService,
          
//            IGenericHttpClientHandlerService genericHttpClientHandlerService,
//            IOptions<AppSettings> appSettings,
//            ILogger<ServiceScheduler> logger, IUnitOfWork unitOfWork,
//            IUserRespository userRespository, IEmailAddressService emailService
//         )
//        {
//            _iEmailAddressService = iEmailAddressService;
          
//            _genericHttpClientHandlerService = genericHttpClientHandlerService;
//            _logger = logger;
//            _unitOfWork = unitOfWork;
//            _userRespository = userRespository;
//            _appSettings = appSettings.Value;
//            _emailService = emailService;
  
//        }

//        public async Task SendEmailNotificationJobAsAsync(string templatePath, string sendMail, string sentTitle)
//        {
//            await _iEmailAddressService.ValidateTransactionAndSendMail(templatePath, sendMail, sentTitle);
//        }
     
 
     
//    }
//}
