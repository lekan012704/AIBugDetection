using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SharedKernel.Helpers
{
    public static class StringFormatter
    {
        public static string RemoveAllNonLetterCharacters(string text)
        {
            return string.Join("",
                from ch in text
                where char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch)
                select ch);
        }

        public static bool IsValidEmailAddress(string email)
        {
            // Regular expression pattern for email validation
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            // Check if the email is not null or empty
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            // Use Regex.IsMatch to validate the email against the pattern
            return Regex.IsMatch(email, pattern);
        }
        public static string RandomTokenString()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(40);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        public static bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsValidYear(int year)
        {
            int currentYear = DateTime.Now.Year;
            return year >= 1900 && year <= currentYear + 1; // Allow near-future years
        }


        public static decimal ParseDecimalOrDefault(object? value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return 0.00m;

            return decimal.TryParse(value.ToString(), out decimal result) ? result : 0.00m;
        }
        public static int ParseIntOrDefault(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return 0;

            return int.TryParse(value.ToString(), out int result) ? result : 0;
        }
       
    }
}
