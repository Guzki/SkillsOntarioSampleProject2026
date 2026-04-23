// =============================================================================
// ConsoleUI.cs
// Animal Rescue Management System — Skills Ontario (Reference Solution)
// Shared static helpers for console prompts and display. Knows nothing about
// animals or the shelter domain — intentionally generic so the same file can
// be reused in any console app.
// Reference: workbook/05-console-ui.md (upcoming)
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillsOntarioSampleProject2026
{
	public static class ConsoleUI
	{
		// Prints a prompt label and returns the user's typed input (unvalidated).
		public static string Prompt(string message)
		{
			Console.Write(message);
			return Console.ReadLine();
		}

		// Displays a numbered list of choices and returns the user's pick (1-based).
		// Loops until the user types a valid number within range.
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
