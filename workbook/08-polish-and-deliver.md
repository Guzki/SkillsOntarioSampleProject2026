# Part 8 — Polish and Deliver

> **Prerequisites:** Parts 0 through 7 are complete. Every menu action — Add, Remove, Search, Display All, Three Oldest, Exit — works end-to-end with persistence. The only screen that hasn't been brought up to the `ConsoleUI` standard is **Help**.

## What this part finishes

This is the wrap-up. No new logic, no new screens. Just five housekeeping tasks that turn a working demo into a competition deliverable:

1. **Help screen retrofit** — bring it up to the same `ConsoleUI` palette every other screen uses.
2. **Stale comment cleanup** — file headers and TODOs left over from earlier parts.
3. **README.md** — write the public-facing readme: features list, screenshots, build instructions, "Known limitations" note.
4. **Release build + standalone test** — the same fresh-folder sanity check from Part 4, but final this time.
5. **Final zip** — `FirstName_LastName.zip` containing source + `bin\Release\` + `README.md`. This is what gets handed in.

---

## Part 8a — Retrofit the Help screen

Help is currently raw `Console.WriteLine` from before `ConsoleUI` existed. It's the only screen that doesn't have a branded header, doesn't end with `Pause`, and doesn't use any of the palette colors. That's been bothering you (or me) since Part 5 — fix it now.

### Current state

```csharp
private static void ShowHelp()
{
    Console.WriteLine("Animal Rescue Management System — Help");
    Console.WriteLine();
    Console.WriteLine("Menu options:");
    Console.WriteLine("  1. Add animal — prompts for details and saves a new animal.");
    // ... etc
    Console.WriteLine("Data is stored in animals.txt next to this .exe.");
}
```

It works, but it doesn't look like the rest of the app.

### Retrofitted version

```csharp
// Prints a help screen summarising the menu and where the save file lives.
// Uses the same ConsoleUI palette as every other screen for a consistent look.
private static void ShowHelp()
{
    ConsoleUI.DrawHeaders("Help");

    ConsoleUI.WriteLineColor("Animal Rescue Management System — Help", ConsoleColor.Cyan);
    Console.WriteLine();
    ConsoleUI.WriteLineColor("Menu options:", ConsoleColor.Cyan);
    Console.WriteLine("  1. Add animal — prompts for details and saves a new animal.");
    Console.WriteLine("  2. Remove animal — deletes an animal by its ID.");
    Console.WriteLine("  3. Search — finds animals by name or species (substring match).");
    Console.WriteLine("  4. Display all (sorted by species) — lists every animal.");
    Console.WriteLine("  5. Display three oldest per species — three oldest within each species.");
    Console.WriteLine("  6. Help — this screen.");
    Console.WriteLine("  7. Exit — closes the program.");
    Console.WriteLine();
    ConsoleUI.WriteLineColor("Data file:", ConsoleColor.Cyan);
    Console.WriteLine("  Animals are stored in animals.txt next to this .exe.");
    Console.WriteLine("  The save file is plain text, pipe-delimited (one animal per line).");
    Console.WriteLine();
    ConsoleUI.WriteLineColor("Date format:", ConsoleColor.Cyan);
    Console.WriteLine("  Birthdays use this machine's short-date pattern.");
    Console.WriteLine("  Copying animals.txt to a machine with a different locale may not load cleanly.");

    ConsoleUI.Pause();
}
```

### Three small changes

1. **`ConsoleUI.DrawHeaders("Help")` at the top** — the branded box. Help is now visually anchored to the rest of the app instead of looking like a different program.
2. **`ConsoleUI.WriteLineColor` for section headers** — the "Menu options:", "Data file:", and "Date format:" lines are cyan because they label sections. The body text underneath each stays default-colored, like a list of paragraphs under a heading.
3. **`ConsoleUI.Pause()` at the end** — every other screen does this. Without it, Help would print and instantly return to the menu, which is a screen flash too fast to read.

The body text gains a "Date format" section that mentions the single-machine assumption. That's the right place for the warning — anyone reading Help is looking for caveats, and "the file might not move between machines" is a real one.

### Test

- ✅ Choose menu option 6 → branded header shows "Help" subtitle
- ✅ Section labels in cyan, body text default
- ✅ Press any key → menu returns

---

## Part 8b — Stale comment cleanup

Walk through every `.cs` file and update any out-of-date file-header references. Specifically:

- **`Program.cs:6`** — "Several screens (Remove, Search, DisplayThreeOldestPerSpecies) are stubs and will be fleshed out in upcoming workbook parts." Outdated; replace with something like:

  ```csharp
  // Application entry point. Owns the main menu loop and one method per
  // menu action. Every screen is implemented and uses the ConsoleUI helpers.
  // Reference: workbook/04-add-screen.md and onwards
  ```

- **`Animal.cs`** — verify the file-header comment matches the current implementation (string interpolation in `ToLine`, `string.Split` in `FromLine`, `ToShortDateString()` for the date). If it references PEDAGOGY.md for "why we skip globalization," that note's still fine — the single-machine assumption is intentionally retained.

- **`Program.cs:32`** (the menu loop) — the stray `Console.WriteLine(DateTime.Now.ToShortDateString());` debug line. Delete it.

- **`Program.cs:26`** — `//AnimalRepository.LoadFromFile();` is **commented out**. **Uncomment it.** This is a critical fix — without it, the app starts with an empty list every time and ignores `animals.txt`. Easy to miss because the bug only shows up on second launch.

