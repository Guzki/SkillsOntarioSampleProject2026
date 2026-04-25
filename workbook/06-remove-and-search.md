# Part 6 — Remove and Search (find-and-act screens)

> **Prerequisites:** Part 5 is complete. `ConsoleUI.cs` is finished, `AddAnimal` and `DisplayAllSortedBySpecies` are using the helpers, and the menu loop is wrapped in try/catch. You can add an animal, display all of them, exit and re-launch, and your data persists.

## What this part builds

Two screens that share the same shape: **find an existing animal (or animals) and do something with the result.**

- **Remove** — show all animals in a table, prompt for the ID of the one to remove, delete and save.
- **Search** — prompt for one search term that matches against name or species, show the results via `PrintTable`.

Both screens reuse the four-step `PrintTable` calling pattern from Part 5g, so they should feel familiar by now. The new things in this part are LINQ at the call site (`Where`, `FirstOrDefault`) and one small string-handling helper (`PadLeft`) that makes the Remove screen forgiving about how the user types IDs.

> **Why bundle Remove and Search into one part?** Both are "look at the data, pick out what matches, then do something with the result." Doing them back-to-back makes the shared shape click. Search shows results; Remove acts on a single result. Same scaffolding, different verb.

---

## Part 6a — The Remove screen

`Program.cs` currently has a stub:

```csharp
private static void RemoveAnimal()
{
    Console.WriteLine("Remove animal — not yet implemented.");
}
```

Replace it with this:

```csharp
// Shows the animals in a small table, prompts for the ID of the one to
// remove, looks it up (left-padding the input with zeros so '7' matches
// '00000007'), removes from the list, and saves. Loops until a valid
// ID is entered.
private static void RemoveAnimal()
{
    ConsoleUI.DrawHeaders("Remove Animal");

    // Display a quick-reference table so the user can see what IDs exist.
    string[] headers = { "ID", "Name", "Species" };
    List<string[]> rows = new List<string[]>();
    foreach (var animal in AnimalRepository.animals)
    {
        rows.Add(new string[]
        {
            animal.Id,
            animal.Name,
            animal.Species
        });
    }
    ConsoleUI.PrintTable(headers, rows);
    Console.WriteLine();

    // Loop until the user types an ID that matches an existing animal.
    while (true)
    {
        string id = ConsoleUI.Prompt("Enter the ID of the animal to remove: ").PadLeft(8, '0');
        var animalToRemove = AnimalRepository.animals.FirstOrDefault(a => a.Id == id);
        if (animalToRemove != null)
        {
            AnimalRepository.animals.Remove(animalToRemove);
            AnimalRepository.SaveToFile();
            ConsoleUI.WriteLineColor("Animal removed successfully!", ConsoleColor.Green);
            break;
        }
        else
        {
            ConsoleUI.WriteLineColor("Invalid ID. Try again.", ConsoleColor.Red);
        }
    }

    ConsoleUI.Pause();
}
```

### What's going on

**1. Show before ask.** The first thing the screen does is print a three-column table of every animal — ID, Name, Species. The user looks at the table, picks an ID, types it. Without this preview, they'd have to remember IDs from a previous Display All call. Showing the table costs ~10 lines of code and saves the user from a memory game.

**2. `.PadLeft(8, '0')` — IDs are zero-padded strings.** Every animal Id is exactly 8 characters wide (`"00000003"`). If the user types `3`, the lookup `a.Id == "3"` would fail. `"3".PadLeft(8, '0')` returns `"00000003"` — left-padded with `'0'` characters until the string is 8 wide. Now both `3` and `00000003` find the same animal. Forgiving, and a one-line addition.

**3. `FirstOrDefault(predicate)` — returns the first match, or `null`.** This is the LINQ method to use whenever "no match" is a normal outcome you want to handle gracefully. The plain `First` throws an exception on no match; `FirstOrDefault` just returns `null`, and you check it with an `if`.

**4. The `while (true)` loop with `break` on success.** Same shape as the `PromptInt`/`PromptDecimal` parse-and-loop pattern from Part 5c — keep asking until the input is valid, then break out. On failure, write a red error and the loop iterates.

**5. Mutate and save are two explicit calls** — same rule as `AddAnimal`. The student must see "change in memory" and "write to disk" as separate operations.

### Heads-up: this version has no cancel

The loop doesn't accept "I changed my mind" as input — the only way out is to enter a real ID. If the user reaches this screen by mistake, they have to either type a real ID and remove an animal they didn't mean to, or kill the process with Ctrl+C and lose any unsaved menu state.

A small, friendly enhancement would be to accept empty input as cancel:

```csharp
string id = ConsoleUI.Prompt("Enter the ID of the animal to remove (or blank to cancel): ");
if (string.IsNullOrEmpty(id))
{
    ConsoleUI.WriteLineColor("Cancelled.", ConsoleColor.Yellow);
    break;
}
id = id.PadLeft(8, '0');
// ... rest of the loop body
```

The current version skips that for simplicity. **Note also: there's no "are you sure?" confirmation step before the deletion happens** — once you type a valid ID, that animal is gone. For a small shelter database this is fine; for anything customer-facing you'd add a Yes/No `PromptChoice` before the `Remove` call.

### Test the Remove screen

