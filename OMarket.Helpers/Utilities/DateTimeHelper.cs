using OMarket.Domain.Enums;

namespace OMarket.Helpers.Utilities
{
    public static class DateTimeHelper
    {
        public static TimeOfDay TimeOfDayNow()
        {
            int hour = DateTime.Now.Hour;

            return hour switch
            {
                >= 0 and < 6 => TimeOfDay.Night,
                >= 6 and < 10 => TimeOfDay.Morning,
                >= 10 and < 18 => TimeOfDay.Day,
                >= 18 and < 22 => TimeOfDay.Evening,
                >= 22 and < 24 => TimeOfDay.Night,
                _ => TimeOfDay.None
            };
        }
    }
}