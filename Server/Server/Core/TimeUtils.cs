using System;

namespace Core
{
    public static class TimeUtils
    {
        public static DateTime FromMilliseconds(long timestamp, bool local = false)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp / 1000.0);
            return local ? dt.ToLocalTime() : dt;
        }

        public static DateTime FromSeconds(double unixTimeStamp, bool local = false)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return local ? dtDateTime.ToLocalTime() : dtDateTime;
        }

        public static int GetDate(DateTime dt)
        {
            if (dt.Year >= 2021)
                return dt.Year * 10000 + dt.Month * 100 + dt.Day;
            return dt.Year * 1000 + dt.Month * 100 + dt.Day;
        }

        public static DateTime GetDateTimeNow()
        {
            return DateTime.UtcNow;
        }

        public static double GetUtcNowSenconds()
        {
            return DateTime.UtcNow.ToSeconds();
        }

        public static double GetTimeMilliSecond()
        {
            return GetUtcNowSenconds() * 1000f;
        }

        public static int GetTodayDate()
        {
            return GetDate(DateTime.UtcNow);
        }

        public static int GetYearMonthInteger()
        {
            return GetYearMonthInteger(DateTime.UtcNow);
        }

        public static int GetYearMonthInteger(DateTime now)
        {
            return now.Year * 100 + now.Month;
        }

        public static int GetYesterdayDate()
        {
            var dt = DateTime.UtcNow.AddDays(-1);
            return GetDate(dt);
        }

        public static bool IsValidTime(DateTime t1, DateTime t2, DateTime target)
        {
            return t1.CompareTo(target) < 0 && target.CompareTo(t2) < 0;
        }

        public static bool IsValidTime(double t1, double t2, double target)
        {
            return t1.CompareTo(target) < 0 && target.CompareTo(t2) < 0;
        }

        public static DateTime ParseOrDefault(string source, DateTime _default)
        {
            if (string.IsNullOrEmpty(source))
                return _default;

            return DateTime.TryParse(source, out var result) ? result : _default;
        }

        public static double ToSeconds(this DateTime dt)
        {
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (dt.Subtract(UnixEpoch)).TotalSeconds;
        }

        public static double ToMilliSeconds(this DateTime dt)
        {
            return ToSeconds(dt) * 1000f;
        }

        public static int DaysToSeconds(int days)
        {
            return days * 86400;
        }
    }
}