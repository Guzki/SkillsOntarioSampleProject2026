// =============================================================================
// Animal.cs
// Animal Rescue Management System — Skills Ontario (Reference Solution)
// Represents one animal in the shelter. Holds the 12 fields below and converts
// to/from a single pipe-delimited line of text for saving to animals.txt.
// NOTE: ToLine and FromLine currently use string interpolation and string.Split.
// A later workbook part rewrites both with a manual StringBuilder character-walk.
// Reference: workbook/02-animal-class.md
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillsOntarioSampleProject2026
{
	public class Animal
	{
		// The 12 fields, in the locked order used by ToLine and FromLine.
		// Once any animal has been saved to disk, this order must NOT change.
		public string Id { get; set; }
		public string Name { get; set; }
		public string Breed { get; set; }
		public string Species { get; set; }
		public DateTime Birthday { get; set; }
		public string SpayedOrNeutered { get; set; }
		public string Gender { get; set; }
		public string Colour { get; set; }
		public string VaccineStatus { get; set; }
		public string IdentificationNumber { get; set; }
		public string IdentificationType { get; set; }
		public decimal AdoptionFee { get; set; }


		// Serializes this animal as one pipe-delimited line for the save file.
		// The field order here MUST match FromLine below.
		public string ToLine()
		{
			return $"{Id}|{Name}|{Breed}|{Species}|{Birthday.ToString("dd/MM/yyyy")}|{SpayedOrNeutered}|{Gender}|{Colour}|{VaccineStatus}|{IdentificationNumber}|{IdentificationType}|{AdoptionFee}";
		}

		// Parses one pipe-delimited line into a new Animal.
		// DateTime.Parse / decimal.Parse use the current machine culture — the app
		// assumes English-locale input (see PEDAGOGY.md for why we skip globalization).
		public static Animal FromLine(string line)
		{
			var parts = line.Split('|');
			return new Animal
			{
				Id = parts[0],
				Name = parts[1],
				Breed = parts[2],
				Species = parts[3],
				Birthday = DateTime.Parse(parts[4]),
				SpayedOrNeutered = parts[5],
				Gender = parts[6],
				Colour = parts[7],
				VaccineStatus = parts[8],
				IdentificationNumber = parts[9],
				IdentificationType = parts[10],
				AdoptionFee = decimal.Parse(parts[11])
				};
		}
	}
}
