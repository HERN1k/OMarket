namespace OMarket.Helpers.Utilities
{
    public static class RegexPatterns
    {
        public const string PhoneNumber = @"(?:\+380\s?)?(\d{2})\s?(\d{3})\s?(\d{4,5})";

        public const string PhoneNumberFormattingPattern = @"[()\s]";

        public const string Back = @"^(back|BACK|back_del|BACK_DEL)$";

        public const string Del = @"^(del|DEL)$";
    }
}