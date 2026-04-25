# Part 9 — Beyond Scope: Adopters and Adoptions (relational data)

> **Beyond the base tier.** The Skills Ontario rubric only requires the animal-management features in Parts 0–8. This part is a tour of **relational data** — tracking *people* (adopters) and the *links* between people and animals (adoptions). It introduces foreign keys, multi-file persistence, and LINQ joins. **Skip this if you're focused on the competition deliverable; come back to it as an extension once Part 8 ships.**

> **Prerequisites:** Parts 0–8 complete. Animals add/remove/search/display/three-oldest all work end-to-end. You're comfortable with LINQ `Where`, `OrderBy`, `GroupBy`, and `FirstOrDefault`.

## What this part adds

A real animal-rescue app does more than list animals — it tracks **who adopted which animal, and when.** That requires three new ideas that the base project deliberately avoided:

1. **A second domain object** (`Adopter`) — people who can adopt animals. Has its own ID, name, contact info.
2. **A linking record** (`Adoption`) — pairs an Animal ID with an Adopter ID, plus an adoption date. This is the "join row" that makes the relationship explicit.
3. **Multi-file persistence** — three text files instead of one. `animals.txt`, `adopters.txt`, `adoptions.txt`. Each is loaded on startup and saved after every mutation.

You'll also see your first **LINQ join** — combining data from two lists by matching IDs.

> **Why "beyond scope"?** Because adding a second domain object more than doubles the surface area of the app, and judges aren't grading on it. But it's also the most realistic next step for the project. If the student wants to keep going after the comp, this is where they go next — and the relational concepts here (foreign keys, multi-table queries, joins) are the same concepts they'll see in any real database course.

---

## Part 9a — The relational concept (linking by ID)

So far, every piece of data in the app is in one place: an `Animal` has its name, breed, and adoption fee inside the same object. But what if you wanted to know **who adopted Rex?**

You *could* add fields to `Animal` — `AdopterName`, `AdopterEmail`, `AdoptionDate`. But then:

- An adopter who takes home two animals has their info duplicated across two `Animal` rows.
- If they update their phone number, you have to update every animal they adopted.
- An adopter who hasn't yet picked an animal has no row anywhere.
- A returned-then-re-adopted animal has no history.

The fix is to **separate adopters from animals** and link them with a third file. Three tables, each with its own ID:

```
animals.txt
  Id | Name | Species | ...
  00000001 | Rex | Dog | ...

adopters.txt
  Id | Name | Email | Phone | Address
  AD000001 | Alice Khan | alice@example.com | 555-0100 | 12 Maple St

adoptions.txt
  Id | AnimalId | AdopterId | AdoptionDate | Status
  AP000001 | 00000001 | AD000001 | 2024-08-15 | Active
```

The key insight: **the adoption row links an animal ID to an adopter ID without copying any data.** If Alice changes her phone number, you update one row in `adopters.txt`. If Rex is returned, you update one row in `adoptions.txt`. Nothing is duplicated.

This is the relational model — many real databases do exactly this, just with more sophisticated tooling around it.

> **Why prefix the IDs?** Because mixing 8-digit numeric strings means you can tell at a glance which file a row came from. `AD000001` is an Adopter; `AP000001` is an adoption record; `00000001` is an animal. The prefix is purely human-readable convention; the code still treats them as opaque strings.

---

## Part 9b — Design the new classes

Two new C# files. Same shape as `Animal.cs`: properties + `ToLine` + `FromLine`.

### `Adopter.cs`

```csharp
public class Adopter
{
    public string Id { get; set; }       // "AD000001" — 8-char including prefix
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }

    public string ToLine()
    {
        return $"{Id}|{Name}|{Email}|{Phone}|{Address}";
    }

    public static Adopter FromLine(string line)
    {
        var parts = line.Split('|');
        return new Adopter
        {
            Id = parts[0],
            Name = parts[1],
            Email = parts[2],
            Phone = parts[3],
            Address = parts[4]
        };
    }
}
```

Five fields — minimal. You can add `City`, `Postal`, `Notes`, etc. later. The shape is intentionally identical to `Animal.cs` so the student sees the pattern transfer.

### `Adoption.cs`

```csharp
public class Adoption
{
    public string Id { get; set; }                // "AP000001"
    public string AnimalId { get; set; }          // foreign key → Animal.Id
    public string AdopterId { get; set; }         // foreign key → Adopter.Id
    public DateTime AdoptionDate { get; set; }
    public string Status { get; set; }            // "Active" or "Returned"

    public string ToLine()
    {
        return $"{Id}|{AnimalId}|{AdopterId}|{AdoptionDate.ToShortDateString()}|{Status}";
    }

    public static Adoption FromLine(string line)
    {
        var parts = line.Split('|');
        return new Adoption
        {
            Id = parts[0],
            AnimalId = parts[1],
            AdopterId = parts[2],
            AdoptionDate = DateTime.Parse(parts[3]),
            Status = parts[4]
        };
    }
}
```

