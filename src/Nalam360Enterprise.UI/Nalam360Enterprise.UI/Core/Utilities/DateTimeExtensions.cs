namespace Nalam360Enterprise.UI.Core.Utilities;

/// <summary>
/// Extension methods for DateTime manipulation
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts a DateTime to relative time (e.g., "2 hours ago")
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>Relative time string</returns>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

        if (timeSpan.TotalSeconds < 60)
        {
            return "just now";
        }

        if (timeSpan.TotalMinutes < 60)
        {
            var minutes = (int)timeSpan.TotalMinutes;
            return $"{minutes} minute{(minutes == 1 ? "" : "s")} ago";
        }

        if (timeSpan.TotalHours < 24)
        {
            var hours = (int)timeSpan.TotalHours;
            return $"{hours} hour{(hours == 1 ? "" : "s")} ago";
        }

        if (timeSpan.TotalDays < 7)
        {
            var days = (int)timeSpan.TotalDays;
            return $"{days} day{(days == 1 ? "" : "s")} ago";
        }

        if (timeSpan.TotalDays < 30)
        {
            var weeks = (int)(timeSpan.TotalDays / 7);
            return $"{weeks} week{(weeks == 1 ? "" : "s")} ago";
        }

        if (timeSpan.TotalDays < 365)
        {
            var months = (int)(timeSpan.TotalDays / 30);
            return $"{months} month{(months == 1 ? "" : "s")} ago";
        }

        var years = (int)(timeSpan.TotalDays / 365);
        return $"{years} year{(years == 1 ? "" : "s")} ago";
    }

    /// <summary>
    /// Checks if a DateTime is today
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>True if today</returns>
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.Today;
    }

    /// <summary>
    /// Checks if a DateTime is yesterday
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>True if yesterday</returns>
    public static bool IsYesterday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.Today.AddDays(-1);
    }

    /// <summary>
    /// Checks if a DateTime is in the future
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>True if in the future</returns>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.Now;
    }

    /// <summary>
    /// Checks if a DateTime is in the past
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>True if in the past</returns>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.Now;
    }

    /// <summary>
    /// Gets the start of the day
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>Start of the day</returns>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>End of the day</returns>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the week (Monday)
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>Start of the week</returns>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        var diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
        return dateTime.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// Gets the end of the week (Sunday)
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>End of the week</returns>
    public static DateTime EndOfWeek(this DateTime dateTime)
    {
        return dateTime.StartOfWeek().AddDays(7).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the month
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>Start of the month</returns>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>End of the month</returns>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the year
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>Start of the year</returns>
    public static DateTime StartOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1);
    }

    /// <summary>
    /// Gets the end of the year
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>End of the year</returns>
    public static DateTime EndOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 12, 31, 23, 59, 59, 999);
    }

    /// <summary>
    /// Adds business days to a DateTime (skips weekends)
    /// </summary>
    /// <param name="dateTime">The starting DateTime</param>
    /// <param name="days">Number of business days to add</param>
    /// <returns>DateTime with business days added</returns>
    public static DateTime AddBusinessDays(this DateTime dateTime, int days)
    {
        var direction = days < 0 ? -1 : 1;
        var absdays = Math.Abs(days);
        var result = dateTime;

        while (absdays > 0)
        {
            result = result.AddDays(direction);
            if (result.DayOfWeek != DayOfWeek.Saturday && result.DayOfWeek != DayOfWeek.Sunday)
            {
                absdays--;
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if a DateTime is a weekend
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>True if weekend</returns>
    public static bool IsWeekend(this DateTime dateTime)
    {
        return dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    /// Checks if a DateTime is a weekday
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>True if weekday</returns>
    public static bool IsWeekday(this DateTime dateTime)
    {
        return !dateTime.IsWeekend();
    }

    /// <summary>
    /// Gets the age from a birth date
    /// </summary>
    /// <param name="birthDate">The birth date</param>
    /// <returns>Age in years</returns>
    public static int GetAge(this DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;

        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    /// <summary>
    /// Formats a DateTime in ISO 8601 format
    /// </summary>
    /// <param name="dateTime">The DateTime to format</param>
    /// <returns>ISO 8601 formatted string</returns>
    public static string ToIso8601(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    /// <summary>
    /// Converts a DateTime to Unix timestamp (seconds since epoch)
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>Unix timestamp</returns>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Creates a DateTime from a Unix timestamp
    /// </summary>
    /// <param name="timestamp">The Unix timestamp</param>
    /// <returns>DateTime</returns>
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
    }
}