- ✅ Type a valid full ID (`00000005`) → Bubbles is gone → Display All confirms → restart the app → still gone.
- ✅ Type just `5` → same result (PadLeft makes `5` → `00000005`).
- ✅ Type `99` → red "Invalid ID" → loop reprompts → enter a valid ID → works.
- ✅ Type a really weird ID like `abc` → "Invalid ID" (because no animal has `"000000ab"` (PadLeft only pads to 8, doesn't reject non-digits) — but the lookup still fails to find a match, so it's harmless). Loop continues.

---

## Part 6b — The Search screen

Replace the stub with a single-prompt search that matches name OR species, case-insensitive, partial.

```csharp
// Prompts for one search term and shows every animal whose Name OR
// Species contains the term (case-insensitive). Empty input shows all
// animals, sorted by species and name.
private static void SearchAnimal()
{
    ConsoleUI.DrawHeaders("Search Animals");

    string searchTerm = ConsoleUI.Prompt(
        "Enter a search term (name or species, leave blank to show all): "
    ).ToLower();

    var searchResults = AnimalRepository.animals
        .Where(a => string.IsNullOrEmpty(searchTerm)
                 || a.Name.ToLower().Contains(searchTerm)
                 || a.Species.ToLower().Contains(searchTerm))
        .OrderBy(a => a.Species)
        .ThenBy(a => a.Name);

    Console.WriteLine();

    string[] headers = { "ID", "Name", "Species", "Breed", "Colour", "Birthday" };
    List<string[]> rows = new List<string[]>();
    foreach (var animal in searchResults)
    {
        rows.Add(new string[]
        {
            animal.Id,
            animal.Name,
            animal.Species,
            animal.Breed,
            animal.Colour,
            animal.Birthday.ToString("dd/MM/yyyy")
        });
    }
    ConsoleUI.PrintTable(headers, rows);

    ConsoleUI.Pause();
}
```

### What's going on

**1. One prompt, one search term.** The simplest search UI possible — type something, see what matches. No sub-menu, no "search by what?" question. The cost is that the user can't search "all dogs named Rex" with this screen, but for a 40-animal shelter database, "show me anything that mentions Rex" is enough.

**2. `.ToLower()` on input AND on the field.** Case-insensitive search. `"rex".Contains("rex")` is `true`, but `"Rex".Contains("rex")` is `false` — `Contains` is case-sensitive by default. The pattern `field.ToLower().Contains(query.ToLower())` makes the comparison case-insensitive. (`String.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0` does the same job without allocating a lower-case copy of the string, but `ToLower().Contains()` reads more like English to a beginner.)

**3. The empty-input shortcut.** `string.IsNullOrEmpty(searchTerm)` returns `true` if the user just pressed Enter without typing anything. The `||` chain evaluates left-to-right and short-circuits — when the term is empty, every animal matches, so the search doubles as "display all sorted by species and name." A free bonus screen.

**4. `OrderBy(species).ThenBy(name)` — same sort order as Display All.** Consistent presentation across screens makes the app feel cohesive.

**5. Same six columns as Display All.** Pattern C from Part 5g, repeated. By now you've written this row-building loop three times — that's the point. By the time you get to a competition, you can write it without thinking.

### A few things this Search screen does NOT do

- **No empty-result message.** If nothing matches, `PrintTable` prints "No data available to display." in gray (its built-in empty handler from Part 5d). That's serviceable; a domain-specific "No matching animals found" would be friendlier.
- **No filter scoping** (e.g. "search Name only, not Species"). One search term, two fields, OR'd together. If the user wants finer control, they use Display All with the species filter (next part touches on that).
- **No handling of multi-word queries.** Typing `golden retriever` does an exact substring match — it'll find "Golden Retriever" but not "Retriever, Golden". Splitting on whitespace and AND-ing the terms would be a natural enhancement.

### Test the Search screen

- ✅ Type `rex` → finds Rex (case-insensitive substring on Name).
- ✅ Type `dog` → finds every dog (substring matches `Dog` species, but also matches "Dogfish" if you had one — this is a feature, not a bug, of broad-substring search).
- ✅ Type `r` → finds anything with R/r anywhere in name or species.
- ✅ Type nothing (just Enter) → see every animal in the shelter, sorted.
- ✅ Type `xyz123` → empty table with "No data available to display." message.

---

## Deployment test

Build Release, copy the `.exe` to a fresh folder (or use the existing `bin\Release\` path), run it standalone. Verify:

- Display all → 40 seeded animals appear.
- Search → typing `dog` finds all 10 dogs; typing `whisk` finds Whiskers.
- Remove → enter `5` (gets padded to `00000005`) → Bubbles is gone.
- Display all → 39 remaining; close and reopen the app → still 39.

If any step fails, walk through the screen code and verify each line matches what's in this part.

---

## What's next (Part 7 — Three oldest per species)

Part 7 implements the LINQ-heaviest screen in the project. For each species in the shelter, show the three oldest animals (oldest birthday = oldest animal, since older animals were born earlier). The query is `GroupBy(species) → for each group, OrderBy(birthday).Take(3) → SelectMany(flatten back to one list)`. You'll walk each operator one at a time so you understand what it does — and you'll see the same logic written as nested `for` loops to know exactly what LINQ is hiding.
