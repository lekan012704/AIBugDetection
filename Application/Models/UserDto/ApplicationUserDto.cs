using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.UserDto
{
    public class ApplicationUserDto
    {
        public string? PayerName { get; set; }
        public string? PayerId { get; set; }
        public string? EmailAddress { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? OtherNames { get; set; }
        public bool? IsEnabled { get; set; }
        public DateTime? EnabledDate { get; set; }
        public string? VerificationToken { get; set; }
        public string? PayerAddress { get; set; }
        public string? PayerUTIN { get; set; }
        public string? PayerType { get; set; }
        public string? CreatedOnFormated { get; set; }
        public List<string>? Role { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? PwdExpired { get; set; }
        public string? IsLocked { get; set; }
        public string? PayerJTBUTIN { get; set; }
        public string? TaxPayerReferenceNumber { get; set; }
        public string? TaxAgentReferenceNumber { get; set; }
        public string? StateName { get; set; }
        public string? MerchantCode { get; set; }
        public long? AccessRemainingDaysCount => Convert.ToInt64(Math.Round(DateTime.Now.Subtract(DateTime.Parse(EnabledDate.ToString())).TotalDays));
        public string? CreatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? DateLastmodified { get; set; }
        public bool? PasswordRecoveryRequest { get; set; }
        public DateTime? PasswordRecoveryRequestDate { get; set; }
        public DateTime? Verified { get; set; }
        public bool IsVerified => Verified.HasValue || PasswordReset.HasValue;
        public DateTime? PasswordReset { get; set; }
        public DateTime? Updated { get; set; }
        public bool? IsAccountActivated { get; set; }
        public DateTime? AccountActivatedDate { get; set; }
        public bool IsFinancialInstitution { get; set; }
    }
}
