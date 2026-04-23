# Part 2 — The Animal Class

> **Prerequisites:** you've finished Part 1. You have an empty project named `SkillsOntarioSampleProject2026`, targeting .NET Framework 4.7.2, that compiles and runs.

## What this part builds

You have a program. It does nothing. Before it can manage a shelter, it needs to know what an **animal** *is* — what pieces of data describe one, and how to write one out as a line of text (and read one back in).

In this part you'll:

1. Create a new file, `Animal.cs`.
2. Declare the **12 properties** you designed in Part 0, in a fixed order that the rest of the app will depend on.
3. Write a first version of **`ToLine()`** — the method that turns one Animal into one line of text.
4. Write a first version of **`FromLine(string)`** — the method that turns one line of text back into an Animal.

> **Important:** the `ToLine` and `FromLine` you write in this part are **temporary**. They work, and they work quickly — which is exactly the point. You're going to use them to test the whole save-and-load pipeline in Part 3 before spending time on the version judges actually want to see. A later part rewrites both methods with a manual **character-by-character walk** using `StringBuilder` — no `string.Split`, no string interpolation. That's the teaching centerpiece of this project. For now you just need something that works so we can move on and come back.
>
> **Why build it twice?** Because the character walk is the hardest single piece of code in the project. Confronting it on top of file I/O, menus, and validation all at once is how people get stuck. Build the easy version, prove the whole app round-trips through a file, *then* replace the engine with the one the judges want.

---

## Part 2a — Create the file

In Visual Studio's **Solution Explorer**, right-click the project name → **Add** → **Class...**. Name the new file **`Animal.cs`** exactly (capital A, no spaces). Click **Add**.

