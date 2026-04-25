// =============================================================================
// Adoption.cs
// Animal Rescue Management System — Skills Ontario (Reference Solution)
// Represents one adoption — the link between an Animal and an Adopter, plus
// when it happened and whether it's still active. AnimalId and AdopterId are
// foreign keys that point into AnimalRepository.animals and
// AdopterRepository.adopters respectively.
// Reference: workbook/09-adopters-and-adoptions.md
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkillsOntarioSampleProject2026
{
	public class Adoption
	{
		// The 5 fields, in the locked order used by ToLine and FromLine.
		public string Id { get; set; }              // "AP" prefix + 6 digits, e.g. "AP000001"
		public string AnimalId { get; set; }        // foreign key → Animal.Id
		public string AdopterId { get; set; }       // foreign key → Adopter.Id
		public DateTime AdoptionDate { get; set; }
		public string Status { get; set; }          // "Active" or "Returned"


		// Serializes this adoption as one pipe-delimited line for the save file.
		// Date uses ToShortDateString to match the same machine-culture pattern
		// used by Animal.cs — single-machine assumption, see CLAUDE.md.
		public string ToLine()
		{
			return $"{Id}|{AnimalId}|{AdopterId}|{AdoptionDate.ToShortDateString()}|{Status}";
		}

		// Parses one pipe-delimited line into a new Adoption.
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
}
