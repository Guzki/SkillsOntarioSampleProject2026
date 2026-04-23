// =============================================================================
// AnimalRepository.cs
// Animal Rescue Management System — Skills Ontario (Reference Solution)
// Owns the in-memory list of every animal in the shelter, plus file persistence.
// Static class: there is only ever one shelter, so there is no reason to create
// instances. The list lives for the whole run of the program.
// Reference: workbook/03-repository.md
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillsOntarioSampleProject2026
{
	public static class AnimalRepository
	{
		// The single in-memory list of every animal. Public so screens in
		// Program.cs can iterate it directly — there's only one caller.
		public static List<Animal> animals = new List<Animal>();

		// Save file sits next to the running .exe (relative path).
		private const string FilePath = "animals.txt";

		// Assigns a fresh Id via NextId, then appends the animal to the list.
		// Callers never set animal.Id themselves — the repository owns identity.
		public static void AddAnimal(Animal animal)
		{
			animal.Id = NextId();
			animals.Add(animal);
		}

		// Returns the next available 8-digit zero-padded Id by scanning the list
		// for the highest existing Id and adding 1. No counter state to keep in sync.
		private static string NextId()
		{
			if (animals.Count == 0)
				return "00000001";
			var lastId = animals.Max(a => int.Parse(a.Id));
			return (lastId + 1).ToString("D8");
		}

		// Writes every animal to animals.txt, one pipe-delimited line per animal.
		// Overwrite mode (default) — the in-memory list is the source of truth.
		public static void SaveToFile()
		{
			using (StreamWriter writer = new StreamWriter(FilePath))
			{
				foreach (var animal in animals)
				{
					writer.WriteLine(animal.ToLine());
				}
			}
		}

		// Replaces the in-memory list with the contents of animals.txt.
		// Missing file is normal on first run — treated as "empty list", not an error.
		public static void LoadFromFile()
		{
			if (!File.Exists(FilePath)) return;

			animals.Clear();
			using (StreamReader reader = new StreamReader(FilePath))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					var animal = Animal.FromLine(line);
					animals.Add(animal);
				}
			}
		}
	}
}
