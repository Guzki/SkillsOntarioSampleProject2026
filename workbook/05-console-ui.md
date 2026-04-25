# Part 5 — `ConsoleUI`: pull the prompt-and-validate patterns into one file

> **Prerequisites:** Part 4 is complete. You have a working Add screen that uses raw `Console.ReadLine` with inline validation loops, and you've run the Release `.exe` standalone at least once.

## What this part builds

In Part 4 you wrote the four prompt-and-validate patterns by hand. You probably noticed they all have the same shape: print a label, read a line, check it, loop if bad, accept if good. That repetition is the signal. In this part you extract every one of those shapes into a single reusable file — `ConsoleUI.cs` — plus a few output helpers for colored text and a `PrintTable` helper you'll lean on in every display screen from now on.

By the end of this part:

- You will have one file (`ConsoleUI.cs`) that every future screen calls into.
- You will have **retrofitted `AddAnimal`** so it uses the new helpers instead of raw `Console.ReadLine` + validation loops.
- You will have **implemented `DisplayAllSortedBySpecies`** using the `PrintTable` helper end-to-end, which is your first real taste of the "build a table from domain data" pattern.

> **Why is this one file and not a few smaller ones?** Because at competition time you paste `ConsoleUI.cs` in as a single unit. Splitting it into `OutputHelpers.cs` and `InputHelpers.cs` would save nothing and cost a second paste. One file, flat.

> **Why are we writing it now and not at the very start?** Because writing the raw validation loops in Part 4 first makes the helpers feel earned. If `ConsoleUI` appeared in Part 1, you'd just be copying method calls without understanding what they hide. The pain comes first.

---

## Part 5a — Create the file and set up the skeleton

Right-click the project in Solution Explorer → **Add → Class → `ConsoleUI.cs`**. Replace the generated skeleton with:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkillsOntarioSampleProject2026
{
    public static class ConsoleUI
    {
        private const string AppName = "Animal Rescue Management System";
        private const int BoxWidth = 60;

        // helpers go here
    }
}
```

Two important choices in that skeleton:

- **`static class`.** There is only ever one ConsoleUI. It owns no state beyond the two constants. Marking the class `static` means you can never accidentally `new` one up.
- **Two constants at the top.** `AppName` is what shows in the header box; `BoxWidth` controls how wide the box is. Keeping them at the top of the file means if you ever rename the app or change the box width, one edit does it.

### The color palette (applied inside helpers, never at the call site)

| Color | Used for |
|---|---|
| Cyan | headers, labels, menu options |
| Yellow | prompts, screen titles, warnings |
| Green | success messages |
| Red | errors, invalid input retry messages |
| DarkGray | secondary text ("press any key…") |
| Default | regular content |

You'll apply these inside the helpers. Screens will never write `Console.ForegroundColor = ...` themselves — they just call `ConsoleUI.WriteLineColor("...", ConsoleColor.Green)` and the palette is done.

---

## Part 5b — Output helpers

### `SetupConsole`

Called once at the top of `Main`. Sets the window title and switches console output to UTF-8 so the `╔═╗║╚╝` box-drawing characters render correctly.

```csharp
public static void SetupConsole()
{
    Console.Title = "Animal Rescue Management System";
    Console.OutputEncoding = Encoding.UTF8;
}
```

**Test.** Add `ConsoleUI.SetupConsole();` as the **first** line of `Main`. Run the app. The window title bar should show "Animal Rescue Management System". If you skip UTF-8 setup, the box-drawing characters coming up next will display as `?` or squares.

---

### `WriteColor` and `WriteLineColor` — the save-change-restore pattern

These are the workhorses of every other helper. Every colored write follows the same four-step dance: **save** the current color, **change** to the new one, **write**, **restore** the old one. Forget the last step and every subsequent write leaks that color until something else resets it.

```csharp
public static void WriteColor(string message, ConsoleColor color)
{
    var previous = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.Write(message);
    Console.ForegroundColor = previous;
}

