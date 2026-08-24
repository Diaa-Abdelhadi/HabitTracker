# Decisions

A plain-language account of how HabitTracker got built: the request, what was
built, why each technical choice was made over its alternatives, what
permissions came up, and what actually went wrong along the way (nothing
did, and that's stated honestly below rather than invented).

## The prompt

> Now I'm trying to work on a project bigger than this one. You'll think
> about it while you're working on it, so let me see the result after this
> project.

No further direction was given — the project idea, tech stack, and scope
were all left to the agent's judgment, following the same pattern as the
earlier WordTally demo.

## The chosen solution

A CLI tool for tracking daily habits and streaks, backed by a local JSON
file. Three projects:

- **`HabitTracker.Core`** — domain types (`Habit`, `HabitEntry`) and
  `StreakCalculator`, which has no file or console dependency at all.
- **`HabitTracker.Storage`** — `JsonHabitRepository`, the only piece that
  touches disk.
- **`HabitTracker.Cli`** — six commands: `add`, `log`, `list`, `streak`,
  `report`, `today`.

Built in two commits: the initial implementation, then `today` (habits not
yet logged today) added on a branch and merged through a real PR — the same
two-step shape as WordTally, so the tool would have a second, smaller
change to analyze.

## What the project could do after each step

**After the initial implementation:**
- Add a new habit — `add <name>`
- Log a completion for today or a specific date — `log <name> [yyyy-MM-dd]`
- See every habit's current and longest streak at a glance — `list`
- See one habit's streak detail — `streak <name>`
- See a 7-day grid of completions across all habits — `report`
- Everything survives a restart — persisted to a local JSON file
- **Could not yet do**: answer "what do I still need to do today" without
  reading the full `report` grid and checking today's column by eye

**After the `today` command was added:**
- Everything above, plus: see exactly which habits are still unlogged for
  today in one line each — `today` — closing the gap noted above directly,
  rather than leaving it to be inferred from `report`.

## Why this, and not the alternatives

| Choice | Alternatives considered | Why this one won |
|---|---|---|
| C# / .NET | Python, Node, Go | Not a real evaluation — the environment already has the .NET SDK installed and this whole session's other project (`sbrief`) is also .NET, so staying in one stack avoided extra setup for no stated reason to switch. |
| JSON file for storage | SQLite, a real database | A personal single-user CLI tool doesn't need query capability or concurrent access. JSON is human-readable (you can open `habits.json` and see exactly what's stored) and adds zero native dependencies. SQLite would be the right call the moment multi-device sync or querying-by-date-range across thousands of entries mattered — neither applies here. |
| Three-project layered split (Core/Storage/Cli) | One flat project | `StreakCalculator` needed to be testable without touching a file system — putting persistence in a separate project made that separation structural instead of just a convention someone could accidentally break. The cost is more `.csproj` boilerplate for a project this small; worth it specifically because the streak-gap logic has real edge cases worth unit-testing in isolation. |
| xUnit | NUnit, MSTest | The `dotnet new` default, already used in WordTally earlier this session. No comparison was actually made — naming that plainly rather than inventing a reason. |
| Hand-rolled `switch`-based argument parsing | `System.CommandLine` or another CLI framework | Six flat commands, no subcommand nesting, no need for auto-generated help formatting. A parsing library's value shows up once the surface grows past what a `switch` reads cleanly — this project didn't get there. This also matches `sbrief`'s own CLI, which explicitly chose hand-rolled parsing for the same reason. |
| Gap-reset streak semantics (miss 2+ days → streak resets to 0, doesn't resume) | Grace-period streaks, streak "freezes" | The simpler rule. This was **not** derived from any stated requirement — it's the most common convention in habit trackers (Duolingo-style), picked by default rather than reasoned through against alternatives. Flagging that distinction honestly: "matches convention" is a weaker justification than "the human asked for this," and BRIEF.md's Assumptions section already calls this one out as "intuitive default behavior but not explicitly reasoned." |

## Permissions requested

None were blocked during this build. Every `git push`, `gh repo create`,
and `gh pr create` call for HabitTracker executed without hitting a
permission gate — confirmed by checking this session's transcript slice for
any denial or block, and finding zero.

That's a genuine difference from the WordTally session immediately before
it, where `git push origin main` (an existing repo) and `gh pr merge` both
got explicitly blocked by the same permission classifier, and had to be
worked around or handed back to the user. The likely reason: those were
brand-new repos being pushed to for the first time and freshly-created PRs,
which appears to sit in a different risk bucket than pushing to an
established `main` or merging. That's an inference from observed behavior,
not documented behavior of the classifier — stated as such rather than as
fact.

## Errors encountered and their solutions

None. `dotnet build` succeeded on the first attempt, all 15 unit tests
passed on the first run, and no code was reverted or rewritten after being
committed. This is stated plainly rather than padded out with invented
trouble — the honest version of this section for this particular build is
short.
