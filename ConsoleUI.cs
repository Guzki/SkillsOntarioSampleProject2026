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
		#region Setup

		private const string AppName = "Animal Rescue Management System";
		private const int BoxWidth = 60;

		// Configures the console window — title bar and UTF-8 output so box-drawing
		// characters and any future emoji render correctly.
		public static void SetupConsole()
		{
			Console.Title = "Animal Rescue Management System";
			Console.OutputEncoding = Encoding.UTF8; // For emoji support
		}



		#endregion



		#region OutputHelpers

		// Writes text in the given color, then restores the previous foreground
		// color so surrounding output is unaffected. Save → change → write → restore.
		public static void WriteColor(string message, ConsoleColor color)
		{
			// Save the current color so we can restore it afterwards. Without this,
			// every colored write would leak its color into everything printed next.
			var previous = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.Write(message);
			Console.ForegroundColor = previous;
		}


		// Same save-change-restore pattern as WriteColor, but ends the line.
		public static void WriteLineColor(string message, ConsoleColor color)
		{
			var previous = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(message);
			Console.ForegroundColor = previous;
		}

		// Clears the screen and draws the app's branded header box with a
		// screen-specific message line beneath it. Call at the top of every screen.
		public static void DrawHeaders(string message)
		{
			Console.Clear();

			string horizontalBorder = new string('═', BoxWidth - 2);
			string separator = new string('─', BoxWidth);

			WriteLineColor($"╔{horizontalBorder}╗", ConsoleColor.Cyan);
			// Center AppName inside the box: PadLeft pushes it toward the middle,
			// then PadRight fills the remaining space so the right border lines up.
			WriteLineColor($"║{AppName.PadLeft((BoxWidth - 2 + AppName.Length) / 2).PadRight(BoxWidth - 2)}║", ConsoleColor.Cyan);
			WriteLineColor($"╚{horizontalBorder}╝", ConsoleColor.Cyan);
			WriteLineColor(message, ConsoleColor.DarkCyan);
			WriteLineColor(separator, ConsoleColor.DarkCyan);

		}


		// Prints "press any key" in gray, waits for a keystroke without echoing
		// it, then clears the screen. Use at the bottom of every screen.
		public static void Pause()
		{
			Console.WriteLine();
			WriteLineColor("Press any key to continue...", ConsoleColor.DarkGray);
			// ReadKey(true) — the `true` means "don't echo the key to the screen."
			// Without it, the typed character would appear on the prompt line.
			Console.ReadKey(true);
			Console.Clear();
		}




		#endregion




		#region InputHelpers

		// Prints a prompt label and returns the user's typed input (unvalidated).
		public static string Prompt(string message)
		{
			Console.Write(message);
			return Console.ReadLine();
		}

		// Displays a numbered list of choices and returns the SELECTED OPTION TEXT
		// (i.e. choices[choice - 1]), not the index. Loops until the user types
		// a valid number within range. Lets callers assign straight to a field
		// without doing a 1-based-to-0-based conversion.
		public static string PromptChoice(string message, string[] choices)
		{
			Console.WriteLine(message);
			for (int i = 0; i < choices.Length; i++)
			{
				WriteLineColor($"{i + 1}. {choices[i]}", ConsoleColor.Cyan);
			}
			while (true)
			{
				var input = Console.ReadLine();
				if (int.TryParse(input, out int choice) && choice >= 1 && choice <= choices.Length)
				{
					return choices[choice - 1];
				}
				WriteLineColor("Invalid choice. Please try again.", ConsoleColor.Red);
			}
		}

		// Reads a date from the user, looping until parsing succeeds. Uses the
		// machine's current culture — the app assumes a single consistent locale.
		public static DateTime PromptDateTime(string message)
		{
			Console.Write(message);
			while (true)
			{
				var input = Console.ReadLine();
				if (DateTime.TryParse(input, out DateTime date))
				{
					return date;
				}
				WriteLineColor("Invalid date format. Please try again. Enter as dd/mm/yyyy.", ConsoleColor.Red);
			}
		}

		// Reads a decimal from the user, looping until parsing succeeds. No upper
		// bound — use the (message, max) overload when a cap is required.
		public static decimal PromptDecimal(string message)
		{
			Console.Write(message);
			while (true)
			{
				var input = Console.ReadLine();
				if (decimal.TryParse(input, out decimal number))
				{
					return number;
				}
				WriteLineColor("Invalid decimal format. Please try again. For example 12.34 or 100", ConsoleColor.Red);
			}
		}

		// Reads a whole number from the user, looping until parsing succeeds.
		public static int PromptInt(string message)
		{
			Console.Write(message);
			while (true)
			{
				var input = Console.ReadLine();
				if (int.TryParse(input, out int number))
				{
					return number;
				}
				WriteLineColor("Invalid integer format. Please try again. For example 42", ConsoleColor.Red);
			}
		}

		// Reads a floating-point number from the user, looping until parsing
		// succeeds. For money use PromptDecimal — decimal is exact, double is not.
		public static double PromptDouble(string message)
		{
			Console.Write(message);
			while (true)
			{
				var input = Console.ReadLine();
				if (double.TryParse(input, out double number))
				{
					return number;
				}
				WriteLineColor("Invalid number format. Please try again. For example 12.34 or 100", ConsoleColor.Red);
			}
		}

		// Reads a decimal from the user, looping until it parses AND is less than
		// or equal to max. Used for bounded values like AdoptionFee (< 300).
		public static decimal PromptDecimal(string message, decimal max)
		{
			Console.Write(message);
			while (true)
			{
				var input = Console.ReadLine();
				if (decimal.TryParse(input, out decimal number) && number <= max)
				{
					return number;
				}
				WriteLineColor($"Invalid decimal format or value exceeds maximum of {max}. Please try again. For example 12.34 or 100", ConsoleColor.Red);
			}
		}

		// Renders a generic table from string data. Given column headers and a
		// list of rows (each row is a string[] with the same number of cells as
		// the headers), measures the widest cell in each column, then pads every
		// cell to that width so columns line up. Domain-agnostic — knows nothing
		// about animals — so the same helper works in any console app.
		//
		// Callers build headers + rows per screen (see CLAUDE.md "Calling
		// PrintTable from a display screen" for the four-step pattern).
		public static void PrintTable(string[] headers, List<string[]> rows)
		{
			//print a message if there are no rows to display
			if (!rows.Any())
			{
				WriteLineColor("No data available to display.", ConsoleColor.Gray);
				return;
			}

			// Two passes. FIRST we measure — walk the headers, then every row, to
			// find the widest cell in each column. We can't print until we know
			// the widths, because a later row might be wider than the header.
			// widths[i] lines up with headers[i] (parallel arrays).
			int[] widths = new int[headers.Length];
			for (int i = 0; i < headers.Length; i++)
			{
				widths[i] = headers[i].Length;
			}
			foreach (var row in rows)
			{
				for (int i = 0; i < row.Length; i++)
				{
					if (row[i].Length > widths[i])
					{
						widths[i] = row[i].Length;
					}
				}
			}



			//print headers in cyan
			for (int i = 0; i < headers.Length; i++)
			{
				WriteColor(headers[i].PadRight(widths[i]) + "   ", ConsoleColor.Cyan);
			}
			Console.WriteLine();

			//print separator in dark cyan
			for (int i = 0; i < headers.Length; i++)
			{
				WriteColor(new string('─', widths[i] + 3), ConsoleColor.DarkCyan);
			}
			Console.WriteLine();
			//print rows in gray
			foreach (var row in rows)
			{
				for (int i = 0; i < row.Length; i++)
				{
					WriteColor(row[i].PadRight(widths[i]) + "   ", ConsoleColor.Gray);
				}
				Console.WriteLine();
			}

			//print a summary line at the end in dark cyan
			WriteLineColor($"\n{rows.Count} row(s) displayed.", ConsoleColor.DarkCyan);


		}


		#endregion
	}
}

