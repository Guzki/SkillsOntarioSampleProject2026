using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillsOntarioSampleProject2026
{
	public class Program
	{
		static void Main(string[] args)
		{
			AnimalRepository.LoadFromFile();

			while (true)
			{
				Console.WriteLine("Welcome to the Animal Shelter Management System");
				Console.WriteLine("1. Add a new animal");
				Console.WriteLine("2. View all animals");
				Console.WriteLine("3. Search for an animal");
				Console.WriteLine("4. Update an animal's information");
				Console.WriteLine("5. Delete an animal");
				Console.WriteLine("6. Exit");


				var choice = Console.ReadLine();
				Console.Clear();
				switch (choice)
				{
					case "1":
						AddAnimal();
						break;
					case "2":
						ViewAllAnimals();
						break;
					case "3":
						SearchAnimal();
						break;
					case "4":
						UpdateAnimal();
						break;
					case "5":
						DeleteAnimal();
						break;
					case "6":
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

		private static void AddAnimal()
		{
			Animal newAnimal = new Animal();
			Console.WriteLine("Adding a new animal...");
			Console.WriteLine("Enter name: ");
			newAnimal.Name = Console.ReadLine();

			Console.WriteLine("Enter Species: ");
			while (true)
			{
				string species = Console.ReadLine();
				if (species != "Dog" && species != "Cat")
				{
					Console.Write("Invalid input. Please enter 'Dog' or 'Cat': ");
				}
				else
				{
					newAnimal.Species = species;
					break;
				}
			}

			Console.WriteLine("Enter Birthday (yyyy-mm-dd): ");
			DateTime birthday;
			while (!DateTime.TryParse(Console.ReadLine(), out birthday))
			{
				Console.Write("Invalid date format. Please enter the birthday (yyyy-mm-dd): ");
			}
			newAnimal.Birthday = birthday;
			Console.WriteLine("Enter Adoption Fee: ");
			decimal adoptionFee;
			while (!decimal.TryParse(Console.ReadLine(), out adoptionFee))
			{
				Console.Write("Invalid input. Please enter a valid adoption fee: ");
			}
			newAnimal.AdoptionFee = adoptionFee;


			AnimalRepository.AddAnimal(newAnimal);
			AnimalRepository.SaveToFile();
			Console.WriteLine("Animal added successfully!");
		}

		private static void ViewAllAnimals()
		{
			Console.WriteLine("Viewing all animals...");
			foreach(var animal in AnimalRepository.animals)
			{
				Console.WriteLine($"Name: {animal.Name}, Species: {animal.Species}");
			}
		}


		private static void SearchAnimal()
		{
			Console.WriteLine("Searching for an animal...");
			// Implementation for searching for an animal
		}


		private static void UpdateAnimal()
		{
			Console.WriteLine("Updating an animal's information...");
			// Implementation for updating an animal's information
		}

		private static void DeleteAnimal()
		{
			Console.WriteLine("Deleting an animal...");
			// Implementation for deleting an animal
		}

	}

}
