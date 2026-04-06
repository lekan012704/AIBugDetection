using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Helper
{
    public static class AppWebExtension
    {
        public static string AssessmentItemKey(string itemKey)
        {
            char[] padding = { '=' };
            var key = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{itemKey}")).TrimEnd(padding).Replace('+', '-').Replace('/', '_');
            return key;
        }
        public static string? GetQueryStringValue(this HttpRequest request, int position = 1)
        {
            var url = request.GetDisplayUrl();
            if (string.IsNullOrWhiteSpace(url)) return null;

            var segments = url.Split('/');
            if (segments.Length < position) return null;

            var value = segments[position - 1];
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public static string Base64UrlEncode(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes)
                .Replace('+', '-') // replace URL unsafe characters with safe ones
                .Replace('/', '_') // replace URL unsafe characters with safe ones
                .TrimEnd('=');     // remove padding
        }

        public static string Base64UrlDecode(string input)
        {
            var incoming = input
                .Replace('_', '/')
                .Replace('-', '+');

            switch (input.Length % 4)
            {
                case 2: incoming += "=="; break;
                case 3: incoming += "="; break;
            }

            var bytes = Convert.FromBase64String(incoming);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string GetDigitRandomNumbers()
        {
            Random generator = new();
            return generator.Next(1, 900000000).ToString("D5");
        }
        public static string RemoveAllNonLetterCharacters(string text)
        {
            //text = "adeoti&.,#$!)(*%@-_+=^~#:;?/|\\ ";
            return string.Join("",
                from ch in text
                where char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch)
                select ch);
        }
        public static string Capitalize(string input)
        {
            return string.IsNullOrEmpty(input) ? input : input.ToUpper();
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
    }
}