The two **foreign keys** (`AnimalId` and `AdopterId`) are the relational glue. They're plain strings on this row, but they *mean* "go find the Animal with this Id" and "go find the Adopter with this Id" in their respective tables. `Status` lets you mark an adoption as returned without deleting the row — the historical record stays.

---

## Part 9c — Two more repositories

Mirror `AnimalRepository.cs` for the new types. Same shape: a static class, an in-memory list, `LoadFromFile`, `SaveToFile`, `Add`, `NextId`, plus a couple of finder methods.

### `AdopterRepository.cs` (sketch)

```csharp
public static class AdopterRepository
{
    public static List<Adopter> adopters = new List<Adopter>();
    private const string FilePath = "adopters.txt";

    public static void Add(Adopter adopter)
    {
        adopter.Id = NextId();
        adopters.Add(adopter);
    }

    private static string NextId()
    {
        if (adopters.Count == 0) return "AD000001";
        int max = adopters
            .Select(a => int.Parse(a.Id.Substring(2)))   // strip the "AD" prefix
            .Max();
        return "AD" + (max + 1).ToString("D6");
    }

    public static void SaveToFile() { /* same shape as AnimalRepository */ }
    public static void LoadFromFile() { /* same shape as AnimalRepository */ }
}
```

The interesting line is `NextId` — same idea as `Animal.NextId`, but the prefix forces a substring extraction before parsing. If you skip that and try `int.Parse("AD000001")`, you get a `FormatException`. The prefix is decorative; the parseable digits are at index 2 onward.

### `AdoptionRepository.cs` (sketch)

```csharp
public static class AdoptionRepository
{
    public static List<Adoption> adoptions = new List<Adoption>();
    private const string FilePath = "adoptions.txt";

    public static void Add(Adoption adoption)
    {
        adoption.Id = NextId();
        adoptions.Add(adoption);
    }

    private static string NextId()
    {
        if (adoptions.Count == 0) return "AP000001";
        int max = adoptions.Select(a => int.Parse(a.Id.Substring(2))).Max();
        return "AP" + (max + 1).ToString("D6");
    }

    public static void SaveToFile() { /* StreamWriter loop */ }
    public static void LoadFromFile() { /* StreamReader loop */ }

    // Domain queries that earn their own method:
    public static Adoption GetActiveAdoptionForAnimal(string animalId)
    {
        return adoptions.FirstOrDefault(
            a => a.AnimalId == animalId && a.Status == "Active");
    }

    public static List<Adoption> GetAdoptionsForAdopter(string adopterId)
    {
        return adoptions.Where(a => a.AdopterId == adopterId).ToList();
    }
}
```

Two finder methods that each correspond to a real screen workflow:

- `GetActiveAdoptionForAnimal` — used when the user wants to "return" an animal, or when listing animals to adopt and you need to know which ones are already taken.
- `GetAdoptionsForAdopter` — for showing one adopter's history.

