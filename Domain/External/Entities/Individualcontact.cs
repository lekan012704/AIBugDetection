using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.External.Entities
{
    public class Individualcontact
    {
        public int AddressID { get; set; }
        public string TaxPayerReferenceNumber { get; set; } = string.Empty;
    }
}
