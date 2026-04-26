# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Purpose and audience

This is a **reference solution** for the Skills Ontario Coding competition (Animal Rescue Management System). A high school teacher is building it to teach a competing student. The student codes slowly, so **simplicity, readability, and copy-friendly patterns matter more than cleverness or idiomatic .NET**. Optimize explanations and code shape for a novice reader.

## Hard competition constraints (non-negotiable)

- **.NET Framework 4.7.2** (not .NET Core / .NET 5+). This is chosen so the build produces a single standalone Windows `.exe`.
- **Standard libraries only.** No NuGet packages, no external references beyond what's already in the `.csproj`.
- **Plain `System.Console` only** — no TUI libraries, no arrow-key menus, no mouse, no ANSI tricks beyond `Console.ForegroundColor`.
- Must **open in Visual Studio** and compile to a standalone `.exe`.
- **The app must never crash.** Every menu action is wrapped in try/catch at the menu level.
- The student writes everything himself at the comp. **No AI-generated code is allowed at the competition.** This repo is the *reference* he studies and practices from beforehand.

## Build

Build from Visual Studio, or from the command line via MSBuild (Developer Command Prompt):

```
msbuild SkillsOntarioSampleProject2026.sln /p:Configuration=Release
```

Release output lands in `bin\Release\SkillsOntarioSampleProject2026.exe`. That `.exe` is the competition deliverable.

There is no test project and none is planned — out of scope for the competition and the student's time budget.

## Architecture (4 files, flat — base tier)

Keep the base solution to four source files. No interfaces, no DI, no abstractions beyond what's listed. The optional Part 9 extension adds four more files (`Adopter.cs`, `Adoption.cs`, `AdopterRepository.cs`, `AdoptionRepository.cs`) — same shape, mirroring the animal pair.

```
ConsoleUI.cs        — pre-built static TUI helper class (paste-in at comp)
Animal.cs           — 12 properties + ToLine() + FromLine(string)
AnimalRepository.cs — static class, owns the List<Animal>, file I/O + queries
Program.cs          — Main + menu loop + one method per menu action
```

### `ConsoleUI.cs` — pre-built TUI helper

Static class. Colors are baked into the helpers so the student never thinks about the palette at the call site. This file represents work the student does *before* the comp and pastes in on day one — it's a productivity tool, not a teaching focus.

