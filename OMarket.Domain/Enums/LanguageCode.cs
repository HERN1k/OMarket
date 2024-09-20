namespace OMarket.Domain.Enums
{
    public enum LanguageCode
    {
        NONE = 0,
        UK = 2,
    }

    public static class LanguageCodeExtensions
    {
        public static LanguageCode GetLanguageCode(string code)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrWhiteSpace(code))
            {
                return LanguageCode.NONE;
            }

            if (Enum.TryParse(code, true, out LanguageCode result))
            {
                return result;
            }
            else
            {
                return LanguageCode.NONE;
            }
        }
    }
}