# Part 4 — The Add Screen (end-to-end vertical slice)

> **Prerequisites:** Part 3 is complete. You have an `AnimalRepository` with `AddAnimal`, `SaveToFile`, and `LoadFromFile`. You've confirmed file round-tripping works.

## What this part builds

The Add screen takes a user through a handful of prompts, creates an Animal, adds it to the repository, and saves the file. By the end of this part you will have a **thin vertical slice** — a working program that takes real input, stores it in memory, writes it to disk, and reads it back after a restart. Every layer of the app is exercised.

You'll also do the **full deployment test** at the end: build a Release `.exe`, move it to a fresh folder, run it standalone, and confirm it behaves correctly. That test simulates exactly what a judge will do when they receive your `.zip`.

### Scope of this Add screen

**Four fields only** for now:

1. `Name` (free text)
2. `Species` (must be one of 8 values)
3. `Birthday` (must parse as `dd/MM/yyyy`)
4. `AdoptionFee` (must parse as decimal, must be < 300)

The other seven fields get added in the next part once `ConsoleUI` exists. Four is enough to prove the vertical slice works and to feel the pain of writing validation loops by hand.

> **Why raw `Console.ReadLine` and not the `ConsoleUI` helpers?** Because `ConsoleUI` doesn't exist yet. You'll write the prompt-and-validate loop three different times by hand in this part. That repetition is the point — the moment when your fingers get tired of typing the same pattern is the moment you're ready to extract `ConsoleUI` in Part 5.

---

## Part 4a — The four patterns you'll use

Every prompt in this screen is one of four shapes. Learn the shapes here; you'll use them everywhere.

### Pattern 1 — Free text

```csharp
Console.Write("Name: ");
animal.Name = Console.ReadLine();
```

One line in, one line out. No validation. Used for things like Name and Colour where any input is accepted.

### Pattern 2 — Must be one of a fixed list

```csharp
string species;
while (true)
{
    Console.Write("Species (Dog, Cat, Bird, Rabbit, Small & Furry, Fish, Barnyard, Other): ");
    species = Console.ReadLine();
    if (species == "Dog" || species == "Cat" || species == "Bird" ||
        species == "Rabbit" || species == "Small & Furry" || species == "Fish" ||
        species == "Barnyard" || species == "Other")
    {
        break;
    }
    Console.WriteLine("Invalid species. Try again.");
}
animal.Species = species;
```

A `while (true)` loop that only `break`s when the input is valid. This is the most common validation shape.

### Pattern 3 — Must parse cleanly

```csharp
DateTime birthday;
while (true)
{
    Console.Write("Birthday (dd/MM/yyyy): ");
    string input = Console.ReadLine();
    if (DateTime.TryParse(input, out birthday))
    {
        break;
    }
    Console.WriteLine("Invalid date. Use dd/MM/yyyy.");
}
animal.Birthday = birthday;
```

`TryParse` returns `true` if the string parsed into a valid date, `false` otherwise. The loop only breaks when parsing succeeds. We assume the user is typing English-format input on an English-locale machine — see the culture sidebar at the end of this part for what we're deliberately *not* teaching.

### Pattern 4 — Parse + range check

```csharp
decimal fee;
while (true)
{
    Console.Write("Adoption fee (must be less than 300): ");
    string input = Console.ReadLine();
    if (decimal.TryParse(input, out fee) && fee < 300)
    {
        break;
    }
    Console.WriteLine("Invalid fee. Must be a number less than 300.");
}
animal.AdoptionFee = fee;
```

Two conditions in the `if`: parses correctly **and** passes the range check. Same loop shape.

---

## Part 4b — A closer look at the Species loop

This pattern is the trickiest, so let's walk through it line by line.

```csharp
string species;
while (true)
{
    Console.Write("Species (Dog, Cat, Bird, Rabbit, Small & Furry, Fish, Barnyard, Other): ");
    species = Console.ReadLine();
    if (species == "Dog" || species == "Cat" || /* ... */)
    {
        break;
    }
    Console.WriteLine("Invalid species. Try again.");
}
animal.Species = species;
```

### Line by line

- **`string species;`** — declare the variable *outside* the loop. If you declared it inside, you couldn't use it on the line after the loop (`animal.Species = species;`). Scope matters.
- **`while (true)`** — an infinite loop. The only way out is an explicit `break`. This shape is natural for "keep prompting until the input is good."
- **`Console.Write(...)`** — `Write`, not `WriteLine`. The cursor should stay on the same line as the prompt so the user types beside it, not below it.
- **`species = Console.ReadLine();`** — reads until the user presses Enter. Returns a string (or `null` if stdin is closed — ignore that edge case for now).
- **`if (species == "Dog" || ...)`** — the validation. Every allowed value, OR-joined.
- **`break;`** — when valid, leave the loop.
- **`Console.WriteLine("Invalid ...");`** — only reached when the `if` was false, meaning no branch broke. Show an error and let the loop iterate again.

### Why `while (true)` instead of a different shape

You *could* write this as:

```csharp
bool valid = false;
string species = null;
while (!valid)
{
    Console.Write("Species: ");
    species = Console.ReadLine();
    valid = (species == "Dog" || ...);
    if (!valid) Console.WriteLine("Invalid species. Try again.");
}
```

