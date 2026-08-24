namespace HabitTracker.Core;

public static class StreakCalculator
{
    /// <summary>
    /// The number of consecutive days, ending today or yesterday, that a habit was completed.
    /// A gap of two or more days resets this to zero — completing a habit today after missing
    /// yesterday starts a new streak of 1, it does not resume the old one.
    /// </summary>
    public static int CurrentStreak(IReadOnlyCollection<DateOnly> completedDates, DateOnly asOf)
    {
        var dates = new HashSet<DateOnly>(completedDates);
        if (dates.Count == 0)
        {
            return 0;
        }

        var cursor = dates.Contains(asOf) ? asOf : asOf.AddDays(-1);
        if (!dates.Contains(cursor))
        {
            return 0;
        }

        var streak = 0;
        while (dates.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    /// <summary>The longest run of consecutive completed days anywhere in the history.</summary>
    public static int LongestStreak(IReadOnlyCollection<DateOnly> completedDates)
    {
        if (completedDates.Count == 0)
        {
            return 0;
        }

        var sorted = completedDates.Distinct().OrderBy(d => d).ToList();
        var longest = 1;
        var current = 1;

        for (var i = 1; i < sorted.Count; i++)
        {
            current = sorted[i] == sorted[i - 1].AddDays(1) ? current + 1 : 1;
            longest = Math.Max(longest, current);
        }

        return longest;
    }
}
