# Part 7 — Three Oldest Per Species (the LINQ chapter)

> **Prerequisites:** Part 6 is complete. Remove and Search are working. You've used LINQ in small doses already (`Where`, `OrderBy`, `ThenBy`, `FirstOrDefault`) and they're starting to feel familiar.

## What this part builds

A single screen that, **for every species in the shelter**, shows the **three oldest animals** of that species. Older birthday = older animal, because earlier birthday means born longer ago.

If your shelter has 10 dogs, this screen shows 3 of them — the three oldest. If it has only 2 birds, it shows both. Across 8 species, expect somewhere between 6 and 24 rows on screen.

The interesting part of this screen isn't the UI — it's the **LINQ** that produces the result. You're about to use one new LINQ operator (`GroupBy`) and combine it with operators you already know. We'll go slowly because LINQ is the part of C# students lose the most time on, and the part judges most often look for.

> **Why this gets a whole chapter.** You've been writing one-line LINQ queries that mostly look like English ("where the species is Dog, ordered by name"). This screen needs three steps that you couldn't fit into a single `Where` no matter how clever. Once you understand `GroupBy` as a real operator with a clear mental model, every other LINQ chain you read becomes more readable.

---

## Part 7a — LINQ recap (what you already know)

Before adding anything new, let's name the LINQ operators you've already used in earlier parts. Each one takes a sequence in, runs a transformation, and gives a (possibly different) sequence out.

| Operator | What it does | Where you saw it |
|---|---|---|
| `Where(predicate)` | Keep only the elements where the predicate returns `true`. | Search screen — keep animals whose name or species contains the search term. |
| `OrderBy(keySelector)` | Sort ascending by the key. | Display All — sort by species. |
| `ThenBy(keySelector)` | Secondary sort, used after `OrderBy` to break ties. | Display All — within each species, sort by name. |
| `FirstOrDefault(predicate)` | Return the first matching element, or `null` if none. | Remove screen — find the animal with the matching ID. |
| `Take(n)` | Return at most the first `n` elements (or fewer if the sequence is shorter). | Coming up next. |
| `Select(transformer)` | Map each element to something else. You've seen this implicitly inside `foreach (var x in things.Where(...))`. | Coming up next. |

**Three things to remember about every LINQ operator:**

1. **They don't change the source list.** `animals.Where(...)` does not modify `animals`. It returns a new sequence describing the result.
2. **They're lazy.** The query doesn't actually run until you iterate it (e.g. with a `foreach`) or force it (e.g. with `.ToList()` or `.Count()`). For everything in this project, lazy is fine — you build a query, then `foreach` it once.
3. **You can chain them.** The output of one operator is the input of the next. `things.Where(...).OrderBy(...).ThenBy(...)` is three transformations applied in sequence.

---

## Part 7b — `GroupBy`: the one new operator

`GroupBy` is the operator that earns this whole chapter. Mental model:

> **Take a flat list, sort similar items into buckets keyed by some property, hand back a list of buckets.**

Concrete example with the seeded animal data:

```
input — flat list of 40 animals
   [Rex, Whiskers, Tweety, Thumper, Bubbles, Daisy, Hammy, Buddy, Luna, Max, ... ]

       │
       ▼  GroupBy(a => a.Species)
       │

output — sequence of groups, one per distinct species
   ┌─ Group "Dog":           [Rex, Buddy, Max, Bella, Charlie, Lucy, Cooper, Daisy, Rocky, Molly]
   ├─ Group "Cat":           [Whiskers, Luna, Tigger, Mittens, Felix, Cleo, Smokey]
   ├─ Group "Bird":          [Tweety, Pepper, Sunny, Mango]
   ├─ Group "Rabbit":        [Thumper, Hopper, Snowball, Cinnamon]
   ├─ Group "Fish":          [Bubbles, Nemo, Gill]
   ├─ Group "Barnyard":      [Daisy (cow), Wilbur, Bessie, Clover]
   ├─ Group "Small & Furry": [Hammy, Peanut, Sprinkles, Coco]
   └─ Group "Other":         [Spike, Slither, Shelly, Iggy]
```

A flat list went in, a list-of-lists came out. Each output element (each "group") has:

- A **`Key`** — the value the group was keyed on (e.g. `"Dog"`, `"Cat"`).
- The **animals themselves** — iterating the group with `foreach (var animal in group)` gives you each animal in that bucket.

