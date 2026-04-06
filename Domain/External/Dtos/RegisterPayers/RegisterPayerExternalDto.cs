using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.External.Dtos.RegisterPayers
{
    public class RegisterPayerExternalDto
    {
            public string Title { get; set; }
            public string Surname { get; set; }
            public string FirstName { get; set; }
            public string OthersName { get; set; }
            public string ContactPhoneNo { get; set; }
            public string ContactEmail { get; set; }
            public string ContactAddress { get; set; }
            public int StateResidenceId { get; set; }
            public int LgaResidenceId { get; set; }
            public int GenderId { get; set; }
            public int? MaritalStatusId { get; set; }
            public string PlatFormCode { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string JTBTIN { get; set; }
            public int? BusinessTypeId { get; set; }
            public string BusinessAddress { get; set; }
            public string NIN { get; set; }
            public bool UpdatePreviousRecord { get; set; } = false;
            public string PayerId { get; set; }
            public string Occupation { get; set; }
            public string RegTypeCode { get; set; }
            public string ChannelCode { get; set; }
            public int? StateTownId { get; set; }
            public string RevenueOfficeId { get; set; }
            public string TaxAgentReferenceNumber { get; set; }
            public string MerchantCode { get; set; }
            public bool IsExternalCall { get; set; }
    }
}
