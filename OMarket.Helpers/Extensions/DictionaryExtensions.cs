using System.Collections.Frozen;

namespace OMarket.Helpers.Extensions
{
    public static class DictionaryExtensions
    {
        public static string TryGetTranslation(this FrozenDictionary<string, string> dictionary, string key)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            return string.Empty;
        }
    }
}