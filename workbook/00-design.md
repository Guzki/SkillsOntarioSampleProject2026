# Part 0 — Design Before Code

This is the pre-code planning step. Before you write a single line of C#, you're going to sketch the whole app on paper. This usually takes 30–60 minutes. **Do not skip it.**

## Why design before coding?

When you start typing code right away, you're trying to solve two hard problems at the same time:

1. **What should the code do?**
2. **How should I write the code that does it?**

Most bugs and wasted time come from mixing these up. Figure out problem 1 first, on paper, where changes cost you nothing. Then problem 2 — writing the actual C# — becomes mostly mechanical.

Good design on paper means:

- No surprises partway through coding ("wait, I forgot the vaccine status field")
- Clearer class boundaries — each class has one clear job
- A sketch you can refer back to when you get lost
- At the competition itself: 15 minutes of design up front saves 2 hours of confusion later

> **Rule of thumb:** if you can't describe what you're about to build in plain English to a friend who doesn't know C#, you aren't ready to type.

---

## The design questions

Work through these questions in order. Use a notebook or a piece of printer paper. **Do not open Visual Studio yet.**

---

### Question 1 — What are the nouns and verbs in the problem?

Read the problem statement carefully. Underline every **noun** and every **verb**.

- **Nouns** become either classes or fields of a class. *"Animal"* is a class. *"Name"*, *"species"*, *"breed"*, *"birthday"* are fields inside that class.
- **Verbs** become methods — *jobs* your program does. *"Add"*, *"remove"*, *"search"*, *"sort"*, *"display the three oldest per species"* are all jobs.

Write them down in two columns.

#### Why this works

English grammar mirrors object-oriented code. Nouns are things; verbs are what you do to them. Starting here forces you to actually *read* the problem statement instead of skimming.

#### Worksheet

```
Nouns I found:               Verbs I found:
_________________            _________________
_________________            _________________
_________________            _________________
_________________            _________________
_________________            _________________
_________________            _________________
```

---

### Question 2 — What is the main "thing" in this problem?

Scan your noun list. Which noun shows up in almost every verb? That's your **entity** — the main thing your program manages.

For this project, the main thing is: **animal**. Every verb is about doing something with animals. *"Name"* and *"breed"* aren't main things — they're attributes *of* an animal.

Some programs have more than one main thing. A bigger shelter app might also track **adopters** (who adopts which animal) and **volunteers** (who works which shifts). Three entities, three classes. Stay alert to this in future projects — but for this one, we have exactly **one** entity.

#### Worksheet

```
Main entity: _______________
```

---

### Question 3 — What does the main entity have?

List every **field** (also called a *property*) that an animal has. For each one, write four things:

- **Name** — what you'll call it in code
- **Type** — `string`, `int`, `decimal`, `DateTime`, etc.
- **Valid values / rules** — any constraints on what's allowed
- **Example** — one real-looking sample value

Do this as a table. Fill it in straight from the problem statement, field by field.

#### Worksheet

| # | Name | Type | Valid values / rules | Example |
|---|------|------|----------------------|---------|
| 1 | Id | string | 8 digits, zero-padded, unique, auto-assigned | `"00000001"` |
| 2 | Name | string | any text | `"Rex"` |
| 3 | Breed | string | must be valid for the species (`"unknown"` always valid) | `"Labrador"` |
| 4 | | | | |
| 5 | | | | |
| 6 | | | | |
| 7 | | | | |
| 8 | | | | |
| 9 | | | | |
| 10 | | | | |
| 11 | | | | |
| 12 | | | | |

Fill in all 12 fields. This table is the **single source of truth** for the Animal class. You'll refer back to it constantly when you write `Animal.cs`, `ToLine`, `FromLine`, and the Add screen.

#### Why a table?

If you can't fill in all four columns cleanly, you don't yet understand the field. The *"valid values"* column especially — if you can't state the rule in plain English now, you won't be able to write the validation code later.

---

### Question 4 — What jobs need to be done?

Go back to your verb list. For every verb, rewrite it as a **specific, complete job**.

Don't write: *"search"*.
Write: *"search for animals whose name contains a word the user types"* and *"search for animals whose species matches a choice the user picks from a list."*

Don't write: *"display oldest"*.
Write: *"for each species, find the three animals with the earliest birthdays, and show them in a table grouped by species."*

#### Why be specific?

Because *"search"* is vague and hides complexity. Specific descriptions expose questions you haven't answered yet:

> *"When I search by name, is it case-sensitive? Does it match partial names or exact names? What happens when no animals match?"*

Answer these questions on paper, not mid-coding.

#### Worksheet

```
Job: _________________________________________________________
Job: _________________________________________________________
Job: _________________________________________________________
Job: _________________________________________________________
Job: _________________________________________________________
Job: _________________________________________________________
Job: _________________________________________________________
Job: _________________________________________________________
```