Surface:
- `SetupConsole()`, `DrawHeaders(string message)`, `Pause()`
- `WriteColor(string, ConsoleColor)`, `WriteLineColor(string, ConsoleColor)`
- `Prompt(string label) → string`
- `PromptInt(string label) → int`
- `PromptDouble(string label) → double`
- `PromptDecimal(string label) → decimal` and `PromptDecimal(string label, decimal max) → decimal` (the overload enforces `value <= max`; use it for bounded values like AdoptionFee)
- `PromptDateTime(string label) → DateTime` (uses `DateTime.TryParse`; loops on bad input. Single-locale assumption — see "Deliberate simplifications" below.)
- `PromptChoice(string label, string[] options) → string` (numbered prompt, loops on bad input. Returns the **selected option text** — i.e. `options[choice - 1]` — so callers can assign the result directly to the field they're collecting: `animal.Gender = ConsoleUI.PromptChoice("Select gender:", new string[] { "Male", "Female" });`. No 1-based/0-based mapping needed at the call site.)
- `PrintTable(string[] headers, List<string[]> rows)` (uses `PadRight` for columns; see "Calling PrintTable from a display screen" below for the calling pattern)

`DrawHeaders` always shows the app brand (`AppName` constant) inside the box and prints the caller's `message` as a dark-cyan subtitle underneath, followed by a thin separator. This keeps every screen visually anchored to the app while letting the subtitle say what the current screen is for ("All animals — sorted by species", "Add a new animal", etc.).

Color palette (applied inside helpers, never at call site):
- **Cyan** — headers, labels
- **Yellow** — prompts, screen titles, warnings
- **Green** — success messages
- **Red** — errors
- **DarkGray** — secondary text ("press any key")
- **Default** — regular content

Box-drawing characters for `DrawHeaders`: `╔ ═ ╗ ║ ╚ ╝`, plus `─` for the subtitle separator.

### `Animal.cs`

12 properties in fixed, documented order. Two methods that are the **teaching centerpiece** of the project: `ToLine()` and `FromLine(string)`.

Field order (document this at the top of the file). This order is how `ToLine` writes and `FromLine` reads — once animals have been saved, it must not change.

1. `Id` — 8-digit zero-padded string (`"00000001"`)
2. `Name` — string
3. `Breed` — free-text via `ConsoleUI.Prompt`. No closed-list validation in the base tier; the user types whatever they want (`"Labrador Retriever"`, `"Mixed"`, `"unknown"`, etc.). A previous draft proposed a `BreedData.cs` lookup with `PromptChoice`-driven validation, but it was dropped from scope.
4. `Species` — one of: `Dog, Cat, Bird, Rabbit, Small & Furry, Fish, Barnyard, Other`
5. `Birthday` — `DateTime`, written via `ToShortDateString()` (machine's short-date pattern) so the file matches whatever the machine reads back with `DateTime.Parse`
6. `SpayedOrNeutered` — `"Yes"` or `"No"`
7. `Gender` — `"Male"` or `"Female"`
8. `Colour` — string
9. `VaccineStatus` — `"Up to date"`, `"Late"`, or `"Unknown"`
10. `IdentificationNumber` — string, may be empty
11. `IdentificationType` — `"Bar code"`, `"Microchip"`, or `"None"`
12. `AdoptionFee` — `decimal`, must be `< 300`

`ToLine()` builds one pipe-delimited line using string interpolation. `FromLine(string)` parses one such line using `string.Split('|')` and assigns the 12 fields in positional order. Keep both methods short and readable — they're the persistence boundary, and clarity matters more than novelty.

> **Note (was previously a teaching rule).** Earlier drafts mandated a manual `StringBuilder` character-walk in both methods. That rule was dropped: the framework versions are clearer, less error-prone, and ship faster. The "build a parser by hand" exercise can live in a future personal project; it isn't earning its keep here.

### `AnimalRepository.cs`

**Static class** (not instantiated — there's only ever one). Owns the in-memory list and the file path.

```csharp
public static List<Animal> animals = new List<Animal>();
private const string FilePath = "animals.txt";
```

The list is **`public`** so screens in `Program.cs` can iterate it directly as `AnimalRepository.animals`. We trust callers not to corrupt it — there's only one repository in this codebase and the surface is small. No `GetAll()` wrapper.

Methods:
- `LoadFromFile()` — `if (File.Exists(FilePath))` guard, then `using (StreamReader sr = new StreamReader(FilePath))` + `while ((line = sr.ReadLine()) != null) animals.Add(Animal.FromLine(line));`
- `SaveToFile()` — `using (StreamWriter sw = new StreamWriter(FilePath))` + `foreach` writing `animal.ToLine()`
- `AddAnimal(Animal)` — assigns `Id` via `NextId()`, appends to list
- `RemoveById(string id)` — find + remove
- `SearchByName(string)` / `SearchBySpecies(string)` — LINQ `Where` allowed
- `GetSortedBySpecies()` — LINQ `OrderBy` allowed
- `GetThreeOldestPerSpecies()` — LINQ `GroupBy` + `OrderBy(Birthday)` + `Take(3)`
- `NextId()` — **private**. Scans the list for the max numeric `Id`, adds 1, formats `D8`. Only `AddAnimal` calls it; callers never assign IDs themselves.

### `Program.cs`

```csharp
class Program
{
    static void Main(string[] args)
    {
        ConsoleUI.SetupConsole();
        AnimalRepository.LoadFromFile();
        bool running = true;
        while (running)
        {
            try { running = ShowMenu(); }
            catch (Exception ex)
            {
                ConsoleUI.WriteLineColor("Error: " + ex.Message, ConsoleColor.Red);
                ConsoleUI.Pause();
            }
        }
    }

    // one static method per menu action: AddAnimal, RemoveAnimal, Search, Display, Help, etc.
}
```

## Persistence model

- **Load once** at startup (missing file = empty list, no error).
- **Save after every mutation**, as **two explicit lines** in the menu code:
  ```csharp
  AnimalRepository.AddAnimal(animal);
  AnimalRepository.SaveToFile();
  ```
  Do **not** hide `SaveToFile` inside `AddAnimal`. The student needs to *see* that "change in memory" and "write to disk" are separate operations — that's half the file-I/O lesson.
- File is `animals.txt` next to the `.exe`, **pipe (`|`) delimited**, fields in the order listed above. Presented to users as "pipe-separated plain text".

## Teaching-driven coding rules (read carefully — these are unusual)

These rules exist because judges and the teacher want to see manual work, not concise idiomatic C#. Breaking them defeats the point of the reference solution.

- **No `File.ReadAllLines` / `File.WriteAllLines`.** Use `StreamReader`/`StreamWriter` with `while` loops in `AnimalRepository.cs`. (Reading line-by-line in a loop is the lesson here; the in-`Animal.cs` parsing of each line uses framework helpers like `string.Split` deliberately for clarity.)
- **No LINQ in `Animal.cs`** (manual loops only). LINQ *is* allowed in `AnimalRepository.cs` queries (`Where`, `OrderBy`, `GroupBy`) where it aids clarity.
- **`var` is OK in short local scopes** — inside a method body, when the type is obvious from the right-hand side (a `new` expression, a LINQ result, a clear literal). Avoid `var` for fields, properties, and anywhere the inferred type isn't self-evident. Explicit types remain the default; `var` is a shortcut, not the style.
- **No `async`/`await`** — overkill and distracting.
- **One class per file.**
- **Comment every method** with a brief `//` description of what it does. (Teaching context overrides the general "no comments" default.)

## Deliberate simplifications (do not "fix")

These are conscious teaching decisions, not oversights. See `PEDAGOGY.md` for reasoning.

- **Culture-sensitive parsing throughout.** Both the interactive prompt (`ConsoleUI.PromptDateTime` → `DateTime.TryParse`) and the persistence layer (`Animal.cs` → `DateTime.Parse` / `decimal.Parse`) use the current machine culture. The single-machine assumption is the load-bearing simplification: the same machine that types data into the prompt also writes the file and reads it back, so producer and consumer agree on what `15/03/2020` (or whatever the machine's short-date pattern emits) means. `ToLine` uses `Birthday.ToShortDateString()` rather than a hardcoded format string so the file is written in whatever short-date pattern the machine uses; `FromLine` then reads it with the same machine's `DateTime.Parse`. **Do not retrofit invariant-culture parsing.** A previous attempt did exactly that and traded one bug (writer/reader format mismatch on en-CA Windows) for another (a hand-seeded `animals.txt` that no longer round-tripped). Trust the same-machine assumption — it's a teaching project, not a production system. The final `README.md` should include a "Known limitations" note saying the data file is **machine-culture-specific**: copy the `.exe` to a machine with a different short-date or decimal pattern and the file won't load.

## UI conventions

Every screen:
- Starts with `ConsoleUI.DrawHeaders(subtitle)` (clears screen, redraws the branded double-line Unicode box, prints the subtitle beneath).
- Uses numbered menus only — no hotkeys, no arrow keys.
- Ends with `ConsoleUI.Pause()` before returning to the menu.
- Uses `ConsoleUI.PrintTable` for tabular output, not hand-rolled padding.

### Calling `PrintTable` from a display screen

This is new territory for the student. `PrintTable` is domain-agnostic — it only understands `string[] headers` and `List<string[]> rows`, so each display screen is responsible for turning its animals into that shape. Rationale for why this lives in the screen, not on `Animal`: different screens show different columns (Display All wants ID/Name/Species/Breed; Three Oldest wants Birthday in there; Search results may show the field that matched), so there is no single canonical row for an animal. A `ToRow()` method on `Animal` would either force every screen to look identical or sprawl into `ToRowShort` / `ToRowFull` / `ToRowSearch` — worse than a little repetition. `Animal.ToLine()` is the opposite situation: one frozen format for one customer (the file), so it earns its own method.

Walk the student through the pattern in four explicit steps, **in this order**, every time a display screen is introduced in the workbook:

1. **Declare the column headers as a local `string[]`.** Keep this at the top of the method so the reader sees what columns the screen shows without jumping to another file.
2. **Create an empty `List<string[]> rows = new List<string[]>();`.** A `List` is used because row count is unknown until the loop finishes — arrays would force a second pass to count first.
3. **Loop through the data (filtered / sorted as the screen requires). Inside the loop, build a `string[]` for the current animal and `rows.Add(...)` it.** The fields inside the row array **must** appear in the same order as the headers — this is parallel arrays. Call this out: index 0 of the row lines up with index 0 of the headers, index 1 with index 1, and so on. Mismatched order is the #1 bug in this pattern.
4. **Call `ConsoleUI.PrintTable(headers, rows);`.** One line. `PrintTable` handles the width measurement, padding, header color, and separator — the screen does not.

Canonical shape to show the student (use this exact skeleton, adapted per screen):

```csharp
private static void DisplayAllSortedBySpecies()
{
    ConsoleUI.DrawHeaders("All animals — sorted by species");

    // 1. Headers for this screen.
    string[] headers = new string[] { "ID", "Name", "Species", "Breed" };

    // 2. An empty list to hold the rows we build.
    List<string[]> rows = new List<string[]>();

    // 3. One row per animal, fields in the same order as the headers.
    foreach (Animal a in AnimalRepository.animals.OrderBy(a => a.Species))
    {
        rows.Add(new string[] { a.Id, a.Name, a.Species, a.Breed });
    }

    // 4. Hand off to the generic helper.
    ConsoleUI.PrintTable(headers, rows);

    ConsoleUI.Pause();
}
```

Teaching notes to repeat when the student hits each display screen:
- **The duplication across screens is intentional.** Three screens all typing `"ID", "Name", "Species"` is fine; abstracting it into a shared helper would hide which columns belong to which screen.
- **`var rows = new List<string[]>()` is acceptable here** per the `var` rule — the type is obvious from the right-hand side, and it's a short local scope.
- **Do not call `PrintTable` with zero rows without checking first.** If the filtered list is empty, print a `WriteLineColor("No animals found.", ConsoleColor.Yellow)` message instead — an empty table with just headers is confusing.

## Features (base / high-school tier)

Add animal · Remove by ID · Search by name or species · Display sorted by species · Display three oldest per species · Help/usage.

**Post-secondary tier** adds archive/restore and fee auto-calculation. **Do not build these until the base tier is complete and working.**

## Build order — thin vertical slice

The goal is that **every step produces a runnable program**.

1. **`ConsoleUI.cs`** — full file. It's the paste-in tool, so build it once, completely.
2. **`Animal.cs`** — properties + `ToLine`/`FromLine` using string interpolation and `string.Split`. No further rewrite planned.
3. **`AnimalRepository.cs`** — only `LoadFromFile`, `SaveToFile`, `Add`, `NextId` to start. Inline LINQ at the screen call sites is fine; promote a query into a named repository method only when it has a domain meaning (e.g. `GetActiveAdoptionForAnimal`).
4. **`Program.cs`** — `Main` + only the **Add** menu action wired end-to-end. Manually inspect `animals.txt` to confirm the format looks right.
5. Add Remove, Search, Display screens one at a time. Test each before moving to the next.
6. Help screen + polish.
7. `README.md` + screenshots last.

## Deliverables

Source + `bin\Release\*.exe` + `README.md` (usage, features, screenshots of Add/Delete/Edit and other major screens), all zipped as `FirstName_LastName.zip`.
