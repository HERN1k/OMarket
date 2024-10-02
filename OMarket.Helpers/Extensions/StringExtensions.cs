using System.Text;
using System.Text.RegularExpressions;

using OMarket.Helpers.Utilities;

namespace OMarket.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static bool RegexIsMatch(this string input, string pattern)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pattern, nameof(pattern));

            if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return Regex.IsMatch(input, pattern);
        }

        public static string ConvertToBase64(this string input) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(input));

        public static string ConvertFromBase64(this string input) =>
            Encoding.UTF8.GetString(Convert.FromBase64String(input));

        public static string TrimTelegramMessageText(this string input)
        {
            return input.Length >= 4096 ? input[..4000] + "... (Message too long)" : input;
        }

        public static string TrimTelegramCallbackText(this string input)
        {
            return input.Length >= 200 ? input[..190] + "... (Message too long)" : input;
        }

        public static string FormattedExceptionStatus(this string input)
        {
            string message = input.Replace('_', ' ');

            return message[0] + message.Substring(1).ToLower() + '.';
        }

        public static string FormattedPhoneNumber(this string input)
        {
            string formattedPhoneNumber = Regex.Replace(
                    input: input,
                    pattern: RegexPatterns.PhoneNumberFormattingPattern,
                    replacement: string.Empty);

            formattedPhoneNumber = '+' + formattedPhoneNumber;

            if (formattedPhoneNumber.Length <= 32 &&
                !formattedPhoneNumber.RegexIsMatch(RegexPatterns.PhoneNumber))
            {
                return string.Empty;
            }

            return formattedPhoneNumber;
        }
    }
}