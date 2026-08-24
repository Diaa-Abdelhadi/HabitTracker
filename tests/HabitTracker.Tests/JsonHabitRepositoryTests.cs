using HabitTracker.Core;
using HabitTracker.Storage;
using Xunit;

namespace HabitTracker.Tests;

public class JsonHabitRepositoryTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"habits-{Guid.NewGuid()}.json");

    [Fact]
    public void Adding_a_habit_persists_it_across_a_new_instance()
    {
        new JsonHabitRepository(_path).AddHabit(new Habit("reading", new DateOnly(2026, 8, 1)));

        var reloaded = new JsonHabitRepository(_path);

        Assert.Single(reloaded.Habits);
        Assert.Equal("reading", reloaded.Habits[0].Name);
    }

    [Fact]
    public void Adding_a_duplicate_habit_name_throws()
    {
        var repo = new JsonHabitRepository(_path);
        repo.AddHabit(new Habit("reading", new DateOnly(2026, 8, 1)));

        Assert.Throws<InvalidOperationException>(() => repo.AddHabit(new Habit("Reading", new DateOnly(2026, 8, 2))));
    }

    [Fact]
    public void Logging_an_entry_for_an_unknown_habit_throws()
    {
        var repo = new JsonHabitRepository(_path);

        Assert.Throws<InvalidOperationException>(() =>
            repo.AddEntry(new HabitEntry("reading", new DateOnly(2026, 8, 1))));
    }

    [Fact]
    public void Logging_the_same_entry_twice_does_not_duplicate_it()
    {
        var repo = new JsonHabitRepository(_path);
        repo.AddHabit(new Habit("reading", new DateOnly(2026, 8, 1)));
        var entry = new HabitEntry("reading", new DateOnly(2026, 8, 2));

        repo.AddEntry(entry);
        repo.AddEntry(entry);

        Assert.Single(repo.EntriesFor("reading"));
    }

    [Fact]
    public void Missing_data_file_starts_empty_instead_of_throwing()
    {
        var repo = new JsonHabitRepository(_path);

        Assert.Empty(repo.Habits);
        Assert.Empty(repo.Entries);
    }

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }
}
