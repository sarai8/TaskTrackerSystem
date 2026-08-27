namespace TaskTrackerSystem.Web.Common;

public static class DateDiffFormatter
{
    public static string FormatRemaining(DateTime from, DateTime to)
    {
        if (to <= from)
            return "Süre geçti";

        var years = to.Year - from.Year;
        var months = to.Month - from.Month;
        var days = to.Day - from.Day;
        var hours = to.Hour - from.Hour;
        var minutes = to.Minute - from.Minute;

        if (minutes < 0) { minutes += 60; hours--; }
        if (hours < 0) { hours += 24; days--; }

        if (days < 0)
        {
            var prevMonth = to.AddMonths(-1);
            days += DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            months--;
        }

        if (months < 0) { months += 12; years--; }

        var parts = new List<string>();

        if (years > 0) parts.Add($"{years}yıl");
        if (months > 0) parts.Add($"{months}ay");
        if (days > 0) parts.Add($"{days}g");
        if (hours > 0) parts.Add($"{hours}sa");

        parts.Add($"{minutes}dk");

        return string.Join(" ", parts) + " kaldı";
    }

    public static string FormatDuration(int totalMinutes)
    {
        var days = totalMinutes / (60 * 24);
        var hours = (totalMinutes % (60 * 24)) / 60;
        var minutes = totalMinutes % 60;

        var parts = new List<string>();

        if (days > 0) parts.Add($"{days}g");
        if (hours > 0) parts.Add($"{hours}sa");

        parts.Add($"{minutes}dk");

        return string.Join(" ", parts);
    }
}