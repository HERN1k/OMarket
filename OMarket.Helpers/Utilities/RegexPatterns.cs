namespace OMarket.Helpers.Utilities
{
    public static class RegexPatterns
    {
        public const string PhoneNumber = @"^(\+?)(\d{1,3})\s?(\d{1,4})\s?(\d{1,4})\s?(\d{1,9})$";

        public const string PhoneNumberFormattingPattern = @"[()\s-+&<>\']";

        public const string Back = @"^(back|BACK|back_del|BACK_DEL)$";

        public const string Del = @"^(del|DEL)$";

        public const string Login = @"^[A-Za-z0-9!@#$%^&*()\-_{}/\\]{3,32}$";

        public const string Password = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*()\-_{}\/\\])(?!.*\s).{8,}$";
    }
}