public static void WriteLineColor(string message, ConsoleColor color)
{
    var previous = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.WriteLine(message);
    Console.ForegroundColor = previous;
}
```

**Test.** Paste this into `Main` temporarily, right after `ConsoleUI.SetupConsole();`:

```csharp
ConsoleUI.WriteLineColor("red line", ConsoleColor.Red);
ConsoleUI.WriteLineColor("green line", ConsoleColor.Green);
ConsoleUI.WriteColor("yellow inline ", ConsoleColor.Yellow);
Console.WriteLine("back to default");
Console.ReadLine();
return;
```

Run it. You should see:

- ✅ A red line saying "red line"
- ✅ A green line saying "green line"
- ✅ "yellow inline " in yellow, immediately followed by "back to default" in the default gray/white — **not** still yellow

If the final text stays yellow, your restore step is missing. The bug is almost always "I assigned the color but forgot to write it back." Fix it, then delete the test block.

> **Why not just use `ForegroundColor` directly at the call site?** You could. But every screen that wanted a red error would have to remember the four-step dance, and the moment one of them forgot to restore the color, every subsequent write would come out red until something else fixed it. Hiding the dance inside the helper is exactly what the helper is for.

---

### `DrawHeaders` — the branded header at the top of every screen

This one is bigger. It draws the double-line box with the app name centered inside, then prints a screen-specific subtitle beneath and a thin separator.

```csharp
public static void DrawHeaders(string message)
{
    Console.Clear();

    string horizontalBorder = new string('═', BoxWidth - 2);
    string separator = new string('─', BoxWidth);

    WriteLineColor($"╔{horizontalBorder}╗", ConsoleColor.Cyan);
    WriteLineColor($"║{AppName.PadLeft((BoxWidth - 2 + AppName.Length) / 2).PadRight(BoxWidth - 2)}║", ConsoleColor.Cyan);
    WriteLineColor($"╚{horizontalBorder}╝", ConsoleColor.Cyan);
    WriteLineColor(message, ConsoleColor.DarkCyan);
    WriteLineColor(separator, ConsoleColor.DarkCyan);
}
```

Two pieces worth slowing down on:

- **`new string('═', BoxWidth - 2)`** — the "repeat a character N times" string constructor. If `BoxWidth` is 60, this builds a string of 58 `═` characters. You'll use the same trick inside `PrintTable` later.
- **The `PadLeft` / `PadRight` combination** — this is how you center text inside a fixed-width box. `PadLeft(n)` pushes the string to the right by padding the left with spaces until the total length reaches `n`. Then `PadRight(BoxWidth - 2)` pads the right side until the string fills the whole interior. Result: the app name sits in the middle with balanced padding on both sides and the right-hand `║` always lands in the same column.

**Test.** Replace the previous test block with:

```csharp
ConsoleUI.DrawHeaders("This is a test subtitle");
Console.ReadLine();
return;
```

Run it. You should see:

- ✅ A cyan box with `╔`, `═`, `╗`, `║`, `╚`, `╝` rendered as real Unicode lines (not `?`)
- ✅ "Animal Rescue Management System" centered inside the box
- ✅ "This is a test subtitle" in dark cyan on the line below
- ✅ A thin dark-cyan line under the subtitle

If the box characters come out as `?`, you forgot `Console.OutputEncoding = Encoding.UTF8;` in `SetupConsole`.

---

### `Pause`

Every screen ends with this. It prints a "press any key" prompt in dark gray, waits for a keystroke without echoing it, then clears the screen so the main menu comes up fresh.

```csharp
public static void Pause()
{
    Console.WriteLine();
    WriteLineColor("Press any key to continue...", ConsoleColor.DarkGray);
    Console.ReadKey(true);
    Console.Clear();
}
```

> **What does `true` do in `Console.ReadKey(true)`?** It tells the console "don't echo the pressed key to the screen." Without it, if the user pressed `h`, an `h` would suddenly appear on the "press any key" line. The `true` keeps the line clean.

**Test.** Add `ConsoleUI.Pause();` after the `DrawHeaders` test above. Run. Press any key. The screen should clear. That's it.

---

## Part 5c — Input helpers

Every input helper has the same shape: **print a prompt, read a line, try to parse/validate, loop on failure, return on success**. Once you see the shape once, the rest is the same pattern with different parsers.

### `Prompt` — no validation

```csharp
public static string Prompt(string message)
{
    Console.Write(message);
    return Console.ReadLine();
}
```

The simplest helper. Used for free-text fields like `Name` and `Colour` where any input is accepted. No validation, no looping. The caller decides what to do if the user enters an empty string.

**Test.**

```csharp
string name = ConsoleUI.Prompt("Your name: ");
Console.WriteLine($"Hello, {name}!");
Console.ReadLine();
return;
```

Type a name, see it echoed back.

---

### `PromptInt` — the parse-and-loop shape

```csharp
public static int PromptInt(string message)
{
    Console.Write(message);
    while (true)
    {
        var input = Console.ReadLine();
        if (int.TryParse(input, out int number))
        {
            return number;
        }
        WriteLineColor("Invalid integer format. Please try again. For example 42", ConsoleColor.Red);
    }
}
```

This is the **canonical parse-and-loop shape**. Memorize it. Every other number helper is the same three steps:

1. Print the prompt once, before the loop.
2. Inside the loop: read a line, try to parse, `return` on success.
3. On failure, print a red error and fall through to the top of the loop (re-reads input, but does *not* re-print the original prompt label — the user can see what they typed last).

**Test.**

```csharp
int age = ConsoleUI.PromptInt("Age: ");
Console.WriteLine($"Age is {age}");
Console.ReadLine();
return;
```

Try typing `hello`, then `12.5`, then `42`. The first two should each produce a red error; the third should succeed and print "Age is 42".

---

### `PromptDouble`

Same shape, `double.TryParse`.

```csharp
public static double PromptDouble(string message)
{
    Console.Write(message);
    while (true)
    {
        var input = Console.ReadLine();
        if (double.TryParse(input, out double number))
        {
            return number;
        }
        WriteLineColor("Invalid number format. Please try again. For example 12.34 or 100", ConsoleColor.Red);
    }
}
```

> **Why have `PromptDouble` AND `PromptDecimal`?** Because `double` is for measurements (weight, temperature) where a little precision loss is fine, and `decimal` is for money where every cent matters. In this project, `AdoptionFee` is a `decimal`. Never store money as `double` — you'll eventually hit rounding bugs that take hours to track down.

---

### `PromptDecimal` — two overloads

Same shape as `PromptInt`, plus an overload that enforces a maximum. The max overload is the one you'll use for `AdoptionFee`, which must be under 300.

```csharp
public static decimal PromptDecimal(string message)
{
    Console.Write(message);
    while (true)
    {
        var input = Console.ReadLine();
        if (decimal.TryParse(input, out decimal number))
        {
            return number;
        }
        WriteLineColor("Invalid decimal format. Please try again. For example 12.34 or 100", ConsoleColor.Red);
    }
}

