using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum LoggerTypes
    {
        [Display(Name = "Serilog")]
        Serilog,

        [Display(Name = "Sentry")]
        Sentry
    }
}
