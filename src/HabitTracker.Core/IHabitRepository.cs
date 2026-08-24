namespace HabitTracker.Core;

public interface IHabitRepository
{
    IReadOnlyList<Habit> Habits { get; }
    IReadOnlyList<HabitEntry> Entries { get; }

    void AddHabit(Habit habit);
    void AddEntry(HabitEntry entry);
    IReadOnlyList<HabitEntry> EntriesFor(string habitName);
}
