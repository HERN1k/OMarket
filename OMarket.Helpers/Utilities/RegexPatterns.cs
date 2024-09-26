namespace OMarket.Helpers.Utilities
{
    public static class RegexPatterns
    {
        public const string PhoneNumber = @"^(\+?)(\d{1,3})\s?(\d{1,4})\s?(\d{1,4})\s?(\d{1,9})$";

        public const string PhoneNumberFormattingPattern = @"[()\s-+&<>\']";

        public const string Back = @"^(back|BACK|back_del|BACK_DEL)$";

        public const string Del = @"^(del|DEL)$";
    }
}