> **Why these earn their own methods (when the animal `Search` doesn't have its own).** Because they encode domain rules. "Active adoption" is a *concept* the rest of the app has to agree on (`Status == "Active"` is the rule). Hiding that inside a method named `GetActiveAdoptionForAnimal` means if the rule changes (e.g. you add a `"Pending"` status), you update one place. Free-text search across animal names doesn't have that property — it's just a query, not a domain concept.

---

## Part 9d — Update `Main` to load all three files

Three `LoadFromFile` calls now, three save files on disk. Order doesn't matter at startup, but it's nice to keep them grouped:

```csharp
static void Main(string[] args)
{
    ConsoleUI.SetupConsole();
    AnimalRepository.LoadFromFile();
    AdopterRepository.LoadFromFile();
    AdoptionRepository.LoadFromFile();

    while (true)
    {
        // ... menu loop unchanged
    }
}
```

Same try/catch pattern protects all three loads — if `adoptions.txt` has been hand-edited badly, the menu loop's catch handles it.

---

## Part 9e — Add Adopter screen (the easy one)

Mirrors `AddAnimal`. New file, new prompts, save.

```csharp
private static void AddAdopter()
{
    ConsoleUI.DrawHeaders("Add Adopter");

    Adopter adopter = new Adopter();
    adopter.Name = ConsoleUI.Prompt("Name: ");
    adopter.Email = ConsoleUI.Prompt("Email: ");
    adopter.Phone = ConsoleUI.Prompt("Phone: ");
    adopter.Address = ConsoleUI.Prompt("Address: ");

    AdopterRepository.Add(adopter);
    AdopterRepository.SaveToFile();

    ConsoleUI.WriteLineColor("Adopter added.", ConsoleColor.Green);
    ConsoleUI.Pause();
}
```

Same shape as `AddAnimal`, with five free-text prompts and no validation. You could add email-format validation later (`Where(s => s.Contains("@"))` plus a loop), but it's optional for a teaching reference.

---

## Part 9f — Record Adoption screen (the linking one)

This is the headline screen of the relational extension. It collects an animal ID and an adopter ID, looks up both records to confirm they exist, and writes a new adoption row.

```csharp
private static void RecordAdoption()
{
    ConsoleUI.DrawHeaders("Record Adoption");

    // Show available animals (those without an active adoption).
    string[] animalHeaders = { "ID", "Name", "Species", "Breed" };
    List<string[]> animalRows = new List<string[]>();
    foreach (Animal a in AnimalRepository.animals)
    {
        // Skip animals already adopted.
        if (AdoptionRepository.GetActiveAdoptionForAnimal(a.Id) != null) continue;
        animalRows.Add(new string[] { a.Id, a.Name, a.Species, a.Breed });
    }
    ConsoleUI.WriteLineColor("Available animals:", ConsoleColor.Cyan);
    ConsoleUI.PrintTable(animalHeaders, animalRows);

    // Show all adopters.
    string[] adopterHeaders = { "ID", "Name", "Email" };
    List<string[]> adopterRows = new List<string[]>();
    foreach (Adopter ad in AdopterRepository.adopters)
    {
        adopterRows.Add(new string[] { ad.Id, ad.Name, ad.Email });
    }
    ConsoleUI.WriteLineColor("\nRegistered adopters:", ConsoleColor.Cyan);
    ConsoleUI.PrintTable(adopterHeaders, adopterRows);

    // Collect the IDs.
    string animalId = ConsoleUI.Prompt("\nAnimal ID to adopt out: ").PadLeft(8, '0');
    Animal animal = AnimalRepository.animals.FirstOrDefault(a => a.Id == animalId);
    if (animal == null)
    {
        ConsoleUI.WriteLineColor("No such animal.", ConsoleColor.Red);
        ConsoleUI.Pause();
        return;
    }
    if (AdoptionRepository.GetActiveAdoptionForAnimal(animalId) != null)
    {
        ConsoleUI.WriteLineColor("That animal is already adopted.", ConsoleColor.Red);
        ConsoleUI.Pause();
        return;
    }

    string adopterId = ConsoleUI.Prompt("Adopter ID: ").ToUpper();
    Adopter adopter = AdopterRepository.adopters.FirstOrDefault(a => a.Id == adopterId);
    if (adopter == null)
    {
        ConsoleUI.WriteLineColor("No such adopter.", ConsoleColor.Red);
        ConsoleUI.Pause();
        return;
    }

    // Build and save the adoption record.
    Adoption adoption = new Adoption
    {
        AnimalId = animalId,
        AdopterId = adopterId,
        AdoptionDate = DateTime.Today,
        Status = "Active"
    };
    AdoptionRepository.Add(adoption);
    AdoptionRepository.SaveToFile();

    ConsoleUI.WriteLineColor(
        $"\n{animal.Name} adopted by {adopter.Name} on {adoption.AdoptionDate.ToShortDateString()}.",
        ConsoleColor.Green);
    ConsoleUI.Pause();
}
```

What this screen demonstrates:

1. **Cross-table query.** `GetActiveAdoptionForAnimal` looks across two lists: it asks the adoption repository whether the animal is adopted. The animal repository never sees the adoption table — and vice versa. Each repository minds its own list.
2. **Validation by lookup.** "No such animal" and "no such adopter" are validation errors, but they're enforced by *failing to find* the record, not by hardcoded rules. As more adopters and animals get added, the validation automatically expands.
3. **Domain status check.** Even if both IDs exist, the adoption can still fail if the animal is already taken. The status check (`GetActiveAdoptionForAnimal != null`) is a business rule expressed in code.

---

## Part 9g — Display Adoptions (the join)

The most LINQ-interesting screen. For each adoption row, look up the animal and adopter to show their names — not their IDs.

```csharp
private static void DisplayAdoptions()
{
    ConsoleUI.DrawHeaders("Adoptions");

    string[] headers = { "Adoption ID", "Animal", "Species", "Adopter", "Date", "Status" };
    List<string[]> rows = new List<string[]>();

    // For each adoption row, look up the animal and adopter.
    foreach (Adoption adoption in AdoptionRepository.adoptions.OrderByDescending(a => a.AdoptionDate))
    {
        Animal animal = AnimalRepository.animals.FirstOrDefault(a => a.Id == adoption.AnimalId);
        Adopter adopter = AdopterRepository.adopters.FirstOrDefault(a => a.Id == adoption.AdopterId);

        // Defensive — if a referenced ID has been removed, show a placeholder.
        rows.Add(new string[]
        {
            adoption.Id,
            animal != null ? animal.Name : "(missing)",
            animal != null ? animal.Species : "—",
            adopter != null ? adopter.Name : "(missing)",
            adoption.AdoptionDate.ToShortDateString(),
            adoption.Status
        });
    }

    ConsoleUI.PrintTable(headers, rows);
    ConsoleUI.Pause();
}
```

This is **manual join via LINQ `FirstOrDefault`** — the simplest way to do a join. For each row in the adoption table, look up the related animal and adopter by ID. Slow for large datasets (it's O(adoptions × animals)), but perfectly fine for a hundred-row teaching app and dramatically more readable than the formal `Join` operator.

> **Could you use LINQ's `Join` operator instead?** Yes — `adoptions.Join(animals, a => a.AnimalId, an => an.Id, (a, an) => new { ... })`. It's faster but harder to read. For this project, the `FirstOrDefault`-per-row pattern wins on clarity. If the student goes on to a database course, `Join` becomes natural; for now, save it for later.

> **Why the "missing" placeholder?** Because adoption rows can outlive the records they reference. If you delete an animal that has a historical adoption row, the `FirstOrDefault` returns null. Showing `"(missing)"` instead of crashing the screen is the friendly thing to do. In a real database, foreign-key constraints would prevent the deletion in the first place — but our flat-file storage doesn't enforce that, so the screen has to.

---

## Part 9h — Return Adoption (the status flip)

Returning an animal isn't deletion — it's a status update on the adoption row.

```csharp
private static void ReturnAdoption()
{
    ConsoleUI.DrawHeaders("Return Adoption");

    string animalId = ConsoleUI.Prompt("Animal ID being returned: ").PadLeft(8, '0');
    Adoption active = AdoptionRepository.GetActiveAdoptionForAnimal(animalId);
    if (active == null)
    {
        ConsoleUI.WriteLineColor("That animal is not currently adopted.", ConsoleColor.Red);
        ConsoleUI.Pause();
        return;
    }

    active.Status = "Returned";
    AdoptionRepository.SaveToFile();

    ConsoleUI.WriteLineColor("Adoption marked Returned.", ConsoleColor.Green);
    ConsoleUI.Pause();
}
```

The historical record (`AnimalId`, `AdopterId`, `AdoptionDate`) stays intact. Only `Status` flips. If the same animal is adopted again later, a *new* row is added — you can see the full history of an animal's adoptions over time.

---

## Part 9i — Wire everything into the menu

Three new menu options: "Add Adopter," "Record Adoption," "Display Adoptions," "Return Adoption." Reorganize the main menu however you like — a sub-menu ("People & Adoptions") might be clearer than dumping everything into one flat list.

---

## Part 9j — Challenges

13. **Adoption history for one animal.** Prompt for an animal ID, show every adoption row that references it (Active and Returned), ordered by date. Multiple rows per animal = animal was returned and re-adopted.

14. **Most adopted species.** `GroupBy` on adoption rows joined with their animals, count per species, print the winner.

15. **Adopters who took multiple animals.** `GroupBy(adoption.AdopterId)`, filter to groups with `Count() > 1`, look up the adopter's name for each.

16. **Average days an animal stayed in the shelter before adoption.** For each `Active` adoption, compute `(adoption.AdoptionDate - animal.Birthday).Days` (if you treat Birthday as intake date — adapt as needed), then `Average()`.

17. **Find lonely adopters.** List adopters who haven't adopted anything yet — `Where(adopter ⇒ !adoptions.Any(a => a.AdopterId == adopter.Id))`.

18. **Adoption volume per month.** `GroupBy(a => a.AdoptionDate.ToString("yyyy-MM"))`, count per group, render as a bar-chart-ish text output.

---

## Wrap-up

This part doesn't ship as part of the comp deliverable. It's an extension exercise that teaches:

- **Foreign keys via plain strings.** No fancy database — just IDs that point across files.
- **Multi-file persistence.** Three saves, three loads, three lists. Same `StreamReader`/`StreamWriter` pattern, repeated.
- **Manual joins with `FirstOrDefault`.** The simplest, most readable way to relate two tables in LINQ.
- **Status fields vs. deletion.** "Returned" is a state, not a missing record. History matters.
- **Validation via lookup.** Existence is enforced by failing to find, not by hardcoded rules.

If the student wants to keep going beyond Skills Ontario, this is the natural launching pad for either a database course (where the same model maps to SQL tables 1:1) or a small business application (where the same pattern handles inventory + customers + orders, employees + departments + projects, students + courses + enrollments — three lists linked by IDs is the shape of half the line-of-business apps in the world).
