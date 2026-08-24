using HabitTracker.Core;
using HabitTracker.Storage;

var dataPath = Environment.GetEnvironmentVariable("HABITTRACKER_DATA")
    ?? Path.Combine(Directory.GetCurrentDirectory(), "habits.json");

IHabitRepository repository = new JsonHabitRepository(dataPath);
var today = DateOnly.FromDateTime(DateTime.Now);

if (args.Length == 0)
{
    PrintUsage();
    return 1;
}

try
{
    return args[0] switch
    {
        "add" => Add(args),
        "log" => Log(args),
        "list" => List(),
        "streak" => Streak(args),
        "report" => Report(),
        "today" => Today(),
        _ => Unknown(args[0]),
    };
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

int Add(string[] a)
{
    if (a.Length != 2)
    {
        Console.Error.WriteLine("usage: habittracker add <name>");
        return 1;
    }

    repository.AddHabit(new Habit(a[1], today));
    Console.WriteLine($"added habit '{a[1]}'");
    return 0;
}

int Log(string[] a)
{
    if (a.Length is < 2 or > 3)
    {
        Console.Error.WriteLine("usage: habittracker log <name> [yyyy-MM-dd]");
        return 1;
    }

    var date = a.Length == 3 ? DateOnly.Parse(a[2]) : today;
    repository.AddEntry(new HabitEntry(a[1], date));
    Console.WriteLine($"logged '{a[1]}' for {date:yyyy-MM-dd}");
    return 0;
}

int List()
{
    if (repository.Habits.Count == 0)
    {
        Console.WriteLine("no habits yet — add one with: habittracker add <name>");
        return 0;
    }

    foreach (var habit in repository.Habits)
    {
        var dates = repository.EntriesFor(habit.Name).Select(e => e.Date).ToList();
        var current = StreakCalculator.CurrentStreak(dates, today);
        var longest = StreakCalculator.LongestStreak(dates);
        Console.WriteLine($"{habit.Name,-20} current: {current,3}   longest: {longest,3}");
    }

    return 0;
}

int Streak(string[] a)
{
    if (a.Length != 2)
    {
        Console.Error.WriteLine("usage: habittracker streak <name>");
        return 1;
    }

    var dates = repository.EntriesFor(a[1]).Select(e => e.Date).ToList();
    Console.WriteLine($"{a[1]}: current streak {StreakCalculator.CurrentStreak(dates, today)}, longest streak {StreakCalculator.LongestStreak(dates)}");
    return 0;
}

int Report()
{
    if (repository.Habits.Count == 0)
    {
        Console.WriteLine("no habits yet — add one with: habittracker add <name>");
        return 0;
    }

    var last7 = Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList();
    Console.WriteLine($"{"habit",-20} {string.Join(" ", last7.Select(d => d.ToString("dd")))}  streak");

    foreach (var habit in repository.Habits)
    {
        var dates = new HashSet<DateOnly>(repository.EntriesFor(habit.Name).Select(e => e.Date));
        var marks = string.Join(" ", last7.Select(d => dates.Contains(d) ? " #" : " ."));
        var current = StreakCalculator.CurrentStreak(dates, today);
        Console.WriteLine($"{habit.Name,-20}{marks}  {current}");
    }

    return 0;
}

int Today()
{
    if (repository.Habits.Count == 0)
    {
        Console.WriteLine("no habits yet — add one with: habittracker add <name>");
        return 0;
    }

    var done = repository.Habits
        .Where(h => repository.EntriesFor(h.Name).Any(e => e.Date == today))
        .Select(h => h.Name)
        .ToList();
    var pending = repository.Habits
        .Select(h => h.Name)
        .Except(done, StringComparer.OrdinalIgnoreCase)
        .ToList();

    if (pending.Count == 0)
    {
        Console.WriteLine("everything logged for today");
        return 0;
    }

    Console.WriteLine("still pending today:");
    foreach (var name in pending)
    {
        Console.WriteLine($"  - {name}");
    }

    return 0;
}

int Unknown(string command)
{
    Console.Error.WriteLine($"unknown command '{command}'");
    PrintUsage();
    return 1;
}

void PrintUsage()
{
    Console.Error.WriteLine("""
        usage: habittracker <command> [args]

        commands:
          add <name>                 add a new habit
          log <name> [yyyy-MM-dd]    log completion (defaults to today)
          list                       show all habits with current/longest streaks
          streak <name>              show one habit's streak detail
          report                     7-day grid of all habits
          today                      habits not yet logged today
        """);
}
