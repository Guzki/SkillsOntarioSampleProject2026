// =============================================================================
// Adopter.cs
// Animal Rescue Management System — Skills Ontario (Reference Solution)
// Represents one adopter (a person who can adopt animals). Holds the 5 fields
// below and converts to/from a single pipe-delimited line for adopters.txt.
// Same shape as Animal.cs — properties + ToLine + FromLine.
// Reference: workbook/09-adopters-and-adoptions.md
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkillsOntarioSampleProject2026
{
	public class Adopter
	{
		// The 5 fields, in the locked order used by ToLine and FromLine.
		// Once any adopter has been saved to disk, this order must NOT change.
		public string Id { get; set; }       // "AD" prefix + 6 digits, e.g. "AD000001"
		public string Name { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Address { get; set; }


		// Serializes this adopter as one pipe-delimited line for the save file.
		// Field order MUST match FromLine below.
		public string ToLine()
		{
			return $"{Id}|{Name}|{Email}|{Phone}|{Address}";
		}

		// Parses one pipe-delimited line into a new Adopter.
		public static Adopter FromLine(string line)
		{
			var parts = line.Split('|');
			return new Adopter
			{
				Id = parts[0],
				Name = parts[1],
				Email = parts[2],
				Phone = parts[3],
				Address = parts[4]
			};
		}
	}
}
