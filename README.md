# Animal Rescue Management System

A small console-based animal shelter management app written in C#, built as a **teaching reference** for the [Skills Ontario Coding](https://www.skillsontario.com/) competition. Pairs with a step-by-step workbook (`workbook/`) that walks a student through the same build, one part at a time.

---

## What this is

This repository serves two audiences:

- **A student preparing for Skills Ontario Coding.** Work through the workbook in order (Parts 0 through 4 so far). Each part teaches one piece of the app and ends with a self-check. By the last part, you will have built a standalone Windows `.exe` that reads and writes a plain-text save file — the exact shape of a Skills Ontario deliverable.
- **Anyone who wants a minimal, readable example of a console CRUD app in C#.** The code deliberately avoids frameworks, NuGet packages, interfaces, and clever abstractions. Just a handful of classes, a flat file, and plain `System.Console`.

## Status

**Base tier complete.** Every menu action works end-to-end:

- `Animal` data class with 12 fields + `ToLine`/`FromLine` round-tripping through `animals.txt`.
- `AnimalRepository` static class — in-memory list, save/load via `StreamReader`/`StreamWriter`, ID generator.
- `ConsoleUI` helper class — colored output, validated prompts (int/double/decimal/decimal-with-max/datetime/choice), branded headers, table renderer.
- **Add animal** — all 12 fields collected via ConsoleUI prompts and `PromptChoice` lookups.
- **Remove animal** — quick reference table + ID lookup with `PadLeft` shorthand.
- **Search animals** — single search term matches name OR species (case-insensitive substring); blank shows all.
- **Display all (sorted by species)** — six headline columns, optional species filter via `PromptChoice`.
- **Display three oldest per species** — LINQ `GroupBy` + `OrderBy` + `Take(3)` with a clean two-step pattern (no `SelectMany`).
- **Help** — branded screen explaining menu, save file, and the single-machine date-format assumption.
- **Crash-resistant menu loop** — every action wrapped in try/catch; the app reports errors in red and keeps running.

**Optional extension** (workbook Part 9): adopters + adoptions tables linked by foreign-key IDs, with manual LINQ joins. Beyond the base scope; not part of the comp deliverable.

---

## Building and running

### Requirements

- Windows with **.NET Framework 4.7.2** or later (every modern Windows machine).
- Visual Studio 2019 or newer (Community Edition is free). During installation, enable the **.NET desktop development** workload.

### Build

1. Open `SkillsOntarioSampleProject2026.sln` in Visual Studio.
2. Press **F5** to build and run in Debug mode.
3. To produce the shippable `.exe`: change the configuration dropdown to **Release** and build with **Ctrl+Shift+B**. The `.exe` lands in `bin\Release\`.

### Run

Run from Visual Studio (F5), or copy the Release `.exe` to any folder and double-click it. The app creates and maintains a plain-text save file named `animals.txt` in the same folder as the running `.exe`.

---

## File format

Every animal occupies one line in `animals.txt`, fields separated by `|`:

```
00000001|Rex|Labrador|Dog|15/06/2019|Yes|M|Brown|Up to date|985141000123456|Microchip|250.00
```

Fields in the locked order:

| # | Field | Type | Notes |
|---|-------|------|-------|
| 1 | Id | string | 8-digit zero-padded, assigned by the repository |
| 2 | Name | string | free text |
| 3 | Breed | string | `unknown` always valid |
| 4 | Species | string | `Dog` / `Cat` / `Bird` / `Rabbit` / `Small & Furry` / `Fish` / `Barnyard` / `Other` |
| 5 | Birthday | `DateTime` | `dd/MM/yyyy` |
| 6 | SpayedOrNeutered | string | `Yes` / `No` |
| 7 | Gender | string | `F` / `M` |
| 8 | Colour | string | free text |
| 9 | VaccineStatus | string | `Up to date` / `Late` / `Unknown` |
| 10 | IdentificationNumber | string | may be empty |
| 11 | IdentificationType | string | `Bar code` / `Microchip` / `None` |
| 12 | AdoptionFee | `decimal` | must be `< 300` |

**This order is locked.** Once any animal has been saved, changing the field order makes every existing `animals.txt` unreadable. If fields ever need to change, the design document (`workbook/00-design.md`) and the code must change together.

---

## Known limitations

- **Single-machine save files.** `DateTime.Parse` and `decimal.Parse` use the current machine culture, and `ToLine` writes via `ToShortDateString()`. The same machine reads what it just wrote, so round-trips are reliable in normal use — but copying `animals.txt` (or `adopters.txt` / `adoptions.txt`) to a machine with a different short-date or decimal pattern may not load cleanly.
- **No undo, no backup.** A `Remove` is immediate and final; the save file is overwritten on every change. Likewise an adoption returned via Part 9 flips status but a deleted adopter or animal cannot be recovered.
- **Breed is free text.** Any string is accepted — there is no closed-list validation.

---

## Repository layout

```
SkillsOntarioSampleProject2026/
├── Animal.cs                         // Animal data class + ToLine / FromLine
├── AnimalRepository.cs               // static in-memory list + file I/O
├── ConsoleUI.cs                      // reusable prompts / display helpers
├── Program.cs                        // Main + menu loop + screen methods
│
├── Adopter.cs                        // (Part 9) Adopter data class
├── AdopterRepository.cs              // (Part 9) adopter list + file I/O
├── Adoption.cs                       // (Part 9) link record (animal ↔ adopter)
├── AdoptionRepository.cs             // (Part 9) adoption list + file I/O + finders
│
├── SkillsOntarioSampleProject2026.sln
├── SkillsOntarioSampleProject2026.csproj
│
├── CLAUDE.md                         // architecture + conventions reference
├── PEDAGOGY.md                       // teaching decisions — what we skip and why
├── README.md                         // this file
│
└── workbook/
    ├── 00-design.md                  // design-before-code on paper
    ├── 01-project-setup.md           // creating the project in Visual Studio
    ├── 02-animal-class.md            // the Animal data class
    ├── 03-repository.md              // static class + in-memory list + file I/O
    ├── 04-add-screen.md              // Add screen + full deployment test
    ├── 05-console-ui.md              // ConsoleUI helpers + AddAnimal retrofit
    ├── 06-remove-and-search.md       // find-and-act screens
    ├── 07-three-oldest-per-species.md // LINQ chapter (GroupBy + challenges)
    ├── 08-polish-and-deliver.md      // Help retrofit + README + zip
    └── 09-adopters-and-adoptions.md  // (beyond scope) relational extension
```

---

## For students — how to use this repo

1. **Start with the workbook.** Read `workbook/00-design.md` before opening Visual Studio. Do the paper exercises.
2. **Work in order.** Every part assumes the one before is done.
3. **Type the code yourself.** The reference `.cs` files show the shape you're aiming for, but the learning happens when you type it, make mistakes, and fix them.
4. **Do the deployment test in Part 4.** That's the one that proves your project is actually shippable. Don't skip it.

---

## About this repository

This is **teaching material**, authored by a high-school teacher for use with one student preparing to compete. AI tools (specifically Claude Code) were used to draft the workbook, write documentation, and review code — this is transparent and intentional. **At the competition itself, Skills Ontario rules prohibit AI assistance**; students must write their own code from scratch on the day. This repository is a study reference, not a submission template.
