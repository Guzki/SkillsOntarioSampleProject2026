# Part 3 — The Animal Repository (in memory first, then on disk)

> **Prerequisites:** you've finished Part 0 (design on paper) and built the `Animal` class with its properties and a working `ToLine` / `FromLine`.

## What this part builds

You have an `Animal` class — it describes **one** animal. But a shelter has **many** animals. Somewhere in your program, those animals have to live together as a collection. That's what `AnimalRepository` is for.

In this part you'll:

1. Learn what a **static class** is and why it fits this job.
2. Build `AnimalRepository` as an in-memory list with an `AddAnimal` method.
3. Wire it into `Program.cs` — add animals, display them, confirm the collection works.
4. **Only then** — after the in-memory version works end-to-end — add file saving and loading.

We split the build for a reason. A repository that doesn't save yet still shows you whether the collection logic is right. Once that works, adding files becomes a small, focused step. If you build everything at once and something breaks, you won't know which layer is broken.

> **Rule of thumb:** get the simplest version working before adding the next capability. Debug one new thing at a time.

---

## Part 3a — Static classes (the lesson)

Before you type any code, you need to understand what a **static class** is and why we're using one here. This is the conceptual heart of this part.

### The two kinds of classes

Every class in C# falls into one of two patterns. Knowing which pattern fits your job is a core design skill.

#### Instance classes — "blueprints for things"

An instance class is a template you use to **create many objects**. Each object has its own data.

```csharp
public class Animal
{
    public string Name { get; set; }
}

Animal rex = new Animal();      // one Animal
rex.Name = "Rex";

Animal fido = new Animal();     // a different Animal
fido.Name = "Fido";
```

`rex` and `fido` are two separate objects. Each has its own `Name`. That's exactly what `Animal` is — every animal in the shelter is its own object with its own values.

#### Static classes — "a bag of functions and shared data"

A static class is different. You **can never create an instance of it**. There's only ever one — the class itself. It acts more like a namespace holding a group of related functions and (optionally) shared data.

```csharp
public static class Calculator
{
    public static int Add(int a, int b) { return a + b; }
    public static int Subtract(int a, int b) { return a - b; }
}

int result = Calculator.Add(2, 3);      // call by class name
// Calculator c = new Calculator();     // compile error — you can't do this
```

You call the methods directly on the class (`Calculator.Add`), never on an instance. Visual Studio will flag `new Calculator()` as an error.

A static class can also hold **static fields** — shared data that lives for the entire program's lifetime:

```csharp
public static class HighScore
{
    private static int topScore = 0;

    public static void Record(int score)
    {
        if (score > topScore) topScore = score;
    }

    public static int Get() { return topScore; }
}
```

`topScore` is created once, when the class is first referenced. It sticks around. Every call to `Record` and `Get` reads and writes the **same** `topScore`. When the program exits, it's gone.

### Why a static class for AnimalRepository?

Three questions decide whether something should be a static class:

1. **Will there ever be more than one of these in the program at the same time?**
   No. The shelter has one list of animals. There is no world where we want two separate repositories running side by side.

2. **Does this thing need to hold shared state that everyone can see?**
   Yes. The list of animals must be the same list no matter who's asking. If the Add screen appends an animal, the Display screen has to see it.

3. **Would requiring callers to first construct an instance and then pass it around make things harder for no gain?**
   Yes. Writing `repository.Add(...)` everywhere — plus managing where that `repository` variable lives, plus making sure every screen has access to the same one — is extra bookkeeping for nothing, because there's only ever one.

When the answers are **no, yes, yes**, use a static class. That's exactly our situation.

### The cost of using static

Static classes aren't always right. The trade-offs:

- **You can't have two independent instances.** If you later needed two separate repositories ("active animals" and "archived animals"), you'd have to re-architect.
- **Static state is harder to test in isolation.** Automated tests often struggle with shared global state.
- **Anyone can touch it from anywhere.** In a large codebase with many developers, this can become a source of bugs.

For this project, none of those costs matter: one shelter, no automated tests, a small codebase. **Trade acknowledged, static it is.**

### The thing that makes static fields work for us

A static field inside a static class is created **the first time the class is referenced** and stays alive for the entire program run. That's the whole point — the list of animals must persist across menu actions, and it does, automatically, because static fields don't get destroyed between method calls.

```csharp
public static class AnimalRepository
{
    public static List<Animal> animals = new List<Animal>();
    // 'animals' is created once, stays until the program exits.
    // Every call to AddAnimal, Save, and Load operates on the same list.
}
```

