using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helper
{
    public static class FormatHelper
    {
        public static string FormatDecimal(decimal? value)
        {
            return value?.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00";
        }

        public static string FormatDecimal(double? value)
        {
            return value.HasValue
                ? Convert.ToDecimal(value.Value).ToString("N2", CultureInfo.InvariantCulture)
                : "0.00";
        }
    }

}
