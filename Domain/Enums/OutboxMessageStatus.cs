using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Outbox
{
    //[JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum OutboxMessageStatus
    {
        [Display(Name = "Pending")]
        Pending,
        [Display(Name = "Processing")]
        Processing,
        [Display(Name = "Completed")]
        Completed,
        [Display(Name = "Failed")]
        Failed
    }
}