Functionally identical, but you've added a flag variable you don't need. `while (true) { ... if (ok) break; }` is cleaner for prompt loops — the condition is *inside* the body, where the work is.

### Why the long OR chain

In a bigger codebase you'd put the valid species in a `string[]` and use `Array.Exists` or LINQ `Contains`. For teaching, the OR chain is more obviously correct and uses no helpers. When you add `ConsoleUI.PromptChoice` in the next part, the whole loop collapses to one line and the array is hidden inside the helper.

---

## Part 4c — Wire it all together

Replace the stub `AddAnimal` in `Program.cs` with:

```csharp
// Prompts the user for each field, creates an Animal, adds it to the
// repository, and saves the list to disk.
private static void AddAnimal()
{
    Animal animal = new Animal();

    // Name — free text
    Console.Write("Name: ");
    animal.Name = Console.ReadLine();

    // Species — must be one of eight
    string species;
    while (true)
    {
        Console.Write("Species (Dog, Cat, Bird, Rabbit, Small & Furry, Fish, Barnyard, Other): ");
        species = Console.ReadLine();
        if (species == "Dog" || species == "Cat" || species == "Bird" ||
            species == "Rabbit" || species == "Small & Furry" || species == "Fish" ||
            species == "Barnyard" || species == "Other")
        {
            break;
        }
        Console.WriteLine("Invalid species. Try again.");
    }
    animal.Species = species;

    // Birthday — dd/MM/yyyy
    DateTime birthday;
    while (true)
    {
        Console.Write("Birthday (dd/MM/yyyy): ");
        string input = Console.ReadLine();
        if (DateTime.TryParse(input, out birthday))
        {
            break;
        }
        Console.WriteLine("Invalid date. Use dd/MM/yyyy.");
    }
    animal.Birthday = birthday;

    // Adoption fee — decimal under 300
    decimal fee;
    while (true)
    {
        Console.Write("Adoption fee (less than 300): ");
        string input = Console.ReadLine();
        if (decimal.TryParse(input, out fee) && fee < 300)
        {
            break;
        }
        Console.WriteLine("Invalid fee. Must be a number less than 300.");
    }
    animal.AdoptionFee = fee;

    // Persist
    AnimalRepository.AddAnimal(animal);
    AnimalRepository.SaveToFile();

    Console.WriteLine("Animal added.");
}
```

> **Don't panic about the missing fields.** `Breed`, `Colour`, `Gender`, `SpayedOrNeutered`, `VaccineStatus`, `IdentificationNumber`, `IdentificationType` all stay at their C# defaults (empty string or similar) for now. That's fine — the `ToLine` call will render them as empty fields in the file. When you add those seven prompts in the next part, fresh animals will be complete; older ones with missing fields will just have blanks.

---

## Part 4d — In-development tests

Before you deploy, make sure the basic behaviours work inside Visual Studio.

### Test 1 — Valid input flows through cleanly

1. Press F5 (Debug) to run.
2. Pick **1. Add animal**.
3. Enter `Rex`, `Dog`, `15/06/2019`, `250`.
4. Expect: `"Animal added."` and return to menu.
5. Pick **2. Display** (or whatever you called it). Expect: one pipe-delimited line starting with `00000001`.

### Test 2 — Each validation loop rejects bad input

1. Add another animal.
2. For species, type `Dragon`. Expect: `"Invalid species. Try again."` and a re-prompt.
3. Type `Dog`. Expect: moves to next field.
4. For birthday, type `garbage`. Expect: `"Invalid date. Use dd/MM/yyyy."` and a re-prompt.
5. Type `20/01/2020`. Expect: moves to next field.
6. For fee, type `abc`, then `400`, then `99`. Expect: two rejections and one acceptance.

### Test 3 — Persistence across restarts

1. Add at least two animals successfully.
2. **Close the console window.** Do not just stop the debugger — actually exit via the menu or close the window so the `using` block finishes its flush.
3. Open `bin\Debug\animals.txt` in Notepad. Confirm two pipe-delimited lines with IDs `00000001` and `00000002`.
4. Press F5 again to relaunch.
5. Pick Display. Confirm both animals are back.

If all three tests pass, the in-development vertical slice works. Now do the real test.

---

## Part 4e — The full deployment test (treat it like a hand-in)

This is the test that matters. It simulates what a judge does when they unzip your submission.

### Step 1 — Build Release

In Visual Studio, change the **configuration dropdown** (in the toolbar) from **Debug** to **Release**. Then **Build → Build Solution** (Ctrl+Shift+B).

A successful build produces `bin\Release\SkillsOntarioSampleProject2026.exe` (among other files).

### Step 2 — Prepare a clean target folder

On your Desktop (or anywhere outside the project), create a new folder called `AnimalRescueTest`.

This folder must start **empty**. No `animals.txt`, no anything. You're simulating a fresh machine.

### Step 3 — Copy the `.exe` only

