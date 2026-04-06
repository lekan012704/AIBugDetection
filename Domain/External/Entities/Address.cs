using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.External.Entities
{
   
    public class Address
    {
        public int AddressID { get; set; }
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int? LgaId { get; set; }
        public int? RevenueOfficeID { get; set; }
        public string PostalAddress { get; set; } = string.Empty;
    }
}
