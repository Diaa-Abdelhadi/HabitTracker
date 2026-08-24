using System.Text.Json;
using HabitTracker.Core;

namespace HabitTracker.Storage;

public sealed class JsonHabitRepository : IHabitRepository
{
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    private readonly string _path;
    private readonly List<Habit> _habits;
    private readonly List<HabitEntry> _entries;

    public JsonHabitRepository(string path)
    {
        _path = path;
        var (habits, entries) = Load(path);
        _habits = habits;
        _entries = entries;
    }

    public IReadOnlyList<Habit> Habits => _habits;
    public IReadOnlyList<HabitEntry> Entries => _entries;

    public void AddHabit(Habit habit)
    {
        if (_habits.Any(h => h.Name.Equals(habit.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"habit '{habit.Name}' already exists");
        }

        _habits.Add(habit);
        Persist();
    }

    public void AddEntry(HabitEntry entry)
    {
        if (!_habits.Any(h => h.Name.Equals(entry.HabitName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"unknown habit '{entry.HabitName}' — add it first");
        }

        if (!_entries.Contains(entry))
        {
            _entries.Add(entry);
            Persist();
        }
    }

    public IReadOnlyList<HabitEntry> EntriesFor(string habitName) =>
        _entries.Where(e => e.HabitName.Equals(habitName, StringComparison.OrdinalIgnoreCase)).ToList();

    private static (List<Habit> Habits, List<HabitEntry> Entries) Load(string path)
    {
        if (!File.Exists(path))
        {
            return ([], []);
        }

        var json = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(json))
        {
            return ([], []);
        }

        var data = JsonSerializer.Deserialize<StoredData>(json)
            ?? throw new InvalidOperationException($"'{path}' did not contain valid habit data");

        return (data.Habits.ToList(), data.Entries.ToList());
    }

    private void Persist()
    {
        var data = new StoredData(_habits, _entries);
        var json = JsonSerializer.Serialize(data, WriteOptions);
        File.WriteAllText(_path, json);
    }

    private sealed record StoredData(List<Habit> Habits, List<HabitEntry> Entries);
}
