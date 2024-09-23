using System.Text;
using System.Text.RegularExpressions;

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
    }
}