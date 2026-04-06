using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.External.Entities
{
    public class Lga
    {
        public Lga()
        {
            Addresses = [];
        }
        public int LgaId { get; set; }
        public string LgaCode { get; set; }
        public string LgaName { get; set; }
        public int? StateId { get; set; }
        public List<Address> Addresses { get; set; }
    }
}