Copy **just the `.exe`** from `bin\Release\` into `AnimalRescueTest\`. Don't copy the `.pdb`, the `.config`, or anything else unless your app actually needs it.

> **Reality check:** for .NET Framework 4.7.2 on a modern Windows machine, a console app compiled to a single `.exe` runs standalone as long as the target machine has .NET Framework 4.7.2 or later installed (which every recent Windows machine does). You don't need to copy DLLs. If your `.exe` won't run standalone, something's wrong with how the project is configured — go back and check the `.csproj`.

### Step 4 — Run it

**Double-click** `SkillsOntarioSampleProject2026.exe`. A console window opens and your menu appears.

### Step 5 — Vertical test

1. Add an animal (any valid values).
2. **Leave the console window open** and open the `AnimalRescueTest` folder in a second window.
3. Confirm `animals.txt` has appeared **in that folder, right next to the `.exe`**.
4. Open `animals.txt` in Notepad. Confirm it contains one pipe-delimited line.
5. Back in the console, pick Display. Confirm the animal shows.
6. Exit via the menu.
7. Double-click the `.exe` again.
8. Pick Display. Confirm the animal is still there.
9. Add a second animal. Confirm `animals.txt` now has two lines.

### Step 6 — Sanity check: no surprises

- The `animals.txt` file should be in `AnimalRescueTest\`, **not** in `bin\Release\` back in your project. (If it appeared in the project folder, something is running the wrong `.exe`.)
- The `.exe` should have **no external dependencies** — no missing-DLL popup when double-clicked.
- File format in `animals.txt` should match the pipe format you designed in Part 0.

### Step 7 — Clean up

Delete `AnimalRescueTest\` when you're done. This is a disposable test folder, not a deliverable.

---

## If any deployment step failed

- **`.exe` won't run / missing .NET error:** your target framework in `.csproj` might be wrong. Confirm `<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>`. Rebuild.
- **`animals.txt` appeared in `bin\Release\` instead of `AnimalRescueTest\`:** you're running the wrong `.exe`. Check which one you double-clicked.
- **Console window flashes and closes immediately:** unhandled exception at startup. Open a Command Prompt, `cd` into `AnimalRescueTest`, run `SkillsOntarioSampleProject2026.exe`, and read the error before the window closes. Common culprit: a malformed `animals.txt` if you copied one from somewhere by accident.
- **Input doesn't get accepted:** `Console.ReadLine` on a double-clicked `.exe` works the same as in VS — if not, your menu loop may be consuming the input wrong. Check that you're not mixing `ReadKey` and `ReadLine` carelessly.

---

## Checkpoint — this part is done when all of these are true

- [ ] The Add screen prompts for Name, Species, Birthday, Adoption Fee.
- [ ] Invalid species / date / fee all re-prompt with a visible error message.
- [ ] `AddAnimal` calls `AnimalRepository.AddAnimal` **and** `AnimalRepository.SaveToFile` as two explicit lines.
- [ ] In development, round-trip works (add, exit, re-launch, display).
- [ ] The **deployment test** passes: Release `.exe` runs standalone from an empty folder and creates `animals.txt` next to itself.

When every box is checked, **you've built a thin vertical slice end-to-end**. Every layer works, every piece of the app is exercised, and a fresh machine can run your `.exe`.

---

## Sidebar — culture, dates, and decimals (a topic we're skipping)

You may have noticed that `DateTime.Parse`, `DateTime.TryParse`, and `decimal.Parse` just... work, without you specifying anything about format. That's because they use the **current machine culture** — the language and region settings of whichever Windows machine is running the code.

This is fine for us because:

- Both development and the competition happen on English-locale machines (or similar enough).
- Our prompts tell the user what format to use (`dd/MM/yyyy`).
- If the user enters something the parser doesn't accept, the `TryParse` loop catches it and re-prompts.

But in production software, culture-sensitive parsing is a real bug source. If a French-Canadian machine saves `animals.txt` with decimals written `250,00` (comma as the decimal separator), and then an English-locale machine tries to read that file, parsing can silently succeed with a wrong value or fail outright. Same story for dates — `jj/mm/aaaa` vs `dd/MM/yyyy`, day-first vs month-first, etc.

The production fix is `CultureInfo.InvariantCulture` + explicit format specifiers:

```csharp
// strict: accepts ONLY dd/MM/yyyy, regardless of machine locale
DateTime.TryParseExact(input, "dd/MM/yyyy",
    CultureInfo.InvariantCulture, DateTimeStyles.None, out birthday);

// strict: decimal separator is always '.', regardless of locale
decimal.Parse(input, CultureInfo.InvariantCulture);
```

We're **deliberately not teaching this** on this project. Globalization is a substantial sub-topic (cultures, number styles, format specifiers, round-trip safety) that would derail the core lessons here. The `README.md` at hand-in time notes the app's single-locale assumption.

**If you keep programming in C# past this project, globalization is the natural next topic to learn around parsing and file I/O.** For now: assume English-format input, parse with plain `TryParse` / `Parse`, and move on.

---

**Next part:** the Add screen is ugly and your fingers hurt from typing `while (true)` three times. Time to build `ConsoleUI` — a set of static helpers that turn each prompt into one line. We build the helpers driven by what the next screens actually need.
