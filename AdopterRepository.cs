// =============================================================================
// AdopterRepository.cs
// Animal Rescue Management System — Skills Ontario (Reference Solution)
// Owns the in-memory list of every adopter, plus file persistence. Mirrors
// AnimalRepository — same shape, different list type and ID prefix.
// Reference: workbook/09-adopters-and-adoptions.md
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SkillsOntarioSampleProject2026
{
	public static class AdopterRepository
	{
		// The single in-memory list of every adopter. Public so screens in
		// Program.cs can iterate it directly — same convention as AnimalRepository.
		public static List<Adopter> adopters = new List<Adopter>();

		// Save file sits next to the running .exe.
		private const string FilePath = "adopters.txt";

		// Assigns a fresh Id via NextId, then appends the adopter to the list.
		public static void Add(Adopter adopter)
		{
			adopter.Id = NextId();
			adopters.Add(adopter);
		}

		// Returns the next available "AD" + 6-digit zero-padded Id by stripping
		// the prefix from existing IDs, finding the max, and adding 1. The "AD"
		// prefix is human-readable convention; the parseable digits start at
		// index 2 (after the two-letter prefix).
		private static string NextId()
		{
			if (adopters.Count == 0) return "AD000001";
			int max = adopters.Select(a => int.Parse(a.Id.Substring(2))).Max();
			return "AD" + (max + 1).ToString("D6");
		}

		// Writes every adopter to adopters.txt, one pipe-delimited line each.
		public static void SaveToFile()
		{
			using (StreamWriter writer = new StreamWriter(FilePath))
			{
				foreach (var adopter in adopters)
				{
					writer.WriteLine(adopter.ToLine());
				}
			}
		}

		// Replaces the in-memory list with the contents of adopters.txt.
		// Missing file is normal on first run — treated as "empty list."
		public static void LoadFromFile()
		{
			if (!File.Exists(FilePath)) return;

			adopters.Clear();
			using (StreamReader reader = new StreamReader(FilePath))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					adopters.Add(Adopter.FromLine(line));
				}
			}
		}
	}
}
