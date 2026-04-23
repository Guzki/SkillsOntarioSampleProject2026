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
		public static List<Animal> animals = new List<Animal>();

		private const string FilePath = "animals.txt";
		public static void AddAnimal(Animal animal)
		{
			animal.Id = NextId();
			animals.Add(animal);
		}

		private static string NextId()
		{
			if (animals.Count == 0)
				return "1";
			var lastId = animals.Max(a => int.Parse(a.Id));
			return (lastId + 1).ToString("D8");
		}

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