Code:

```csharp
var groupedBySpecies = AnimalRepository.animals.GroupBy(a => a.Species);
```

That single line builds the structure shown above. `groupedBySpecies` is a sequence of groups; you iterate it with `foreach (var group in groupedBySpecies)`, and inside that, you can iterate each group's animals with another `foreach (var animal in group)`. **A nested loop is the natural shape for "for each group, do something with its members."**

> **Why is the type called `IGrouping<TKey, TElement>`?** Because each group is both a *thing with a Key* (the species name) and *a sequence of elements* (the animals of that species). You can use `group.Key` to get the species name and `foreach (var x in group)` to walk the members. For our purposes that's the whole API surface.

---

## Part 7c — `Take(n)`: the second new operator

Once you have a sorted sequence, `Take(n)` returns just the first `n` elements. Combine it with `OrderBy(birthday)`:

```csharp
group.OrderBy(a => a.Birthday).Take(3);
```

reads as: "sort this group by birthday ascending (earliest birthday first, which is the oldest animal), then keep the first three." The result is a sequence of up to three animals — the three oldest in that group.

`Take(3)` is forgiving: if the group has only 2 elements, you get 2. No error, no padding. That's exactly what we want — show what we have.

---

## Part 7d — Putting it together (without `SelectMany`)

You could chain everything into one expression with a fourth operator (`SelectMany`) to flatten the groups back to a flat list. But for a beginner, this is one too many concepts at once. **Use a `foreach` instead — it does the same flattening job with code that reads top-to-bottom.**

```csharp
private static void DisplayThreeOldestPerSpecies()
{
    ConsoleUI.DrawHeaders("Three Oldest Animals Per Species");

    string[] headers = { "ID", "Name", "Species", "Breed", "Colour", "Birthday" };
    List<string[]> rows = new List<string[]>();

    // Step 1 — group every animal by its species.
    // Result is a sequence of groups; each group has a Key (species name) and
    // contains the animals of that species.
    var groupedBySpecies = AnimalRepository.animals.GroupBy(a => a.Species);

    // Step 2 — for each group, sort by Birthday and take the three oldest.
    // The OUTER foreach walks the species buckets; the INNER foreach walks
    // the three animals we picked from each bucket. Adding rows to one list
    // inside the inner loop naturally produces a flat result table — no
    // SelectMany needed.
    foreach (var group in groupedBySpecies)
    {
        var threeOldest = group.OrderBy(a => a.Birthday).Take(3);
        foreach (var animal in threeOldest)
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
    }

    ConsoleUI.PrintTable(headers, rows);
    ConsoleUI.Pause();
}
```

### Read it like a sentence

1. *Take all animals.*
2. *Group them by species.* — produces 8 buckets, one per species.
3. *For each bucket:*
    - *Sort by birthday and take the three oldest.* — produces up to 3 animals.
    - *Add each of those animals as a row in our table.* — accumulates rows across all buckets.
4. *Render the table.*

Every statement maps to one or two lines of code. Read the method top-to-bottom and you get the algorithm. That's the goal.

### Why two LINQ steps and not one chain

You *could* compress steps 1 and 2 into a single LINQ expression with `SelectMany`:

```csharp
// Same logic, all in one chain — included here just so you know it exists.
// Don't copy this into the project; the foreach version above is clearer.
var oldestPerSpecies = AnimalRepository.animals
    .GroupBy(a => a.Species)
    .Select(g => g.OrderBy(a => a.Birthday).Take(3))   // each element is itself a sequence of up to 3
    .SelectMany(seq => seq);                           // flatten sequence-of-sequences back to flat
```

That works. It's also harder to read for someone learning LINQ. The `Select` returns a sequence-of-sequences (one mini-list of three animals per species), and `SelectMany` undoes the nesting. **You're learning two new operators (`Select` and `SelectMany`) just to avoid writing a `foreach` you were going to write anyway.** Skip it. The two-step version does the same job in code that reads more naturally.

---

## Part 7e — Test the screen

With your seeded data, the three oldest dogs should be **Cooper** (2016), **Buddy** (2017), **Bella** (2018). The three oldest cats: **Tigger** (2017), **Whiskers** (2018), **Smokey** (2018). The oldest bird is **Mango** the African Grey from 2015 — by far the oldest of any bird. The oldest animal in the entire shelter is **Shelly** the tortoise from 2014, who lives in the "Other" species bucket.

