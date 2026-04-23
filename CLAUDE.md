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

## Architecture (5 files, flat)

Keep the whole solution to five source files. No interfaces, no DI, no abstractions beyond what's listed.

```
ConsoleUI.cs        — pre-built static TUI helper class (paste-in at comp)
BreedData.cs        — static Dictionary<string, string[]> species → valid breeds
Animal.cs           — 12 properties + ToLine() + FromLine(string)
AnimalRepository.cs — static class, owns the List<Animal>, file I/O + queries
Program.cs          — Main + menu loop + one method per menu action
```

### `ConsoleUI.cs` — pre-built TUI helper

Static class. Colors are baked into the helpers so the student never thinks about the palette at the call site. This file represents work the student does *before* the comp and pastes in on day one — it's a productivity tool, not a teaching focus.

Surface:
- `SetupConsole()`, `DrawHeader(string title)`, `Pause()`
- `WriteColor(string, ConsoleColor)`, `WriteLineColor(string, ConsoleColor)`
- `Prompt(string label) → string`
- `PromptInt(string label) → int`
- `PromptDouble(string label) → double`
- `PromptDecimal(string label, decimal max) → decimal`
- `PromptDate(string label) → DateTime` (uses `DateTime.TryParseExact` with `"dd/MM/yyyy"`)
- `PromptChoice(string label, string[] options) → string` (numbered, loops on bad input)
- `PrintTable(string[] headers, List<string[]> rows)` (uses `PadRight` for columns)

Color palette (applied inside helpers, never at call site):
- **Cyan** — headers, labels
- **Yellow** — prompts, screen titles, warnings
- **Green** — success messages
- **Red** — errors
- **DarkGray** — secondary text ("press any key")
- **Default** — regular content

Box-drawing characters for `DrawHeader`: `╔ ═ ╗ ║ ╚ ╝`

### `Animal.cs`

12 properties in fixed, documented order. Two methods that are the **teaching centerpiece** of the project: `ToLine()` and `FromLine(string)`.

Field order (document this at the top of the file). This order is how `ToLine` writes and `FromLine` reads — once animals have been saved, it must not change.

1. `Id` — 8-digit zero-padded string (`"00000001"`)
2. `Name` — string
3. `Breed` — must be valid for the species via `BreedData` (`"unknown"` always valid)
4. `Species` — one of: `Dog, Cat, Bird, Rabbit, Small & Furry, Fish, Barnyard, Other`
5. `Birthday` — `DateTime`, formatted `"dd/MM/yyyy"`
6. `SpayedOrNeutered` — `"Yes"` or `"No"`
7. `Gender` — `"F"` or `"M"`
8. `Colour` — string
9. `VaccineStatus` — `"Up to date"`, `"Late"`, or `"Unknown"`
10. `IdentificationNumber` — string, may be empty
11. `IdentificationType` — `"Bar code"`, `"Microchip"`, or `"None"`
12. `AdoptionFee` — `decimal`, must be `< 300`

`ToLine()` walks the properties and builds one pipe-delimited line **character by character** with `StringBuilder`. **No `string.Join`.**

`FromLine(string)` walks the line **character by character** with `StringBuilder`, splitting on `|` and assigning the 12 fields in order. **No `string.Split`.**

The character-walk pattern is written out fully in *both* methods (build vs. read) so the student sees the symmetry. **Do not factor out a shared helper** — seeing the pattern twice is the point.

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

### `BreedData.cs`

Static `Dictionary<string, string[]>` mapping species → valid breeds, plus an `IsValid(species, breed)` helper that returns true for `"unknown"` regardless of species.

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

- **No `string.Split` or `string.Join` in `Animal.cs`.** Serialization uses `StringBuilder` and a **character-by-character walk**. Pattern is written out in both `ToLine` and `FromLine`.
- **No `File.ReadAllLines` / `File.WriteAllLines`.** Use `StreamReader`/`StreamWriter` with `while` loops in `AnimalRepository.cs`.
- **No LINQ in `Animal.cs`** (manual loops only). LINQ *is* allowed in `AnimalRepository.cs` queries (`Where`, `OrderBy`, `GroupBy`) where it aids clarity.
- **`var` is OK in short local scopes** — inside a method body, when the type is obvious from the right-hand side (a `new` expression, a LINQ result, a clear literal). Avoid `var` for fields, properties, and anywhere the inferred type isn't self-evident. Explicit types remain the default; `var` is a shortcut, not the style.
- **No `async`/`await`** — overkill and distracting.
- **One class per file.**
- **Comment every method** with a brief `//` description of what it does. (Teaching context overrides the general "no comments" default.)

## Deliberate simplifications (do not "fix")

These are conscious teaching decisions, not oversights. See `PEDAGOGY.md` for reasoning.

- **Culture-sensitive parsing.** `DateTime.Parse` / `decimal.Parse` in `Animal.cs` use the current machine culture. `InvariantCulture` + `TryParseExact` would be more robust but pulls in a large globalization sub-topic that would derail the core lessons. The app assumes a single consistent machine locale. **Do not retrofit invariant-culture parsing unless the user explicitly asks.** The final `README.md` must include a "Known limitations" note making this assumption explicit.

## UI conventions

Every screen:
- Starts with `ConsoleUI.DrawHeader(title)` (clears screen, redraws double-line Unicode box).
- Uses numbered menus only — no hotkeys, no arrow keys.
- Ends with `ConsoleUI.Pause()` before returning to the menu.
- Uses `ConsoleUI.PrintTable` for tabular output, not hand-rolled padding.

## Features (base / high-school tier)

Add animal (with breed validation against species) · Remove by ID · Search by name or species · Display sorted by species · Display three oldest per species · Help/usage.

**Post-secondary tier** adds archive/restore and fee auto-calculation. **Do not build these until the base tier is complete and working.**

## Build order — thin vertical slice

The goal is that **every step produces a runnable program**. Confront the hardest part (the manual character walk) early, while motivation is high, instead of after a week of UI chrome.

1. **`ConsoleUI.cs`** — full file. It's the paste-in tool, so build it once, completely.
2. **`Animal.cs`** — properties only, plus stub `ToLine`/`FromLine` returning placeholders so the project compiles.
3. **`BreedData.cs`** — populate one species fully (e.g. Dog), the rest later.
4. **`AnimalRepository.cs`** — only `LoadFromFile`, `SaveToFile`, `Add`, `NextId` to start.
5. **`Program.cs`** — `Main` + only the **Add** menu action wired end-to-end. Manually inspect `animals.txt` to confirm the format looks right.
6. Flesh out `ToLine`/`FromLine` with the real character walk. Verify load round-trips correctly.
7. Add Remove, Search, Display screens one at a time. Test each before moving to the next.
8. Help screen + polish.
9. `README.md` + screenshots last.

## Deliverables

Source + `bin\Release\*.exe` + `README.md` (usage, features, screenshots of Add/Delete/Edit and other major screens), all zipped as `FirstName_LastName.zip`.
