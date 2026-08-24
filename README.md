# HabitTracker

A small CLI for tracking daily habits and streaks, backed by a local JSON file.

```
$ habittracker add reading
added habit 'reading'

$ habittracker log reading
logged 'reading' for 2026-08-24

$ habittracker report
habit                18 19 20 21 22 23 24  streak
reading              .  .  .  .  #  #  #  3
exercise             .  .  #  .  .  .  .  0
```

## Commands

| Command | Description |
|---|---|
| `add <name>` | Add a new habit |
| `log <name> [yyyy-MM-dd]` | Log a completion (defaults to today) |
| `list` | Show all habits with current/longest streaks |
| `streak <name>` | Show one habit's streak detail |
| `report` | 7-day grid of all habits |

## Data

Stored as JSON at `./habits.json` in the current directory, or wherever
`HABITTRACKER_DATA` points.

## Streak rules

- **Current streak**: consecutive days ending today or yesterday. Miss two
  days in a row and it resets to zero — logging today after that starts a
  new streak of 1, it does not resume the old one.
- **Longest streak**: the best run anywhere in the history, independent of
  whether it's still active.

## Building

```bash
dotnet build
dotnet test
```