- **`Program.cs` `DisplayAllSortedBySpecies` calls `ConsoleUI.DrawHeaders` twice — that's intentional.** The first call paints the branded box at the top of the screen. The species-filter `PromptChoice` then scrolls the screen as it draws its numbered list and reads input, pushing the original header out of view. The second `DrawHeaders` redraws the box right before the table renders, giving the table a clean canvas. Add a brief comment above the second call explaining this if there isn't one already.

### Test after each cleanup

After each edit, hit `Ctrl+Shift+B` to make sure it still compiles.

---

## Part 8c — Write the final `README.md`

The README is what a stranger sees first. Make it useful in under a minute of reading.

### Required sections

Use this skeleton; expand each section to the depth that feels right.

```markdown
# Animal Rescue Management System

A Skills Ontario Coding (Animal Rescue Management) reference solution.
A standalone Windows console app for managing a small animal-rescue database:
add, remove, search, sort, and report on animals in a shelter.

## Features

- Add an animal with 12 fields, all validated through the prompt layer
- Remove an animal by ID (left-padded so `5` matches `00000005`)
- Search by name or species (case-insensitive substring; blank = show all)
- Display all animals sorted by species and name
- Display the three oldest animals in each species
- Persistence to a plain-text pipe-delimited file (`animals.txt`)
- Crash-resistant menu — every action is wrapped in try/catch; the app never crashes

## Building and running

### Requirements

- Windows
- .NET Framework 4.7.2 (built into Windows 10/11)
- Visual Studio 2019+ with the .NET desktop development workload

### Build

Open `SkillsOntarioSampleProject2026.sln` in Visual Studio. Build → Build Solution
(or Ctrl+Shift+B). The output `.exe` lives in `bin\Release\`.

From the command line (Developer Command Prompt for VS):

```
msbuild SkillsOntarioSampleProject2026.sln /p:Configuration=Release
```

### Run

Double-click `bin\Release\SkillsOntarioSampleProject2026.exe`. Data is saved
to `animals.txt` in the same folder as the `.exe`.

## Screenshots

[Insert: Add screen, Display All, Three Oldest Per Species, Help screen]

## File layout

| File | Purpose |
|------|---------|
| `Program.cs` | Main loop and menu actions |
| `Animal.cs` | The Animal data class + ToLine/FromLine for persistence |
| `AnimalRepository.cs` | Static class holding the in-memory list and file I/O |
| `ConsoleUI.cs` | Reusable colored prompts and table renderer |

## Known limitations

- **Single-machine save file.** `animals.txt` is written using the local
  machine's culture (short-date pattern, decimal separator). Copying the
  file to a machine with a different locale may fail to load.
- **No backup or undo.** A `Remove` is immediate and final; the save file
  is overwritten on every change.
- **In-memory list scales linearly.** Practical up to a few thousand animals.

## Workbook

Step-by-step build-up in `workbook/` parts 0 through 8.
```

