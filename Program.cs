// =============================================================================
// Program.cs
// Animal Rescue Management System — Skills Ontario (Reference Solution)
// Application entry point. Owns the main menu loop and one method per menu
// action — Add, Remove, Search, Display All, Three Oldest, Help, Exit. Every
// screen calls into ConsoleUI for prompts and rendering, and into
// AnimalRepository for data. The menu loop wraps each action in try/catch so
// the app survives any unexpected exception (file corruption, malformed
// input, etc.) without crashing.
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
							ShowHelp();
							break;
						case "7":
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
			Console.WriteLine("  6. Help — this screen.");
			Console.WriteLine("  7. Exit — closes the program.");
			Console.WriteLine();
			ConsoleUI.WriteLineColor("Data file:", ConsoleColor.Cyan);
			Console.WriteLine("  Animals are stored in animals.txt next to this .exe.");
			Console.WriteLine("  The save file is plain text, pipe-delimited (one animal per line).");
			Console.WriteLine();
			ConsoleUI.WriteLineColor("Date format:", ConsoleColor.Cyan);
			Console.WriteLine("  Birthdays use this machine's short-date pattern.");
			Console.WriteLine("  Copying animals.txt to a machine with a different locale may not load cleanly.");

			ConsoleUI.Pause();
		}

	}

}
