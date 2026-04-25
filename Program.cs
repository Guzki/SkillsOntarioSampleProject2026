// =============================================================================
// Program.cs
// Animal Rescue Management System — Skills Ontario (Reference Solution)
// Application entry point. Owns the main menu loop and one method per menu
// action. Every screen calls into ConsoleUI for prompts and rendering, and
// into the appropriate repository for data. The menu loop wraps each action
// in try/catch so the app survives any unexpected exception (file corruption,
// malformed input, etc.) without crashing.
//
// Top-level menu (the base Skills Ontario tier):
//   Add Animal · Remove Animal · Search · Display All Sorted By Species ·
//   Three Oldest Per Species · Help · Exit
//
// People & Adoptions sub-menu (Part 9 — the relational extension):
//   Add Adopter · List Adopters · Record Adoption · List Adoptions ·
//   Return Adoption
//
// Reference: workbook/04-add-screen.md and onwards
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillsOntarioSampleProject2026
{
	public class Program
	{
		// Entry point. Configures the console, loads any saved animals from disk,
		// then runs the menu loop forever until the user chooses Exit (case "7").
		// Every menu action is wrapped in try/catch so the app never crashes —
		// the catch reports the error in red and pauses, then the loop continues.
		static void Main(string[] args)
		{
			ConsoleUI.SetupConsole();

			// Three repositories, three save files. Each loads independently;
			// missing files are normal on first run.
			AnimalRepository.LoadFromFile();
			AdopterRepository.LoadFromFile();
			AdoptionRepository.LoadFromFile();

			while (true)
			{
				ConsoleUI.DrawHeaders("Main Menu");

				Console.WriteLine("Welcome to the Animal Shelter Management System");
				Console.WriteLine("1. Add animal");
				Console.WriteLine("2. Remove animal");
				Console.WriteLine("3. Search");
				Console.WriteLine("4. Display all (sorted by species)");
				Console.WriteLine("5. Display three oldest per species");
				Console.WriteLine("6. People & Adoptions");
				Console.WriteLine("7. Help");
				Console.WriteLine("8. Exit");

				try
				{
					var choice = Console.ReadLine();

					switch (choice)
					{
						case "1":
							AddAnimal();
							break;
						case "2":
							RemoveAnimal();
							break;
						case "3":
							SearchAnimal();
							break;
						case "4":
							DisplayAllSortedBySpecies();
							break;
						case "5":
							DisplayThreeOldestPerSpecies();
							break;
						case "6":
							PeopleAndAdoptionsMenu();
							break;
						case "7":
							ShowHelp();
							break;
						case "8":
							return;
						default:
							ConsoleUI.WriteLineColor("Invalid choice. Please try again.", ConsoleColor.Red);
							ConsoleUI.Pause();
							break;
					}

				}
				catch (Exception ex)
				{
					ConsoleUI.WriteLineColor($"An error occurred: {ex.Message}", ConsoleColor.Red);
					ConsoleUI.Pause();
				}
			}
		}

		// Sub-menu for the relational extension (Part 9). Loops with its own
		// numbered menu until the user picks "Back to main menu", at which
		// point control returns to the main loop.
		private static void PeopleAndAdoptionsMenu()
		{
			while (true)
			{
				ConsoleUI.DrawHeaders("People & Adoptions");

				Console.WriteLine("1. Add adopter");
				Console.WriteLine("2. List adopters");
				Console.WriteLine("3. Record adoption");
				Console.WriteLine("4. List adoptions");
				Console.WriteLine("5. Return adoption");
				Console.WriteLine("6. Back to main menu");

				try
				{
					var choice = Console.ReadLine();

					switch (choice)
					{
						case "1":
							AddAdopter();
							break;
						case "2":
							ListAdopters();
							break;
						case "3":
							RecordAdoption();
							break;
						case "4":
							DisplayAdoptions();
							break;
						case "5":
							ReturnAdoption();
							break;
						case "6":
							return;
						default:
							ConsoleUI.WriteLineColor("Invalid choice. Please try again.", ConsoleColor.Red);
							ConsoleUI.Pause();
							break;
					}
				}
				catch (Exception ex)
				{
					ConsoleUI.WriteLineColor($"An error occurred: {ex.Message}", ConsoleColor.Red);
					ConsoleUI.Pause();
				}
			}
		}

		// Prompts the user for the core Animal fields, validates each, then
		// adds the new animal to the repository and saves to disk.
		// "Mutate then save" is two explicit calls — file I/O is never hidden.
		private static void AddAnimal()
		{
			ConsoleUI.DrawHeaders("Add Animal");

			Animal newAnimal = new Animal();
			newAnimal.Name = ConsoleUI.Prompt("Enter Name: ");
			newAnimal.Species = ConsoleUI.PromptChoice("Select Species: ", new string[] { "Dog", "Cat", "Bird", "Rabbit", "Small & Furry", "Fish", "Barnyard", "Other" });
			newAnimal.Breed = ConsoleUI.Prompt("Enter Breed: ");
			newAnimal.Colour = ConsoleUI.Prompt("Enter Colour: ");
			newAnimal.Birthday = ConsoleUI.PromptDateTime("Enter Birthday (dd/MM/yyyy): ");
			newAnimal.Gender = ConsoleUI.PromptChoice("Select Gender: ", new string[] { "Male", "Female" });
			newAnimal.SpayedOrNeutered = ConsoleUI.PromptChoice("Is the animal spayed or neutered? ", new string[] { "Yes", "No" });
			newAnimal.VaccineStatus = ConsoleUI.PromptChoice("Vaccine status: ", new string[] { "Up to date", "Late", "Unknown" });
			newAnimal.AdoptionFee = ConsoleUI.PromptDecimal("Enter Adoption Fee (< 300): ", 300m);
			newAnimal.IdentificationType = ConsoleUI.PromptChoice("Select Identification Type: ", new string[] { "Bar code", "Microchip", "None" });
			newAnimal.IdentificationNumber = ConsoleUI.Prompt("Enter Identification Number: ");

			AnimalRepository.AddAnimal(newAnimal);
			AnimalRepository.SaveToFile();

			ConsoleUI.WriteLineColor("Animal added successfully!", ConsoleColor.Green);
			ConsoleUI.Pause();
		}

		// Lists every animal in an aligned table, sorted by species and then by
		// name within each species. The user can optionally filter to a single
		// species via PromptChoice (or pick "All" for the full list). Shows a
		// headline subset of six columns; full 12-field detail belongs on a
		// future single-animal detail screen, not here.
		private static void DisplayAllSortedBySpecies()
		{
			ConsoleUI.DrawHeaders("All animals — sorted by species");

			// Filter the user's choice through PromptChoice — the "All" option
			// at the end of the list lets them skip the filter without leaving
			// the screen.
			string specie = ConsoleUI.PromptChoice("Filter by species?", new string[] { "Dog", "Cat", "Bird", "Rabbit", "Small & Furry", "Fish", "Barnyard", "Other", "All" });

			// LINQ chain: Where applies the filter (no-op when specie == "All"),
			// OrderBy sorts by species, ThenBy breaks ties by name. Lazy — the
			// query doesn't actually run until the foreach below iterates it.
			var sortedAnimals = AnimalRepository.animals
				.Where(a => specie == "All" || a.Species == specie)
				.OrderBy(a => a.Species)
				.ThenBy(a => a.Name);

			// Redraw the header — the species-filter PromptChoice above has scrolled
			// the screen and pushed the original header out of view. Redrawing here
			// gives the table a clean canvas to render onto.
			ConsoleUI.DrawHeaders("All animals — sorted by species");

			// 1. Headers for this screen (six columns — narrow enough to fit a standard console).
			string[] headers = { "ID", "Name", "Species", "Breed", "Colour", "Birthday" };

			// 2. Empty list to hold the rows we build.
			List<string[]> rows = new List<string[]>();

			// 3. One row per animal, fields in the same order as the headers above.
			foreach (var animal in sortedAnimals)
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

			// 4. Hand off to the generic helper.
			ConsoleUI.PrintTable(headers, rows);

			ConsoleUI.Pause();
		}

		// Shows a quick three-column reference table of every animal so the user
		// can see what IDs exist, then loops asking for the ID of the one to
		// remove. Accepts either a full 8-digit ID ("00000005") or a shorthand
		// ("5") — PadLeft pads with zeros up to 8 characters. Removes from the
		// in-memory list and saves to disk on success; reprompts in red on any
		// non-matching input.
		private static void RemoveAnimal()
		{
			ConsoleUI.DrawHeaders("Remove Animal");

			// Display a three-column "what IDs are available?" table so the
			// user doesn't have to remember IDs from a previous Display All.
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

			// Loop until the user enters an ID that matches an existing animal.
			// PadLeft means "5" and "00000005" both find the same animal.
			while (true)
			{
				string id = ConsoleUI.Prompt("Enter the ID of the animal to remove: ").PadLeft(8, '0');
				// FirstOrDefault returns the first match, or null if no match —
				// this is the LINQ shape for "look up something that may not exist."
				var animalToRemove = AnimalRepository.animals.FirstOrDefault(a => a.Id == id);
				if (animalToRemove != null)
				{
					// Mutate then save — two explicit calls. File I/O is never
					// hidden inside the mutation; the student sees both steps.
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

		// Prompts for a single search term and shows every animal whose Name
		// OR Species contains the term (case-insensitive, partial substring).
		// Empty input matches everything, so a blank Enter doubles as
		// "show all sorted." Results render in the same six-column table as
		// DisplayAllSortedBySpecies for visual consistency.
		private static void SearchAnimal()
		{
			ConsoleUI.DrawHeaders("Search Animals");

			// ToLower on input — paired with ToLower on each field below — gives
			// us case-insensitive substring search without having to think about
			// CultureInfo.InvariantCulture or StringComparison.OrdinalIgnoreCase.
			string searchTerm = ConsoleUI.Prompt("Enter a search term (name or species, leave blank to show all): ").ToLower();

			// LINQ Where with three OR'd conditions: empty term matches all,
			// name match, species match. The empty-term shortcut lets this
			// screen double as "display everything sorted."
			var searchResults = AnimalRepository.animals
				.Where(a => string.IsNullOrEmpty(searchTerm) ||
							a.Name.ToLower().Contains(searchTerm) ||
							a.Species.ToLower().Contains(searchTerm))
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

		// For each species in the shelter, shows the three oldest animals of that
		// species. Built in two LINQ steps with a foreach in between, instead of one
		// long chain — easier to read for a beginner. Step 1: GroupBy splits the
		// flat list into per-species groups. Step 2: for each group, OrderBy +
		// Take(3) picks the three oldest, and the outer foreach naturally flattens
		// each group's rows into the combined table — no SelectMany needed.
		private static void DisplayThreeOldestPerSpecies()
		{
			ConsoleUI.DrawHeaders("Three Oldest Animals Per Species");

			string[] headers = { "ID", "Name", "Species", "Breed", "Colour", "Birthday" };
			List<string[]> rows = new List<string[]>();

			// Step 1: group every animal by its species. Result is a sequence of
			// groups; each group has a Key (the species name) and contains the
			// animals of that species.
			var groupedBySpecies = AnimalRepository.animals.GroupBy(a => a.Species);

			// Step 2: walk each group, sort by Birthday ascending (earliest birthday
			// = oldest animal), keep the first three, and add their rows. The outer
			// foreach handles the "flatten groups back to one list" step.
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

		// Prints a help screen summarising the menu, where the save file lives,
		// and the date-format / single-machine assumption. Uses the same
		// ConsoleUI palette as every other screen — branded header, cyan
		// section labels, default-color body text, ends with Pause.
		private static void ShowHelp()
		{
			ConsoleUI.DrawHeaders("Help");

			ConsoleUI.WriteLineColor("Animal Rescue Management System — Help", ConsoleColor.Cyan);
			Console.WriteLine();
			ConsoleUI.WriteLineColor("Menu options:", ConsoleColor.Cyan);
			Console.WriteLine("  1. Add animal — prompts for details and saves a new animal.");
			Console.WriteLine("  2. Remove animal — deletes an animal by its ID.");
			Console.WriteLine("  3. Search — finds animals by name or species (substring match).");
			Console.WriteLine("  4. Display all (sorted by species) — lists every animal, optionally filtered.");
			Console.WriteLine("  5. Display three oldest per species — three oldest within each species.");
			Console.WriteLine("  6. People & Adoptions — sub-menu for adopters and adoption records.");
			Console.WriteLine("  7. Help — this screen.");
			Console.WriteLine("  8. Exit — closes the program.");
			Console.WriteLine();
			ConsoleUI.WriteLineColor("Data files:", ConsoleColor.Cyan);
			Console.WriteLine("  animals.txt    — every animal in the shelter.");
			Console.WriteLine("  adopters.txt   — registered adopters (Part 9 extension).");
			Console.WriteLine("  adoptions.txt  — link rows pairing animals with adopters.");
			Console.WriteLine("  All three are plain text, pipe-delimited, one row per line.");
			Console.WriteLine();
			ConsoleUI.WriteLineColor("Date format:", ConsoleColor.Cyan);
			Console.WriteLine("  Birthdays and adoption dates use this machine's short-date pattern.");
			Console.WriteLine("  Copying the save files to a machine with a different locale may not load cleanly.");

			ConsoleUI.Pause();
		}


		// =====================================================================
		// People & Adoptions screens (Part 9 — relational extension).
		// =====================================================================

		// Prompts for the 4 contact fields and saves a new adopter. Same shape
		// as AddAnimal but with no PromptChoice — every field is free text.
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

			ConsoleUI.WriteLineColor($"Adopter added with ID {adopter.Id}.", ConsoleColor.Green);
			ConsoleUI.Pause();
		}

		// Lists every registered adopter in a five-column table. Pattern C
		// from Part 5g, applied to the Adopter list instead of Animal.
		private static void ListAdopters()
		{
			ConsoleUI.DrawHeaders("Adopters");

			string[] headers = { "ID", "Name", "Email", "Phone", "Address" };
			List<string[]> rows = new List<string[]>();
			foreach (Adopter adopter in AdopterRepository.adopters.OrderBy(a => a.Name))
			{
				rows.Add(new string[]
				{
					adopter.Id,
					adopter.Name,
					adopter.Email,
					adopter.Phone,
					adopter.Address
				});
			}

			ConsoleUI.PrintTable(headers, rows);
			ConsoleUI.Pause();
		}

		// Pairs an animal with an adopter and saves the link as a new adoption
		// row. Shows available animals (those not currently adopted) and all
		// adopters first so the user can see the IDs they're about to enter.
		// Validates that both IDs exist and that the animal isn't already
		// adopted before recording the row.
		private static void RecordAdoption()
		{
			ConsoleUI.DrawHeaders("Record Adoption");

			// Show available animals — anything without an Active adoption row.
			ConsoleUI.WriteLineColor("Available animals:", ConsoleColor.Cyan);
			string[] animalHeaders = { "ID", "Name", "Species", "Breed" };
			List<string[]> animalRows = new List<string[]>();
			foreach (Animal a in AnimalRepository.animals.OrderBy(a => a.Species).ThenBy(a => a.Name))
			{
				if (AdoptionRepository.GetActiveAdoptionForAnimal(a.Id) != null) continue;
				animalRows.Add(new string[] { a.Id, a.Name, a.Species, a.Breed });
			}
			ConsoleUI.PrintTable(animalHeaders, animalRows);

			// Show all adopters.
			Console.WriteLine();
			ConsoleUI.WriteLineColor("Registered adopters:", ConsoleColor.Cyan);
			string[] adopterHeaders = { "ID", "Name", "Email" };
			List<string[]> adopterRows = new List<string[]>();
			foreach (Adopter ad in AdopterRepository.adopters.OrderBy(a => a.Name))
			{
				adopterRows.Add(new string[] { ad.Id, ad.Name, ad.Email });
			}
			ConsoleUI.PrintTable(adopterHeaders, adopterRows);

			// Collect the animal ID. PadLeft with '0' lets the user type "5"
			// for animal "00000005" — same shorthand as RemoveAnimal.
			Console.WriteLine();
			string animalId = ConsoleUI.Prompt("Animal ID to adopt out: ").PadLeft(8, '0');
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

			// Collect the adopter ID. ToUpper because the prefix is uppercase.
			string adopterId = ConsoleUI.Prompt("Adopter ID (e.g. AD000001): ").ToUpper();
			Adopter adopter = AdopterRepository.adopters.FirstOrDefault(a => a.Id == adopterId);
			if (adopter == null)
			{
				ConsoleUI.WriteLineColor("No such adopter.", ConsoleColor.Red);
				ConsoleUI.Pause();
				return;
			}

			// Build and save the adoption row. AdoptionDate defaults to today;
			// you could prompt for a custom date if needed.
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

		// Lists every adoption row with the animal's name and the adopter's
		// name resolved by ID lookup. This is a manual LINQ join — for each
		// adoption row, FirstOrDefault into the animals and adopters lists to
		// pull the human-readable names. If a referenced ID has been removed,
		// shows "(missing)" instead of crashing.
		private static void DisplayAdoptions()
		{
			ConsoleUI.DrawHeaders("Adoptions");

			string[] headers = { "Adoption ID", "Animal", "Species", "Adopter", "Date", "Status" };
			List<string[]> rows = new List<string[]>();

			// Newest adoptions first.
			foreach (Adoption adoption in AdoptionRepository.adoptions.OrderByDescending(a => a.AdoptionDate))
			{
				// Manual join: look up the animal and adopter by their IDs.
				Animal animal = AnimalRepository.animals.FirstOrDefault(a => a.Id == adoption.AnimalId);
				Adopter adopter = AdopterRepository.adopters.FirstOrDefault(a => a.Id == adoption.AdopterId);

				// Defensive — adoption rows can outlive the records they point at
				// if an animal or adopter was removed. Show a placeholder rather
				// than crash.
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

		// Marks an active adoption as Returned. The historical row stays in
		// adoptions.txt — Status flips from "Active" to "Returned" — so an
		// animal that's been adopted, returned, and re-adopted shows three
		// distinct rows in DisplayAdoptions, not one overwritten record.
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

			// Status flip — not a delete. The historical record stays.
			active.Status = "Returned";
			AdoptionRepository.SaveToFile();

			ConsoleUI.WriteLineColor("Adoption marked Returned.", ConsoleColor.Green);
			ConsoleUI.Pause();
		}

	}

}
