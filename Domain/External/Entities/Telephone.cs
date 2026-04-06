using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.External.Entities
{
    public class Telephone
    {
        public required string TaxPayerReferenceNumber { get; set; } 
        public string? PhoneNo1 { get; set; }
        public string? PhoneNo2 { get; set; }
        public string? PhoneNo3 { get; set; } 
        public bool? IsModified { get; set; }
    }
}
