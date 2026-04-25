// =============================================================================
// AdoptionRepository.cs
// Animal Rescue Management System — Skills Ontario (Reference Solution)
// Owns the in-memory list of every adoption (link rows between animals and
// adopters), plus file persistence. Adds two finder methods that encode
// domain rules — "is this animal currently adopted?" and "what has this
// adopter taken home?" — so the screens don't have to repeat the LINQ.
// Reference: workbook/09-adopters-and-adoptions.md
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SkillsOntarioSampleProject2026
{
	public static class AdoptionRepository
	{
		// The single in-memory list of every adoption row.
		public static List<Adoption> adoptions = new List<Adoption>();

		private const string FilePath = "adoptions.txt";

		// Assigns a fresh Id via NextId, then appends the adoption to the list.
		public static void Add(Adoption adoption)
		{
			adoption.Id = NextId();
			adoptions.Add(adoption);
		}

		// Returns the next available "AP" + 6-digit zero-padded Id. Same shape
		// as AdopterRepository.NextId — strip prefix, find max, increment.
		private static string NextId()
		{
			if (adoptions.Count == 0) return "AP000001";
			int max = adoptions.Select(a => int.Parse(a.Id.Substring(2))).Max();
			return "AP" + (max + 1).ToString("D6");
		}

		// Writes every adoption to adoptions.txt, one pipe-delimited line each.
		public static void SaveToFile()
		{
			using (StreamWriter writer = new StreamWriter(FilePath))
			{
				foreach (var adoption in adoptions)
				{
					writer.WriteLine(adoption.ToLine());
				}
			}
		}

		// Replaces the in-memory list with the contents of adoptions.txt.
		public static void LoadFromFile()
		{
			if (!File.Exists(FilePath)) return;

			adoptions.Clear();
			using (StreamReader reader = new StreamReader(FilePath))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					adoptions.Add(Adoption.FromLine(line));
				}
			}
		}

		// Returns the active adoption row for the given animal, or null if
		// the animal isn't currently adopted. "Active" status is the rule that
		// distinguishes a current adoption from a returned one — encoding that
		// rule here means screens don't have to remember the magic string.
		public static Adoption GetActiveAdoptionForAnimal(string animalId)
		{
			return adoptions.FirstOrDefault(a => a.AnimalId == animalId && a.Status == "Active");
		}

		// Returns every adoption row (Active or Returned) referencing the given
		// adopter — the adopter's full history with the shelter.
		public static List<Adoption> GetAdoptionsForAdopter(string adopterId)
		{
			return adoptions.Where(a => a.AdopterId == adopterId).ToList();
		}
	}
}