- ✅ Branded header bar shows "Three Oldest Animals Per Species".
- ✅ Eight blocks of up to three rows each.
- ✅ Within each block, the rows are in **birthday-ascending** order (oldest first, then second-oldest, then third-oldest).
- ✅ "Fish" only has 3 animals (Bubbles, Nemo, Gill) — `Take(3)` returns all three.
- ✅ "Other" has 4 animals — Shelly (2014), Spike (2019), Slither (2023), Iggy (2022) — and only the three oldest (Shelly, Spike, Iggy) appear; Slither is dropped because she's the youngest.
- ✅ Press any key returns to the menu.

If a species block is missing, your `GroupBy` lost a key — check that `a.Species` is the right property. If the wrong animals appear, your `OrderBy` is wrong (probably descending instead of ascending — `OrderBy(a => a.Birthday)` is ascending, which puts the oldest first since their birthday is earliest).

---

## Part 7f — LINQ challenges (optional)

The base project is now functionally complete. Here are LINQ exercises to work through on your own — each one is a one- or two-line query you can paste into a temporary screen, write to the console, and verify by hand. **Don't look up the answers; struggle with each one for ten minutes before checking documentation.**

Each challenge has a difficulty marker:
- 🟢 Easy — one operator, looks like Search/Display.
- 🟡 Medium — two or three operators chained.
- 🔴 Hard — uses `GroupBy` or other concepts from this part.

### 🟢 Easy — single operator

1. **Count the dogs.** Use `Count(predicate)` or `.Where(...).Count()` to print the number of animals whose species is `"Dog"`.
2. **Find the most expensive adoption fee.** Use `.Max(a => a.AdoptionFee)` to print the highest fee in the shelter.
3. **Cheapest fee.** Same shape, with `Min`.
4. **Animals overdue for vaccinations.** Filter to those where `VaccineStatus == "Late"` and print their names.

### 🟡 Medium — two or three operators

5. **Average adoption fee for cats.** Combine `Where` (species == "Cat") and `Average` (`a.AdoptionFee`). Print to two decimal places.
6. **Animals with no identification number.** Filter on `string.IsNullOrEmpty(a.IdentificationNumber)` and display the result with `PrintTable`.
7. **Animals adopted before 2020.** Filter on `a.Birthday.Year < 2020` and order by birthday ascending, then display.
8. **Newest five animals across all species.** Combine `OrderByDescending(a => a.Birthday)` with `Take(5)`.

### 🔴 Hard — uses `GroupBy`

9. **Count of animals per species.** Use `GroupBy(a => a.Species)`. For each group, print `group.Key` and `group.Count()`. The output should look like:
   ```
   Dog: 10
   Cat: 7
   Bird: 4
   ...
   ```

10. **Species with the most animals.** Order the groups from `(9)` by `Count()` descending and `Take(1)`. Print the winner's species name and count.

11. **Average adoption fee per species.** `GroupBy(species)` then for each group compute `group.Average(a => a.AdoptionFee)`. Display as a two-column table (species, average fee).

12. **Newest animal in each species.** `GroupBy(species)`, then for each group, `OrderByDescending(birthday).First()`. Render as a table with one row per species.

13. **Species with at least one "Late" vaccination.** `Where` on vaccine status, then `GroupBy(species)`, then walk the group keys.

### Bonus — composing what you've learned

14. **Build a "shelter stats" screen** that combines (9), (10), and (11) into one display. Use `DrawHeaders("Shelter statistics")`, three labeled sections (with `WriteLineColor` cyan headings), and a small table for each.

> **Tip for working through these:** add a temporary menu option (e.g. case "8" or "9") in `Program.cs` that calls a new method. Write the LINQ, render to `Console.WriteLine` or `PrintTable`, run, eyeball the output. Once you're confident in the result, decide if it's worth keeping as a real screen or revert.

---

## What's next

**Part 8** is the wrap-up — Help screen retrofit (already done if you followed it), README, deployment, and the final zip.

**Part 9** is **beyond the base scope** — adding *adopters* and *adoptions* tables, linking them by ID. It's a tour of relational data thinking and LINQ joins. Treat it as bonus material once Part 8 ships.
