using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillsOntarioSampleProject2026
{
	public class Animal
	{
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


		public string ToLine()
		{
			return $"{Id}|{Name}|{Breed}|{Species}|{Birthday.ToString("dd/MM/yyyy")}|{SpayedOrNeutered}|{Gender}|{Colour}|{VaccineStatus}|{IdentificationNumber}|{IdentificationType}|{AdoptionFee}";
		}

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