This is the property of static classes you're leaning on. Without it, every menu action would start with an empty list — the program would be useless.

---

## Part 3b — Build AnimalRepository (in memory only)

Create a new file `AnimalRepository.cs` next to `Animal.cs`.

```csharp
using System;
using System.Collections.Generic;

namespace SkillsOntarioSampleProject2026
{
    public static class AnimalRepository
    {
        // In-memory list of all animals. Public so screens can iterate it directly.
        public static List<Animal> animals = new List<Animal>();

        // File path for the save/load step coming next. Define it now.
        private const string FilePath = "animals.txt";

        // Adds an animal to the list after assigning it a fresh ID.
        public static void AddAnimal(Animal animal)
        {
            animal.Id = NextId();
            animals.Add(animal);
        }

        // Computes the next available 8-digit ID by scanning the existing list.
        private static string NextId()
        {
            int max = 0;
            foreach (Animal a in animals)
            {
                int id;
                if (int.TryParse(a.Id, out id) && id > max)
                {
                    max = id;
                }
            }
            return (max + 1).ToString("D8");
        }
    }
}
```

### Understanding each piece

**`public static List<Animal> animals`** — the list itself. `public` so the screens in `Program.cs` can iterate it directly as `AnimalRepository.animals`. We trust callers not to corrupt it — there's only one caller and the surface is small. `static` because it belongs to the class, not to an instance, and because a static field lives for the whole program run. *(In a bigger codebase with many contributors you'd make this `private` and expose a read-only accessor. Here, the simpler form is fine.)*

**`private const string FilePath = "animals.txt"`** — prepare the path constant now so you only declare it once. Later, `SaveToFile` and `LoadFromFile` will both use it. `"animals.txt"` is a relative path, so the file will sit next to the running `.exe`.

**`AddAnimal(Animal animal)`** — the **only** way an animal enters the list. Notice: the caller doesn't pass an ID. The repository assigns the `Id` itself, inside `AddAnimal`, by calling `NextId`. This is an important rule — **the repository owns identity**, not the caller. If the ID-generation rule ever changes (say, a new prefix), it changes in one place.

**`NextId()` is `private`** — because nobody outside should be generating IDs. Making it private is what *makes* `AddAnimal` the single entry point for identity. A caller can't sneak around by writing `animal.Id = "99999999"` themselves before calling `AddAnimal`... well, they could, but `AddAnimal` always overwrites the `Id`. That's deliberate.

### Why NextId scans the list every call

A counter (`private static int nextIdCounter = 1;`) would be faster. But counters have two drawbacks:

1. A counter has to be persisted to the file alongside the animals. That's one more thing to get right on every save and load.
2. A scan is obviously correct: the next ID is always the highest existing one plus one. No state to get out of sync.

The list will never be big enough for the scan to be slow. Simple and correct wins here.

---

## Part 3c — Wire it to `Program.cs`

Now let's make the repository useful. We'll build the simplest possible menu — just **Add** and **Display** — and verify both work in memory.

Use raw `Console.Write` / `Console.ReadLine` for now. You'll feel the repetition and the ugliness. That's what motivates `ConsoleUI` in the next part — don't cheat yourself out of feeling the pain first.

```csharp
using System;

namespace SkillsOntarioSampleProject2026
{
    class Program
    {
        // Application entry point.
        static void Main(string[] args)
        {
            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("Animal Rescue Management System");
                Console.WriteLine("1. Add animal");
                Console.WriteLine("2. Display all animals");
                Console.WriteLine("3. Exit");
                Console.Write("Choose: ");
                string choice = Console.ReadLine();

                if (choice == "1") AddAnimal();
                else if (choice == "2") DisplayAll();
                else if (choice == "3") running = false;
            }
        }

        // Prompts the user for each field, then adds the animal to the repository.
        static void AddAnimal()
        {
            Animal animal = new Animal();
            Console.Write("Species: ");
            animal.Species = Console.ReadLine();
            Console.Write("Name: ");
            animal.Name = Console.ReadLine();
            // For now, fill in just a couple of fields to confirm the pipeline works.
            // You'll add the other ten in the next part when ConsoleUI is ready.

            AnimalRepository.AddAnimal(animal);
            Console.WriteLine("Added. Press any key.");
            Console.ReadKey();
        }

        // Prints every animal in the repository.
        static void DisplayAll()
        {
            foreach (Animal a in AnimalRepository.animals)
            {
                Console.WriteLine(a.ToLine());
            }
            Console.WriteLine("Press any key.");
            Console.ReadKey();
        }
    }
}
```

### Test the in-memory version