### Take screenshots

Run the Release build with the seeded data, take four screenshots:

1. **Main menu** — the branded box + numbered list
2. **Add Animal** — partway through, showing a `PromptChoice` numbered list (e.g. Species)
3. **Display all (sorted by species)** — the table with multiple species
4. **Three Oldest Per Species** — the LINQ-driven output

Save them in `screenshots/` next to the README. Reference them in the Screenshots section.

---

## Part 8d — Final Release build + standalone test

Same fresh-folder sanity check from Part 4, but final.

1. **Clean** the solution: Build → Clean Solution.
2. **Build Release**: Build → Configuration Manager → Release → Build Solution.
3. **Copy** `bin\Release\SkillsOntarioSampleProject2026.exe` and any seeded `animals.txt` to a brand-new folder (e.g. `C:\Temp\AnimalRescue-Final\`).
4. **Run** the `.exe` from that folder.

Walk through every menu option:

- Add an animal → see a green success message → close + reopen → animal persists.
- Display all → table with all rows.
- Search → both empty and non-empty queries.
- Remove → with the seeded data, remove an animal you don't mind losing.
- Three oldest per species → 8 blocks of up-to-3 rows.
- Help → branded screen, returns to menu cleanly.
- Exit → app closes cleanly.

If anything fails, fix it and rebuild. Don't ship a known-broken `.exe`.

---

## Part 8e — Package the deliverable

The competition deliverable is `FirstName_LastName.zip` containing source + `bin\Release\` + `README.md`. From the project root:

1. Make sure `animals.txt` in `bin\Release\` is either the seed data you want shipped, or **delete it** so the app starts empty for the judges. Most projects ship empty.
2. Make sure `bin\Debug\` is **not** included — only `bin\Release\` ships.
3. Right-click the project root folder in Windows Explorer → **Send to → Compressed (zipped) folder**.
4. Rename the resulting `.zip` to `FirstName_LastName.zip` (using the student's actual name).

What's in the zip:

```
FirstName_LastName.zip
├── README.md
├── workbook/
│   ├── 00-design.md
│   ├── 01-project-setup.md
│   ├── ...
│   └── 08-polish-and-deliver.md
├── *.cs                                           (source files)
├── *.csproj, *.sln                                (Visual Studio project files)
├── screenshots/                                   (PNGs referenced in README)
├── bin/Release/SkillsOntarioSampleProject2026.exe (the runnable deliverable)
└── bin/Release/SkillsOntarioSampleProject2026.exe.config
```

What's **not** in the zip:

- `bin/Debug/` — debug builds bloat the zip and confuse judges
- `obj/` — intermediate build files
- `.vs/` — Visual Studio's local cache
- `.git/` if you used source control

If the judging machine extracts the zip and double-clicks the `.exe` from inside `bin\Release\`, the app should run. That's the test the deliverable has to pass.

---

## You're done

That's the project. Eight workbook parts, five `.cs` files, one save file, one standalone `.exe`, one zip. The student now has:

- A reference solution for Skills Ontario Animal Rescue Management.
- A workbook that walks them through it part by part.
- A `ConsoleUI.cs` they can paste into any future console project.
- The four-step "headers + rows + loop + PrintTable" pattern memorized for any tabular display.
- LINQ comfort with `Where`, `OrderBy`, `ThenBy`, `GroupBy`, `Select`, `Take`, `SelectMany`, `FirstOrDefault`.
- File I/O comfort with `StreamReader`/`StreamWriter`.
- A working mental model of "in-memory list + file-on-disk + serialize/deserialize through ToLine/FromLine."

Good luck at the comp.
