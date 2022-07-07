namespace DFTelegram.Helper
{
    public class DateTimeHelper
    {
        public static DateTime GetTodayAtZero()
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        }

        public static DateTime GetTomorrowAtZero()
        {
            return GetTodayAtZero().AddDays(1);
        }

        public static TimeSpan GetUntilTomorrowTimeSpan()
        {
            return (GetTomorrowAtZero() - DateTime.Now);
        }

        public static Double GetUntilTomorrowSeconds()
        {
            return GetUntilTomorrowTimeSpan().TotalSeconds;
        }

        public static int GetUntilTomorrowMilliseconds()
        {
            return (int)GetUntilTomorrowSeconds() * 1000;
        }


    }
}