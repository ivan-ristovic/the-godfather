namespace TheGodfather.Extensions;

public static class DayOfWeekExtensions
{
    public static TimeSpan Until(this DayOfWeek currentDay, DayOfWeek targetDay)
        => TimeSpan.FromDays((7 + (targetDay - currentDay)) % 7);
}