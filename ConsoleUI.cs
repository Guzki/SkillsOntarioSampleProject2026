using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillsOntarioSampleProject2026
{
	public static class ConsoleUI
	{
		public static string Prompt(string message)
		{
			Console.Write(message);
			return Console.ReadLine();
		}

		public static int PromptChoice(string message, string[] choices)
		{
			Console.WriteLine(message);
			for (int i = 0; i < choices.Length; i++)
			{
				Console.WriteLine($"{i + 1}. {choices[i]}");
			}
			while (true)
			{
				var input = Console.ReadLine();
				if (int.TryParse(input, out int choice) && choice >= 1 && choice <= choices.Length)
				{
					return choice;
				}
				Console.WriteLine("Invalid choice. Please try again.");
			}
		}
	}
}
