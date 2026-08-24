using HabitTracker.Core;
using Xunit;

namespace HabitTracker.Tests;

public class StreakCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 8, 24);

    [Fact]
    public void No_entries_gives_zero_current_streak()
    {
        Assert.Equal(0, StreakCalculator.CurrentStreak([], Today));
    }

    [Fact]
    public void Completed_today_gives_streak_of_one()
    {
        Assert.Equal(1, StreakCalculator.CurrentStreak([Today], Today));
    }

    [Fact]
    public void Three_consecutive_days_ending_today_gives_streak_of_three()
    {
        var dates = new[] { Today.AddDays(-2), Today.AddDays(-1), Today };

        Assert.Equal(3, StreakCalculator.CurrentStreak(dates, Today));
    }

    [Fact]
    public void Streak_still_counts_if_not_yet_logged_today_but_completed_yesterday()
    {
        var dates = new[] { Today.AddDays(-2), Today.AddDays(-1) };

        Assert.Equal(2, StreakCalculator.CurrentStreak(dates, Today));
    }

    [Fact]
    public void Gap_of_two_days_resets_current_streak_to_zero()
    {
        var dates = new[] { Today.AddDays(-3) };

        Assert.Equal(0, StreakCalculator.CurrentStreak(dates, Today));
    }

    [Fact]
    public void Old_streak_before_a_gap_does_not_extend_a_new_streak()
    {
        var dates = new[] { Today.AddDays(-10), Today.AddDays(-9), Today.AddDays(-8), Today };

        Assert.Equal(1, StreakCalculator.CurrentStreak(dates, Today));
    }

    [Fact]
    public void Duplicate_dates_are_not_double_counted()
    {
        var dates = new[] { Today, Today };

        Assert.Equal(1, StreakCalculator.CurrentStreak(dates, Today));
    }

    [Fact]
    public void Longest_streak_finds_the_best_run_even_after_a_break()
    {
        var dates = new[]
        {
            Today.AddDays(-20), Today.AddDays(-19), Today.AddDays(-18), Today.AddDays(-17), Today.AddDays(-16),
            Today.AddDays(-2), Today.AddDays(-1),
        };

        Assert.Equal(5, StreakCalculator.LongestStreak(dates));
    }

    [Fact]
    public void Longest_streak_with_no_entries_is_zero()
    {
        Assert.Equal(0, StreakCalculator.LongestStreak([]));
    }

    [Fact]
    public void Longest_streak_ignores_date_order_in_the_input()
    {
        var dates = new[] { Today, Today.AddDays(-2), Today.AddDays(-1) };

        Assert.Equal(3, StreakCalculator.LongestStreak(dates));
    }
}