1. Build and run.
2. Pick **1. Add animal**. Type a species and a name.
3. Pick **1** again. Add a second animal.
4. Pick **2. Display all animals**. Both should appear. The first should have ID `00000001`, the second `00000002`.
5. Pick **3. Exit**. Relaunch the program.
6. Pick **2. Display**. The list is **empty**.

If steps 4 and 6 both behaved as described, the in-memory layer works. Step 6 being empty is expected — you haven't added file I/O yet. That's next.

---

## Part 3d — Add file reading and writing

Now you'll add two methods to `AnimalRepository`: `SaveToFile` and `LoadFromFile`. The pattern uses `StreamWriter` and `StreamReader` in `while` loops, not `File.ReadAllLines` / `File.WriteAllLines`. That's deliberate — `StreamReader.ReadLine` in a `while` loop is the pattern judges look for.

### Update AnimalRepository.cs

Add `using System.IO;` at the top of the file, then add these two methods inside the class:

```csharp
// Writes every animal to the file, one per line.
public static void SaveToFile()
{
    using (StreamWriter writer = new StreamWriter(FilePath))
    {
        foreach (Animal a in animals)
        {
            writer.WriteLine(a.ToLine());
        }
    }
}

// Loads every animal from the file, replacing the in-memory list.
public static void LoadFromFile()
{
    animals.Clear();
    if (!File.Exists(FilePath)) return;

    using (StreamReader reader = new StreamReader(FilePath))
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            animals.Add(Animal.FromLine(line));
        }
    }
}
```

### Three things to understand in this code

1. **`using (StreamWriter writer = new StreamWriter(FilePath))`.** The `using` block guarantees the file is closed when the block ends, even if an error happens inside. Without it, Windows could hold the file open and the next save would silently fail. `using` is not optional here — always use it around streams.
2. **`if (!File.Exists(FilePath)) return;`.** The first time the program runs, the file doesn't exist yet. `File.Exists` prevents a crash. Missing file = empty list, not an error.
3. **`animals.Clear();` before loading.** Because `Load` **replaces** the list, it doesn't append. If you didn't clear first, loading twice would double every animal.

### Wire it into Program.cs

Two small changes:

**1.** At the top of `Main`, before the menu loop:

```csharp
static void Main(string[] args)
{
    AnimalRepository.LoadFromFile();   // add this line
    bool running = true;
    while (running)
    {
        ...
    }
}
```

**2.** Inside `AddAnimal`, right after `AnimalRepository.AddAnimal(animal);`:

```csharp
AnimalRepository.AddAnimal(animal);
AnimalRepository.SaveToFile();   // add this line
```

### Why the save is a separate call in the menu code

`SaveToFile` is **not** hidden inside `AddAnimal`. It's a second, explicit call the menu code makes right after it. This is deliberate. When someone reads the Add screen, they see two distinct operations:

```csharp
AnimalRepository.AddAnimal(animal);   // change in memory
AnimalRepository.SaveToFile();        // write to disk
```

Those are different concerns. Mixing them into one call hides the file I/O and makes the program feel magic — *"how did my animal end up saved?"*. Keeping them separate teaches the reader (and you, writing it) that persistence is an action you take, not a side effect you hope for.

> **Rule for this codebase:** every menu action that mutates the list follows the two-step pattern: mutate, then save.

### Test the full round trip

1. Add two animals.
2. Open `animals.txt` in Notepad. It's next to the running `.exe` — in `bin\Debug\` if you ran in Debug, `bin\Release\` if you ran Release. Confirm the file contains two pipe-delimited lines.
3. Exit the program. Relaunch.
4. Display. Both animals should still be there, with their original IDs.

If that round-trip works, **you're done with this part**.

---

## Before moving on, check yourself

- [ ] I can explain in my own words what a static class is and why `AnimalRepository` is one.
- [ ] I can explain why `NextId` is private and why the caller of `AddAnimal` doesn't pass in an ID.
- [ ] `AnimalRepository.cs` has: one `public static` list, one `private const string FilePath`, a public `AddAnimal`, a private `NextId`, a public `SaveToFile`, a public `LoadFromFile`.
- [ ] I can Add an animal, Display it, quit, relaunch, and still see it.
- [ ] `animals.txt` opens in Notepad and shows one pipe-delimited line per animal.

**Next part:** the raw `Console.Write` / `Console.ReadLine` calls in `AddAnimal` are clumsy, and the output is ugly. That's the pain that motivates `ConsoleUI` — a set of helpers that make every screen look clean without you having to think about colors or formatting. We build those helpers **as we need them**, driven by what the next few screens actually demand.