public static decimal PromptDecimal(string message, decimal max)
{
    Console.Write(message);
    while (true)
    {
        var input = Console.ReadLine();
        if (decimal.TryParse(input, out decimal number) && number <= max)
        {
            return number;
        }
        WriteLineColor($"Invalid decimal format or value exceeds maximum of {max}. Please try again. For example 12.34 or 100", ConsoleColor.Red);
    }
}
```

> **C# overloading refresher.** Two methods can share a name if their parameter lists are different. `PromptDecimal("Fee: ")` and `PromptDecimal("Fee: ", 300m)` are two distinct methods. The compiler picks based on how many arguments you pass. The student doesn't need to understand "how overloading works internally" — just understand that the same name can have multiple shapes.

**Test.**

```csharp
decimal fee = ConsoleUI.PromptDecimal("Fee (< 300): ", 300m);
Console.WriteLine($"Fee is {fee}");
Console.ReadLine();
return;
```

Try `abc`, `500`, `299.99`. First two should error; third should succeed.

---

### `PromptDateTime`

Same shape, `DateTime.TryParse`.

```csharp
public static DateTime PromptDateTime(string message)
{
    Console.Write(message);
    while (true)
    {
        var input = Console.ReadLine();
        if (DateTime.TryParse(input, out DateTime date))
        {
            return date;
        }
        WriteLineColor("Invalid date format. Please try again. Enter as dd/mm/yyyy.", ConsoleColor.Red);
    }
}
```

> **Culture sidebar.** `DateTime.TryParse` uses the machine's current culture to decide whether `03/04/2026` means March 4 or April 3. For this app we assume a single consistent English-locale machine, which is what the competition environment will be. A more robust version uses `DateTime.TryParseExact("dd/MM/yyyy", CultureInfo.InvariantCulture, ...)` to reject ambiguous input — we're deliberately not doing that because it pulls in the whole globalization topic and this app doesn't need it. The `README.md` will note this as a known limitation.

**Test.**

```csharp
DateTime dob = ConsoleUI.PromptDateTime("Birthday (dd/mm/yyyy): ");
Console.WriteLine($"Birthday: {dob:dd/MM/yyyy}");
Console.ReadLine();
return;
```

Try `not a date`, `02/13/2026` (invalid month on en-CA), `03/04/2026`. First should error, behavior of the second depends on culture, third should succeed.

---

### `PromptChoice` — numbered menu, returns the selected option text

```csharp
public static string PromptChoice(string message, string[] choices)
{
    Console.WriteLine(message);
    for (int i = 0; i < choices.Length; i++)
    {
        WriteLineColor($"{i + 1}. {choices[i]}", ConsoleColor.Cyan);
    }
    while (true)
    {
        var input = Console.ReadLine();
        if (int.TryParse(input, out int choice) && choice >= 1 && choice <= choices.Length)
        {
            return choices[choice - 1];
        }
        WriteLineColor("Invalid choice. Please try again.", ConsoleColor.Red);
    }
}
```

**Returns the option text**, not the number. If choices are `["Dog", "Cat", "Bird"]` and the user picks option 2, this returns `"Cat"`, not `2`.

> **Why the option text and not the index?** Because the option text is almost always exactly what you want to store. `animal.Gender = ConsoleUI.PromptChoice("Select gender:", new string[] { "Male", "Female" });` — the return value goes straight into the property with no extra mapping step. Returning the index would force every caller to do `options[pick - 1]` and remember the 1-based-to-0-based conversion, which is the kind of off-by-one bug that bites a tired student at competition.
>
> The one place this falls down is when you want to `switch` on the choice and the option strings are long ("Small & Furry"). In that case, write `string pick = ConsoleUI.PromptChoice(...);` and compare strings: `switch (pick) { case "Small & Furry": ...; ... }`. Slightly more typing than `case 1:`, but you can read what each branch handles without scrolling back to the prompt.

**Test.**

```csharp
string[] options = new string[] { "Red", "Green", "Blue" };
string pick = ConsoleUI.PromptChoice("Pick a color:", options);
Console.WriteLine($"You picked: {pick}");
Console.ReadLine();
return;
```

The variable `pick` is now `"Red"`, `"Green"`, or `"Blue"` — drop it straight into wherever the value is needed.

---

## Part 5d — `PrintTable`: the headline helper

This is the biggest helper in the file, and the one you will lean on in every display screen. It takes a header row and a list of body rows (both as `string[]`) and prints them as a nicely aligned table. You never have to think about column widths again.

```csharp
public static void PrintTable(string[] headers, List<string[]> rows)
{
    if (!rows.Any())
    {
        WriteLineColor("No data available to display.", ConsoleColor.Gray);
        return;
    }

    // --- Pass 1: measure each column's width ---
    int[] widths = new int[headers.Length];
    for (int i = 0; i < headers.Length; i++)
    {
        widths[i] = headers[i].Length;
    }
    foreach (var row in rows)
    {
        for (int i = 0; i < row.Length; i++)
        {
            if (row[i].Length > widths[i])
            {
                widths[i] = row[i].Length;
            }
        }
    }

    // --- Pass 2: print ---
    // Header row, cyan.
    for (int i = 0; i < headers.Length; i++)
    {
        WriteColor(headers[i].PadRight(widths[i]) + " ", ConsoleColor.Cyan);
    }
    Console.WriteLine();

    // Separator line, dark cyan.
    for (int i = 0; i < headers.Length; i++)
    {
        WriteColor(new string('─', widths[i] + 1), ConsoleColor.DarkCyan);
    }
    Console.WriteLine();

    // Body rows, gray.
    foreach (var row in rows)
    {
        for (int i = 0; i < row.Length; i++)
        {
            WriteColor(row[i].PadRight(widths[i]) + " ", ConsoleColor.Gray);
        }
        Console.WriteLine();
    }

    // Row count summary.
    WriteLineColor($"\n{rows.Count} row(s) displayed.", ConsoleColor.DarkCyan);
}
```

### The core idea: two passes

You **cannot print until you know all the column widths**, because a row later in the list might be wider than the header. So you loop twice:

1. **Measure pass.** Walk the headers (start each column's width at the header's length). Then walk every row and widen each column if you find a bigger cell.
2. **Print pass.** Now that widths are known, walk everything again — headers, separator, body — and `PadRight` every cell to its column's width so the columns line up.

### The one new trick: a parallel `int[]`

```csharp
int[] widths = new int[headers.Length];
```

`widths` has exactly as many slots as there are columns. `widths[0]` is the width of the first column, `widths[1]` the second, and so on. This is the "parallel arrays" pattern: index `i` in `widths` lines up with index `i` in `headers` and with index `i` in every body row. Same concept, three arrays.

### Methods doing the real work

- **`"Rex".PadRight(22)`** → `"Rex                   "`. Returns a new string padded on the right with spaces until it's at least 22 chars long. If the string is already longer, nothing changes.
- **`new string('─', 12)`** → `"────────────"`. The "repeat a char N times" constructor you saw in `DrawHeaders`.

### Test

```csharp
var headers = new string[] { "ID", "Name", "Species" };
var rows = new List<string[]>
{
    new string[] { "00000001", "Rex", "Dog" },
    new string[] { "00000002", "Mrs. Whiskerbottom III", "Cat" },
    new string[] { "00000003", "Kiwi", "Bird" },
};
ConsoleUI.PrintTable(headers, rows);
Console.ReadLine();
return;
```

Paste that into `Main`, run. You should see:

- ✅ Three columns, cyan headers, dark-cyan separator line underneath
- ✅ The "Name" column is wide enough to fit "Mrs. Whiskerbottom III" without truncation; "Rex" and "Kiwi" are padded with spaces so the "Species" column still lines up
- ✅ "3 row(s) displayed." in dark cyan below the table

Also test the empty case:

```csharp
ConsoleUI.PrintTable(new string[] { "ID", "Name" }, new List<string[]>());
```

- ✅ Just prints "No data available to display." in gray. No headers, no separator, no summary.

Delete the test block before moving on.

### What `PrintTable` deliberately does NOT do

- **No truncation of long cells.** If someone names a dog "ReallyLongDogNameGoesHere", the column just gets wider. Don't solve problems you don't have.
- **No right-alignment for numbers.** Everything is `PadRight` (left-aligned). The `AdoptionFee` column will left-align and nobody will care.
- **No borders.** A clean header + separator is enough.
- **No wrapping long rows to multiple lines.** If the total width exceeds the console width, you get ugly wrap. Live with it — this app's data is short.

---

## Part 5e — Retrofit the main menu loop (with the crash-proof safety net)

Before retrofitting the screens themselves, retrofit the menu loop that calls them. Two changes happen here:

1. The menu loop becomes the place where `ConsoleUI.DrawHeaders` redraws the branded box every iteration, and where invalid choices report in red and pause.
2. Every call to a menu action gets wrapped in `try`/`catch`. The competition rule is **"the app must never crash"** — judges will type garbage, mash keys, enter dates that don't exist, and put `abc` where a number belongs. One unhandled exception that kills the process is the difference between a passing app and a failing one. The try/catch is your safety net.

### Why the try/catch goes at the *menu* level, not inside each screen

You could wrap every method body in try/catch. You shouldn't. Two reasons:

- **One catch covers all screens.** Every menu action gets the same protection without each one re-implementing it. New screens you add later are protected automatically just by being called from the menu.
- **A screen can't fix its own crash.** If `AddAnimal` throws halfway through, that screen is already broken — there's nothing useful for `AddAnimal` itself to do. The right place to recover is *outside* the screen, where you can show an error and return cleanly to the menu.

> **Rule of thumb for try/catch.** Catch exceptions at the boundary where you know how to recover. For a console menu, that boundary is the menu loop. Anywhere deeper and you don't have enough context to do anything useful; anywhere shallower (`Main` itself) and the app has already exited.

### The shape

```csharp
static void Main(string[] args)
{
    ConsoleUI.SetupConsole();
    AnimalRepository.LoadFromFile();

    while (true)
    {
        ConsoleUI.DrawHeaders("Main Menu");

        Console.WriteLine("Welcome to the Animal Shelter Management System");
        Console.WriteLine("1. Add animal");
        Console.WriteLine("2. Remove animal");
        Console.WriteLine("3. Search");
        Console.WriteLine("4. Display all (sorted by species)");
        Console.WriteLine("5. Display three oldest per species");
        Console.WriteLine("6. Help");
        Console.WriteLine("7. Exit");

        var choice = Console.ReadLine();

        try
        {
            switch (choice)
            {
                case "1": AddAnimal(); break;
                case "2": RemoveAnimal(); break;
                case "3": SearchAnimal(); break;
                case "4": DisplayAllSortedBySpecies(); break;
                case "5": DisplayThreeOldestPerSpecies(); break;
                case "6": ShowHelp(); break;
                case "7": return;
                default:
                    ConsoleUI.WriteLineColor("Invalid choice. Please try again.", ConsoleColor.Red);
                    ConsoleUI.Pause();
                    break;
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteLineColor($"Something went wrong: {ex.Message}", ConsoleColor.Red);
            ConsoleUI.Pause();
        }
    }
}
```

Six things to notice:

1. **`DrawHeaders` is *inside* the loop**, not outside. Every iteration redraws the branded header. If it were outside, after the first menu action's `Pause` cleared the screen the header would never come back.
2. **`Console.Clear()` is gone from the bottom of the loop.** It was redundant — the next iteration's `DrawHeaders` clears the screen first, and screens themselves end with `ConsoleUI.Pause()` which also clears. Two clears in a row is harmless; *no* clears would be the bug.
3. **The `try` wraps the `switch`, not the whole loop.** The loop must keep running even after a screen throws. Wrap the `switch`, catch, recover, fall through to the next iteration.
4. **`catch (Exception ex)` is intentionally broad.** Normally you'd catch specific exception types and let unknown ones propagate. Here you literally cannot let *anything* through — judges will find the one corner case you didn't think of, and the app must survive it. This is one of the few places where catching `Exception` is the right call.
5. **The error message uses `ex.Message`, not `ex.ToString()`.** `ex.Message` is one human-readable line ("Input string was not in a correct format."). `ex.ToString()` includes the entire stack trace — useful for developers, terrifying for judges.
6. **The default case has its own `Pause()`.** Without it, the red "Invalid choice" message would flash and immediately get erased by the next iteration's `DrawHeaders`. The `Pause()` lets the user actually read what went wrong.

### Test (force a crash to verify the safety net)

The whole point of this retrofit is the app surviving a crash. To verify the safety net works, you have to *cause* a crash on purpose. Temporarily add this line at the top of `AddAnimal`, just inside the method:

```csharp
throw new Exception("simulated crash for testing");
```

Run the app, choose "1. Add animal". You should see:

- ✅ A red message: `Something went wrong: simulated crash for testing`
- ✅ "Press any key to continue..." in dark gray
- ✅ After pressing a key, the menu redraws and the app keeps running
- ✅ You can pick another option and it works

If the app exits or shows a stack trace, the catch isn't wrapping correctly. **Remove the `throw` line before moving on** — it's a test, not a feature. Forgetting it is a live grenade.

> **Why not just trust your validation?** Because validation only catches the inputs you anticipated. The exceptions you'll actually hit at the comp are things like a corrupted `animals.txt` (someone hand-edited it and broke the format), an unparseable date in a saved file, or `IndexOutOfRangeException` from a row in `PrintTable` that has fewer cells than the headers. You don't need to anticipate them — you just need the safety net to swallow them and keep the menu alive.

---

## Part 5f — Retrofit `AddAnimal` to use `ConsoleUI`

The Add screen you wrote in Part 4 works, but it's full of raw `Console.WriteLine` / `Console.ReadLine` and hand-rolled validation loops. Now that `ConsoleUI` exists, you can replace all of that with helper calls. The screen becomes dramatically shorter and every validation loop is centralized.

### Before (Part 4 version)

You currently have something like this inside `AddAnimal`:

```csharp
Console.Write("Enter Name: ");
newAnimal.Name = Console.ReadLine();

Console.Write("Enter Species (Dog, Cat, Bird, Rabbit, Small & Furry, Fish, Barnyard, Other): ");
while (true)
{
    string species = Console.ReadLine();
    if (species == "Dog" || species == "Cat" || species == "Bird" ||
        species == "Rabbit" || species == "Small & Furry" || species == "Fish" ||
        species == "Barnyard" || species == "Other")
    {
        newAnimal.Species = species;
        break;
    }
    Console.Write("Invalid species. Try again: ");
}

Console.Write("Enter Birthday (dd/MM/yyyy): ");
DateTime birthday;
while (!DateTime.TryParse(Console.ReadLine(), out birthday))
{
    Console.Write("Invalid date format. Please enter the birthday (dd/MM/yyyy): ");
}
newAnimal.Birthday = birthday;

Console.Write("Enter Adoption Fee (less than 300): ");
decimal adoptionFee;
while (!decimal.TryParse(Console.ReadLine(), out adoptionFee) || adoptionFee >= 300)
{
    Console.Write("Invalid fee. Must be a number less than 300: ");
}
newAnimal.AdoptionFee = adoptionFee;
```

### After (using `ConsoleUI`)

Replace the whole body of `AddAnimal` with:

```csharp
// Prompts for the core Animal fields using ConsoleUI helpers, then adds the
// new animal to the repository and saves to disk. "Mutate then save" is two
// explicit calls — file I/O is never hidden.
private static void AddAnimal()
{
    ConsoleUI.DrawHeaders("Add a new animal");

    Animal newAnimal = new Animal();

    newAnimal.Name = ConsoleUI.Prompt("Name: ");

    string[] speciesOptions = new string[]
    {
        "Dog", "Cat", "Bird", "Rabbit",
        "Small & Furry", "Fish", "Barnyard", "Other"
    };
    newAnimal.Species = ConsoleUI.PromptChoice("Species:", speciesOptions);

    newAnimal.Birthday = ConsoleUI.PromptDateTime("Birthday (dd/MM/yyyy): ");

    newAnimal.AdoptionFee = ConsoleUI.PromptDecimal("Adoption fee (< 300): ", 300m);

    AnimalRepository.AddAnimal(newAnimal);
    AnimalRepository.SaveToFile();

    ConsoleUI.WriteLineColor("Animal added successfully!", ConsoleColor.Green);
    ConsoleUI.Pause();
}
```

Three things to notice:

1. **The validation loops are gone.** They live in `ConsoleUI` now. Every screen benefits.
2. **`PromptChoice` returns the option text directly** — `newAnimal.Species = ConsoleUI.PromptChoice(...)` assigns straight into the property. No `[pick - 1]` mapping step. The string the user picked *is* the value you store.
3. **`AnimalRepository.AddAnimal(...)` and `SaveToFile()` are still two explicit calls.** Saving is not hidden inside `AddAnimal` — the rule from Part 3 stands: mutation and persistence are separate operations, side by side, on purpose.

### Test

Run the app, choose "1. Add animal". You should see:

- ✅ The cyan branded header at the top with "Add a new animal" as the subtitle
- ✅ Name prompt accepts anything
- ✅ Species shows a numbered cyan list of 8 options; typing `9` or `abc` shows a red error and reprompts
- ✅ Birthday rejects `hello` with a red error, accepts a valid date
- ✅ Fee rejects `abc` and `500`, accepts `250.50`
- ✅ "Animal added successfully!" in green, then "press any key" in dark gray
- ✅ After Pause, the menu returns

Try adding a few animals, then exit the app, then re-run — your animals should still be there (loaded from `animals.txt` on startup).

---

## Part 5g — Implement `DisplayAllSortedBySpecies` using `PrintTable`

This is your first real display screen. Earlier you had a stub that just did a `Console.WriteLine` loop. Replace it with the four-step `PrintTable` calling pattern.

### The four-step pattern (every display screen uses this)

1. **Headers** — declare the column headers as a local `string[]`.
2. **Rows** — create an empty `List<string[]> rows`.
3. **Build** — loop through the (filtered / sorted) data, and for each item `Add` a `string[]` to `rows`. The fields **must be in the same order as the headers** — parallel arrays again. Getting them out of order is the #1 bug in this pattern.
4. **Call** — pass `headers` and `rows` to `ConsoleUI.PrintTable`.

### The code

```csharp
// Lists every animal, ordered by species, in an aligned table.
private static void DisplayAllSortedBySpecies()
{
    ConsoleUI.DrawHeaders("All animals — sorted by species");

    // 1. Headers for this screen.
    string[] headers = new string[] { "ID", "Name", "Species", "Breed" };

    // 2. An empty list to hold the rows we build.
    List<string[]> rows = new List<string[]>();

    // 3. One row per animal, fields in the same order as the headers above.
    foreach (Animal a in AnimalRepository.animals.OrderBy(a => a.Species))
    {
        rows.Add(new string[] { a.Id, a.Name, a.Species, a.Breed });
    }

    // 4. Hand off to the generic helper.
    ConsoleUI.PrintTable(headers, rows);

    ConsoleUI.Pause();
}
```

### Why this lives in the screen and not on `Animal`

You might wonder: "wouldn't it be cleaner if `Animal` had a `ToRow()` method that returned its own `string[]`?" The answer is no, for this project, because **different screens show different columns**. Display All wants ID / Name / Species / Breed. The "three oldest per species" screen (coming in a later part) will want Birthday in there. Search results might want the field you searched on. There's no single canonical row for an animal — so putting one on `Animal` would either force every screen to look identical, or lead to `ToRowShort` / `ToRowFull` / `ToRowSearch` sprawl.

Keeping the headers and row-building local to the screen means:

- Each screen is self-contained — to understand what `DisplayAllSortedBySpecies` shows, you only read `DisplayAllSortedBySpecies`. No jumping to `Animal.cs`.
- Adding a new screen with different columns is easy — you don't have to decide whether to edit `Animal` or add a new method there.

The small amount of repetition across screens (same three or four header strings typed in a few places) is the **price**, and it's cheaper than the abstraction.

> **Compare this to `Animal.ToLine()`.** `ToLine` earns its own method because it has exactly **one customer** (the save file) and exactly **one frozen format**. There's nothing to vary. A display row is the opposite situation — many customers, different formats — so it stays in the screen.

### Test

Add at least three animals of different species using the Add screen, then run "4. Display all (sorted by species)". You should see:

- ✅ Branded header with "All animals — sorted by species" subtitle
- ✅ Cyan header row: `ID  Name  Species  Breed`
- ✅ Dark-cyan separator line
- ✅ Animals listed in alphabetical order by Species, in gray
- ✅ All four columns aligned regardless of how long the names are
- ✅ "N row(s) displayed." at the bottom in dark cyan
- ✅ Press any key returns to the menu

Try the empty case too: delete `animals.txt` next to the `.exe`, restart the app, choose "Display all". You should see "No data available to display." in gray instead of an empty headers-only table.

---

## Deployment test (repeat from Part 4)

Every time you finish a significant part, do the full deployment test:

1. Build the Release configuration: **Build → Configuration Manager → Release → Build**.
2. Open `bin\Release\` in File Explorer.
3. Copy `SkillsOntarioSampleProject2026.exe` to somewhere else (like your desktop).
4. Double-click it from there. It should run standalone.
5. Try Add Animal and Display All — they should work and the `.txt` file should appear next to the `.exe`.

If the `.exe` refuses to run or crashes on startup, you've introduced a dependency the build hasn't picked up. Fix it before moving on — the whole competition deliverable is a single `.exe` + `README.md` + a `.txt`.

---

## What's next (Part 6 — Remove and Search)

You've got Add and Display All working end-to-end. Part 6 adds the two "find an existing animal and do something with it" screens: **Remove** (look up by ID, confirm, delete, save) and **Search** (look up by name or species, show matches via `PrintTable`). Both share the same find-by-X pattern, so they get bundled into one chapter. You'll also add four new methods to `AnimalRepository` (`RemoveById`, `SearchByName`, `SearchBySpecies`) — short LINQ one-liners that keep query logic out of the screens.
