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

        public static bool IsTimeAllowed()
        {
            TimeZoneInfo ukraineTimeZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
            DateTime currentUkraineTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, ukraineTimeZone);

            TimeSpan start = new(22, 0, 0);
            TimeSpan end = new(10, 0, 0);
            TimeSpan now = currentUkraineTime.TimeOfDay;

            if (now >= start || now < end)
            {
                return false;
            }

            return true;
        }
    }
}