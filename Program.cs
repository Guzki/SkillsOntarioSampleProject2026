// =============================================================================
// Program.cs
// Animal Rescue Management System — Skills Ontario (Reference Solution)
// Application entry point. Owns the main menu loop and one method per
// menu action that wires the user interface to AnimalRepository.
// Several screens (Remove, Search, DisplayThreeOldestPerSpecies) are stubs
// and will be fleshed out in upcoming workbook parts.
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
		// Entry point. Loads the saved animals once, then runs the menu loop
		// until the user chooses Exit.
		static void Main(string[] args)
		{
			AnimalRepository.LoadFromFile();

			while (true)
			{
				Console.WriteLine("Welcome to the Animal Shelter Management System");
				Console.WriteLine("1. Add animal");
				Console.WriteLine("2. Remove animal");
				Console.WriteLine("3. Search");
				Console.WriteLine("4. Display all (sorted by species)");
				Console.WriteLine("5. Display three oldest per species");
				Console.WriteLine("6. Help");
				Console.WriteLine("7. Exit");


				var choice = Console.ReadLine();
				Console.Clear();
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
						Console.WriteLine("Invalid choice. Please try again.");
						break;

				}
				Console.WriteLine("\n\n\n\nPress any key to continue...");
				Console.ReadKey();
				Console.Clear();
			}
		}

		// Prompts the user for the core Animal fields, validates each, then
		// adds the new animal to the repository and saves to disk.
		// "Mutate then save" is two explicit calls — file I/O is never hidden.
		private static void AddAnimal()
		{
			Animal newAnimal = new Animal();
			Console.WriteLine("Adding a new animal...");
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


			AnimalRepository.AddAnimal(newAnimal);
			AnimalRepository.SaveToFile();
			Console.WriteLine("Animal added successfully!");
		}

		// Lists every animal, ordered by species.
		private static void DisplayAllSortedBySpecies()
		{
			Console.WriteLine("All animals (sorted by species):");
			foreach (var animal in AnimalRepository.animals.OrderBy(a => a.Species))
			{
				Console.WriteLine($"[{animal.Id}] {animal.Name} — {animal.Species} ({animal.Breed})");
			}
		}

		// Stub — remove an animal by Id. Implemented in a later workbook part.
		private static void RemoveAnimal()
		{
			Console.WriteLine("Remove animal — not yet implemented.");
		}

		// Stub — search by name or species. Implemented in a later workbook part.
		private static void SearchAnimal()
		{
			Console.WriteLine("Search — not yet implemented.");
		}

		// Stub — show the three oldest animals per species.
		// Implemented in a later workbook part.
		private static void DisplayThreeOldestPerSpecies()
		{
			Console.WriteLine("Display three oldest per species — not yet implemented.");
		}

		// Prints a help screen summarising the menu and where the save file lives.
		private static void ShowHelp()
		{
			Console.WriteLine("Animal Rescue Management System — Help");
			Console.WriteLine();
			Console.WriteLine("Menu options:");
			Console.WriteLine("  1. Add animal — prompts for details and saves a new animal.");
			Console.WriteLine("  2. Remove animal — deletes an animal by its Id.");
			Console.WriteLine("  3. Search — finds animals by name or species.");
			Console.WriteLine("  4. Display all (sorted by species) — lists every animal.");
			Console.WriteLine("  5. Display three oldest per species — lists the three oldest within each species.");
			Console.WriteLine("  6. Help — this screen.");
			Console.WriteLine("  7. Exit — closes the program.");
			Console.WriteLine();
			Console.WriteLine("Data is stored in animals.txt next to this .exe.");
		}

	}

}
