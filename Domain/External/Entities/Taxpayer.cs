using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.External.Entities
{
    public class Taxpayer
    {
        public string? PayerUtin { get; set; }
        public required string TaxPayerReferenceNumber { get; set; }
        public string? CourtesyTitle { get; set; }
        public string? Surname { get; set; } 
        public string? FirstName { get; set; } 
        public string? OtherName { get; set; }
        public int? GenderId { get; set; }
        public DateTime? DateofBirth { get; set; }
        public int? MaritalStatusId { get; set; }
        public  string? MerchantCode { get; set; } 
        public string? Email { get; set; }
        public string?   Occupation { get; set; } 
        public DateTime? DateCreated { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsRegConfirmed { get; set; }
        public string? CountryCode { get; set; } 
        public string? JTBTin { get; set; }
        public int? BusinessTypeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmploymentAddress { get; set; } 
        public int? EmploymentType { get; set; }
        public string? TaxAgentReferenceNumber { get; set; } 
        public string? RegTypeCode { get; set; }
        public string? StaffNumber { get; set; } 
        public bool? IsModified { get; set; }
        public string? CreatedBy { get; set; } 
        public DateTime? DateModified { get; set; }
        public string? AppName { get; set; }
        public bool? Ispulled { get; set; }
        public bool? IsPramary { get; set; }
        public bool? IsAttendentto { get; set; }
        public string? FinalUTIN { get; set; }
    }
}