---

### Question 5 — Sort jobs into buckets. These become your classes.

**This is the most important question in the whole design step.**

Take every job from Question 4 and drop it into one of these buckets:

1. **Jobs about one animal on its own.** Things a single animal "knows how to do" by itself — like serializing itself to a line of text, or parsing itself back from a line.
2. **Jobs about the whole collection of animals.** Adding to it, removing from it, searching it, sorting it, loading it from a file, saving it to a file.
3. **Jobs about talking to the user.** Asking questions, drawing the menu header, printing tables, showing colored text.
4. **Jobs about domain rules.** Facts about the problem that aren't really storage and aren't really UI. *"Is this breed valid for this species?"* is the main example here.

Each bucket becomes a class:

- **Bucket 1** → `Animal` (one instance per animal — each animal holds its own data and knows how to serialize itself)
- **Bucket 2** → `AnimalRepository` (one **static** class; holds the list of all animals and does collection-level work)
- **Bucket 3** → `ConsoleUI` (a **static** helper class with prompts, headers, tables — useful in any console app, not just this one)
- **Bucket 4** → **you decide** (see Question 6)

> **Aside — why some classes are static.** `AnimalRepository` and `ConsoleUI` are *static* classes because there is only ever **one** of each in the program. You never need "two separate animal databases" or "two different console UIs" running side by side, so you skip the idea of "making an instance" entirely. A static class is namespaced like a class but acts like a bag of functions and shared data. Static fields inside it (like the `List<Animal>` inside `AnimalRepository`) exist for the entire program's lifetime — they're essentially well-behaved globals.
>
> `Animal` is **not** static, because you want *many* of them at once (one per animal in the shelter). Each Animal is an instance with its own `Name`, `Species`, etc.

#### Worked example

Here's what sorting might look like for a few of the jobs:

| Job | Bucket | Goes into class |
|-----|--------|-----------------|
| Turn one animal into a pipe-delimited line | 1 | `Animal.ToLine()` |
| Parse a pipe-delimited line back into an animal | 1 | `Animal.FromLine(string)` |
| Load every animal from `animals.txt` | 2 | `AnimalRepository.LoadFromFile()` |
| Save every animal to `animals.txt` | 2 | `AnimalRepository.SaveToFile()` |
| Add one animal to the in-memory list | 2 | `AnimalRepository.AddAnimal(Animal)` |
| Find every animal whose name contains "rex" | 2 | `AnimalRepository.SearchByName(string)` |
| Ask the user to type a date | 3 | `ConsoleUI.PromptDate(label)` |
| Show the main menu and wait for a choice | 3 | `ConsoleUI.PromptChoice(...)` |
| Check whether "Poodle" is valid for "Dog" | 4 | ??? — see Question 6 |

Do this for every job on your list. When you're done, the classes almost write themselves.

---

### Question 6 — Where does breed validation live?

Here's a real design decision, and both answers compile and work. The point is for you to decide **deliberately**, with a reason — not to guess.

**The job:** *"Given a species and a breed, is that a valid combination?"* (With the twist that `"unknown"` is always valid, no matter the species.)

You have three plausible homes:

#### Option A — its own class, `BreedData.cs`

A small static class that owns the `Dictionary<string, string[]>` of species → valid breeds, plus an `IsValid(species, breed)` method.

- *Good:* single responsibility — this class does exactly one thing.
- *Good:* the dictionary data is bulky. Keeping it separate means `AnimalRepository` stays focused and easy to read.
- *Cost:* one more file, one more class name in your head.

#### Option B — inside `AnimalRepository.cs`

Add a private static dictionary and an `IsValidBreed(species, breed)` method to the repository.

- *Good:* one less file. Lower total class count.
- *Cost:* the repository now does two unrelated jobs — **storage** (mutable list + file I/O) *and* **validation rules** (static reference data). Two different natures mixed in one class.

#### Option C — inside `Animal.cs`

An Animal "knows" what breeds are valid for its own species, via a static helper that lives in the same file.

- *Good:* cohesion — the rules for what makes a valid Animal live with the Animal class.
- *Cost:* we agreed on *"one class per file."* A static helper class inside `Animal.cs` breaks that rule.

#### Ask yourself these questions

1. *"Does `AnimalRepository` need to know what breeds are valid in order to do its main job — storing and loading animals?"* → **No.** The repository stores whatever you hand it.
2. *"Is breed data the same **kind** of thing as the list of animals?"* → **No.** One is fixed reference data (never changes), the other is mutable state that grows and shrinks.
3. *"Will the bulky dictionary make one class harder to read if I put it there?"* → Look at the size of the data. If yes, separate.