Visual Studio creates `Animal.cs` next to `Program.cs` and opens it. It'll look something like:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillsOntarioSampleProject2026
{
    class Animal
    {
    }
}
```

Change `class Animal` to `public class Animal` — this lets other files (`Program.cs`, `AnimalRepository.cs` later) see and use the class.

```csharp
public class Animal
{
}
```

That's the skeleton. Everything else in this part goes inside those braces.

---

## Part 2b — The 12 properties

These are the fields you designed in your Question 3 table in Part 0. They go inside the class, in this exact order:

```csharp
public class Animal
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Breed { get; set; }
    public string Species { get; set; }
    public DateTime Birthday { get; set; }
    public string SpayedOrNeutered { get; set; }
    public string Gender { get; set; }
    public string Colour { get; set; }
    public string VaccineStatus { get; set; }
    public string IdentificationNumber { get; set; }
    public string IdentificationType { get; set; }
    public decimal AdoptionFee { get; set; }
}
```

### Why this exact order matters

The order you see above is the same order `ToLine` writes and the same order `FromLine` reads. **Once you save an `animals.txt` file, this order is locked in.** If you later swap two fields, every previously-saved file becomes unreadable — `FromLine` will happily assign a breed into the species slot and nothing will complain until the data looks wrong on screen.

**Write these 12 lines in this order. Don't reorder them.** If you need to adjust Part 0's design later, adjust the design document and the code together.

### What is `{ get; set; }`?

That pair of braces after each property — `{ get; set; }` — is an **auto-property**. It's shorthand for "create a private hidden field, plus a getter and a setter for it." You can read the value with `animal.Name` and assign a new value with `animal.Name = "Rex"`. The compiler wires it up for you.

The longer form would be:

```csharp
private string name;
public string Name
{
    get { return name; }
    set { name = value; }
}
```

Same behaviour, far more typing. For simple data-holder classes like `Animal`, auto-properties are the right tool. You'll use them everywhere.

### Why `Id` is a `string`, not an `int`

Your IDs are 8-digit zero-padded values like `"00000001"`. A real integer can't preserve the leading zeros — `int id = 00000001` is literally the number `1` and prints as `"1"`. A `string` preserves the exact characters. The repository converts to and from `int` only inside `NextId()` when it needs arithmetic.

### Why `Birthday` is a `DateTime`, but `Gender` is a `string`

- `Birthday` is a real date. You want to sort by it ("three oldest per species"), which is easy on `DateTime` and awkward on a string. Store it typed.
- `Gender` is `"F"` or `"M"` — two fixed values, no arithmetic, no sorting, no math. A `string` with a validation check at the Add screen is more than enough. You'd get no payoff from wrapping it in an enum or a custom type.

The pattern: **type things the way you'll use them.** Dates as dates, money as decimal, free-text labels as strings. Don't over-engineer.

### Why `AdoptionFee` is `decimal`, not `double` or `float`

Money math on `double`/`float` gives you rounding bugs. `$0.10 + $0.20` in `double` is not exactly `0.30`. `decimal` is a base-10 number type designed for money and doesn't have this problem. **Any time you see money in C#, reach for `decimal`.**

---

## Part 2c — Write ToLine (the easy version)

`ToLine()` takes one `Animal` and returns a single string — one line of your save file. Add this method inside the class, right below the 12 properties:

```csharp
// Turns this Animal into one pipe-delimited line for saving to the file.
public string ToLine()
{
    return $"{Id}|{Name}|{Breed}|{Species}|{Birthday.ToString("dd/MM/yyyy")}|{SpayedOrNeutered}|{Gender}|{Colour}|{VaccineStatus}|{IdentificationNumber}|{IdentificationType}|{AdoptionFee}";
}
```

### What this line does

- **`$"..."`** is a **string interpolation**. Anywhere you write `{Something}` inside the dollar-sign-quoted string, C# substitutes the value of `Something`. It's a nicer way to write `"Hello, " + name + "!"` — same result, less punctuation.
- The **`|` characters** between each `{Property}` are literal pipe characters. They're the delimiter you picked in Part 0. They sit there in the final string separating every field.
- **`Birthday.ToString("dd/MM/yyyy")`** formats the date as `"15/06/2019"`. Without the format string, `DateTime.ToString()` gives you whatever the current machine's default is — possibly with the time, possibly in a different order — and that's not what you want in a file. Force the exact format so every saved date looks the same.
- **Every other field is already a string** (or converts cleanly — `decimal` will render as `"250"` or `"250.00"` depending on the value).

### What you'll get for a sample animal

For an Animal with `Id = "00000001"`, `Name = "Rex"`, `Breed = "Labrador"`, `Species = "Dog"`, `Birthday = new DateTime(2019, 6, 15)`, and the rest filled in, `ToLine()` produces:

```
00000001|Rex|Labrador|Dog|15/06/2019|Yes|M|Brown|Up to date|985141000123456|Microchip|250.00
```

That's one line of `animals.txt`. Many animals → many lines.

> **Why we'll rewrite this later:** this version uses string interpolation, which is convenient but does a lot of work for you invisibly. In a later part you'll rewrite `ToLine` to build the string **character by character** using a `StringBuilder` — no interpolation, no `string.Join`. That rewrite teaches you what `$"..."` was doing behind your back, and it's what the judges want to see. For now, interpolation gets you a working save file in one line of code so you can keep moving.

---

## Part 2d — Write FromLine (the easy version)

`FromLine` is the mirror image of `ToLine`. Given one line of the file, it returns a new `Animal` with the 12 fields filled in. Add this method **below** `ToLine`, still inside the class:

```csharp
// Parses one pipe-delimited line back into an Animal.
public static Animal FromLine(string line)
{
    string[] parts = line.Split('|');
    return new Animal
    {
        Id = parts[0],
        Name = parts[1],
        Breed = parts[2],
        Species = parts[3],
        Birthday = DateTime.Parse(parts[4]),
        SpayedOrNeutered = parts[5],
        Gender = parts[6],
        Colour = parts[7],
        VaccineStatus = parts[8],
        IdentificationNumber = parts[9],
        IdentificationType = parts[10],
        AdoptionFee = decimal.Parse(parts[11])
    };
}
```

### Three things new in this method

**1. `public static`** instead of just `public`.

A `static` method is called on the class, not on an instance. You call it as `Animal.FromLine(someLine)`, **not** `someAnimalInstance.FromLine(someLine)`. That's right, because when you're parsing a line you don't yet have an Animal — you're *making* one. `ToLine` is instance-level because it serializes "this particular animal." `FromLine` is static because it builds a new one from scratch.

**2. `line.Split('|')`**.

This takes the string and splits it at every `|` character, returning a `string[]` array of 12 pieces. `parts[0]` is the Id, `parts[1]` is the Name, and so on, in the same order `ToLine` wrote them.

**3. The object initializer syntax.**

```csharp
return new Animal
{
    Id = parts[0],
    Name = parts[1],
    // ...
};
```

That's shorthand for "create a new Animal, then assign each of these properties." Equivalent to:

```csharp
Animal a = new Animal();
a.Id = parts[0];
a.Name = parts[1];
// ...
return a;
```

Same result, less noise.

### About `DateTime.Parse` and `decimal.Parse`

- `DateTime.Parse(parts[4])` turns the string `"15/06/2019"` back into a real `DateTime`.
- `decimal.Parse(parts[11])` turns `"250.00"` back into a `decimal` value.

Both of these use the **current machine's locale** to interpret the input. That's fine for this project — the machine writing the file and the machine reading it are the same machine. A more careful production app would use `CultureInfo.InvariantCulture` and `DateTime.TryParseExact` to lock down the exact format, but that pulls in a large topic (cultures, number styles, exact vs. standard parsing) that isn't worth teaching mid-project. `PEDAGOGY.md` goes into the reasoning.

The prompt in the Add screen tells the user exactly what format to type (`dd/MM/yyyy`), and the Add screen's validation loop will catch unparseable input before it ever reaches the file. So in practice these `Parse` calls only ever see strings that `ToLine` wrote — and `ToLine` writes them cleanly.

> **Why we'll rewrite this later:** `string.Split('|')` does the whole parse in one call. The judges' version parses the line **character by character** using a `StringBuilder` — reading one character at a time, appending to a buffer, and emitting a field every time the character is `|`. That's the teaching pair to the character-by-character version of `ToLine`. You'll see both together in a later part. For now, `Split` gets you a working load path in two lines so Part 3 can actually test file round-tripping.

---

## Part 2e — Build and sanity-check

You can't run and see anything yet — there's no menu, no prompts, no `Console.WriteLine`. But you can prove the code compiles.

Press **Ctrl+Shift+B**. Visual Studio builds the solution. Look at the status bar at the bottom — it should say **"Build succeeded"**.

If it says "Build failed," scroll through the **Error List** panel. Common problems:

- A typo in a property name (`Birtday` instead of `Birthday`). The compiler points at the line.
- Missing semicolon at the end of a `{ get; set; }` line — each property line needs `;` at the end after the closing `}`.
- `namespace` in `Animal.cs` doesn't match `namespace` in `Program.cs`. Both must read `namespace SkillsOntarioSampleProject2026`.
- You forgot the `using System;` at the top (`DateTime` is in the `System` namespace). The `using` lines Visual Studio put there by default already include this, so only a hand-edit can break it.

You can also do a quick "does it run" test from inside `Main`. Temporarily add a few lines to `Program.cs`:

```csharp
static void Main(string[] args)
{
    Animal a = new Animal();
    a.Id = "00000001";
    a.Name = "Rex";
    a.Breed = "Labrador";
    a.Species = "Dog";
    a.Birthday = new DateTime(2019, 6, 15);
    a.SpayedOrNeutered = "Yes";
    a.Gender = "M";
    a.Colour = "Brown";
    a.VaccineStatus = "Up to date";
    a.IdentificationNumber = "985141000123456";
    a.IdentificationType = "Microchip";
    a.AdoptionFee = 250.00m;

    string line = a.ToLine();
    Console.WriteLine(line);

    Animal b = Animal.FromLine(line);
    Console.WriteLine(b.ToLine());

    Console.ReadKey();
}
```

Press **F5**. You should see the **same line printed twice** — once from `a.ToLine()`, once from the round-tripped `b.ToLine()`. If the two lines are identical, your `ToLine` and `FromLine` are a matched pair.

> **The `250.00m` suffix.** The `m` after the number tells the compiler *"this literal is a `decimal`"* (not a `double`, which is the default for decimal-point literals). Without the `m`, `a.AdoptionFee = 250.00;` is a compile error because C# won't silently convert `double` to `decimal`. Always use the `m` suffix for `decimal` literals.

**When you're done with the test, delete those lines from `Main`.** Leave `Main` empty again — Part 3 writes a proper menu.

---

## Before moving on, check yourself

- [ ] `Animal.cs` exists next to `Program.cs` and declares `public class Animal`.
- [ ] It has exactly **12 properties**, in the order: `Id`, `Name`, `Breed`, `Species`, `Birthday`, `SpayedOrNeutered`, `Gender`, `Colour`, `VaccineStatus`, `IdentificationNumber`, `IdentificationType`, `AdoptionFee`.
- [ ] All 12 are auto-properties (`{ get; set; }`), and the types match: `Birthday` is `DateTime`, `AdoptionFee` is `decimal`, the rest are `string`.
- [ ] `ToLine()` is an instance method that returns a pipe-delimited string, with `Birthday` formatted as `"dd/MM/yyyy"`.
- [ ] `FromLine(string)` is a `public static` method that returns a new `Animal`.
- [ ] I ran the round-trip test in `Main` and saw the same line printed twice. Then I deleted the test code.
- [ ] **I understand that `ToLine` and `FromLine` will be rewritten later** using a character-by-character walk with `StringBuilder` — and that the current versions are the "working but temporary" placeholders.

When every box is checked, the `Animal` class is good enough to build on. **Next part:** `AnimalRepository` — a static class that holds the whole list of animals and (once it all works) saves them to a file.