#### My recommendation

**Option A — its own file.** The dictionary is bulky, the validation is a distinct job, and separating it teaches a clean pattern (a class that's pure reference data, with no mutable state and no instances).

But Option B is defensible if you really dislike having an extra file for something this small. Just make sure you're picking B **because you decided**, not because you couldn't be bothered.

> **Postscript — what this project actually shipped.** As the build progressed we ended up dropping breed validation entirely: the `Add Animal` screen takes Breed as free text (any string the user types is accepted). No `BreedData.cs`, no `IsValid` check. The design exercise above is still worth doing — it teaches the "where does this responsibility live?" question every domain class eventually faces — but it's no longer load-bearing for this codebase. If you decide to add a closed breed list later, Option A's pattern is the one to follow.

#### Worksheet

```
My decision: Option _____
Why: _________________________________________________________
_____________________________________________________________
```

---

### Question 7 — Draw the "who calls whom" diagram

On paper, draw a box for each class. Then draw an arrow from class A to class B whenever A **uses** B (calls a method on B, reads data from B, creates an instance of B, etc.).

Here's roughly what the diagram should look like for this project:

```
                ┌──────────────┐
                │   Program    │
                └───┬──────┬───┘
                    │      │
           ┌────────┘      └─────────┐
           ▼                         ▼
    ┌──────────────┐        ┌──────────────────┐
    │  ConsoleUI   │        │ AnimalRepository │
    └──────────────┘        └─────────┬────────┘
                                      │
                                      ▼
                              ┌────────────┐
                              │   Animal   │
                              └────────────┘
```

(In an earlier draft we'd have had a `BreedData` box hanging off `Animal` for breed validation — Option A from Question 6. We dropped that piece, so the diagram simplified to the four boxes shown.)

#### Things to check on your diagram

- **All arrows go one way.** If you have a cycle (A → B and B → A), one of your classes is doing someone else's job. Rethink the buckets.
- **`ConsoleUI` has zero arrows going out to app-specific classes.** It doesn't know anything about animals — it's a generic helper you could drop into any console project. If you catch yourself drawing an arrow from `ConsoleUI` to `Animal`, you've accidentally mixed UI with domain code.
- **`Animal` doesn't call `AnimalRepository`.** A single animal doesn't need to know it's in a collection. The repository manages animals; animals don't manage themselves.
- **`Program` sits at the top.** It's the conductor that wires everything together. Data flows *down* through it.

#### Why draw this?

Because a cycle or a weird arrow on paper is cheap to fix. Finding the same cycle mid-coding means tearing apart code that already works.

---

### Question 8 — Sketch the file format

You're storing animals in a plain text file with a pipe (`|`) as the delimiter between fields. Write one sample line by hand, with real values:

```
00000001|Rex|Labrador|Dog|15/06/2019|Yes|M|Brown|Up to date|985141000123456|Microchip|250.00
```

Now check three things:

1. **Are the fields in exactly the same order as your Question 3 table?** They must be. This order is how `ToLine` writes and how `FromLine` reads. If it ever changes, every saved file becomes unreadable.
2. **Does every field have a clean string form?** Dates are `dd/MM/yyyy`. Decimals are written plainly (`250.00`, not `$250.00`). Enumerated values are spelled out in full (`"Up to date"`, not `"UTD"`).
3. **Could any field contain a `|` character?** Probably not — names and breeds don't usually contain pipes. If you want to be paranoid, disallow `|` in the Name prompt. For this project, just pick delimiters that won't collide and move on.

#### Worksheet

```
My sample line:
_____________________________________________________________
_____________________________________________________________
```

---

### Question 9 — Sketch the main menu

Write the main menu on paper, numbered:

```
Main Menu
---------
1. Add animal
2. Remove animal
3. Search
4. Display all (sorted by species)
5. Display three oldest per species
6. Help
7. Exit
```

Beside each menu item, in one sentence, note what happens when the user picks it:

- *Which class does the work?*
- *Which `ConsoleUI` helper gathers the input?*

#### Why do this?

Because every menu item will become **exactly one method** in `Program.cs`. Listing them now means your `Program.cs` skeleton writes itself later.

---

## When Part 0 is done

You should now have, on paper:

- [ ] A table of the 12 Animal fields with types and rules
- [ ] A list of every job your program does, sorted into class-buckets
- [ ] A written decision about where breed validation lives, with a reason
- [ ] A diagram of which classes call which
- [ ] One sample line of the file format
- [ ] A numbered main menu with a one-line note per item

If any of those six things is missing or fuzzy, **finish Part 0 before opening Visual Studio**. The temptation to "just start coding" is real — resist it. Every minute spent on paper here saves five minutes of refactoring later.

When all six are done, you're ready for **Part 1: Project Setup**.
