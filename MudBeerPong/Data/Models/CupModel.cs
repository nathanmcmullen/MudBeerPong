using System.Numerics;

namespace MudBeerPong.Data.Models
{
	public partial class CupModel
	{
		public int Id { get; set; }
		public Vector2 Position { get; set; }
		/// <summary>
		/// Lettered row of the cup
		/// </summary>
		/// <remarks>
		/// e.g. <c>A</c>, <c>B</c>, <c>C</c>
		/// </remarks>
		public required char Row { get; set; } // Row identifier (e.g., "A", "B", "C")

		/// <summary>
		/// Column number of the cup
		/// </summary>
		/// <remarks>
		/// e.g. <c>1</c>, <c>2</c>, <c>3</c>
		/// </remarks>
		public required int Column { get; set; } // Column number (e.g., 1, 2, 3)

		public override string ToString()
		{
			return $"{Row}{Column}";
		}

	}

	public static class StandardBoardLayouts
	{
		/// <summary>
		/// Validates a beer pong cup configuration.
		/// Rules:
		/// - Rows must have decreasing (or equal) number of cups as you go down.
		/// - Cups in a row must be contiguous (no gaps).
		/// - Each cup (except in the first row) must touch at least one cup in the row above.
		/// - No duplicate positions.
		/// </summary>
		public static bool IsValidConfiguration(List<CupModel> cups)
		{
			if (cups == null || cups.Count == 0) return false;

			// Group by row (ordered by row letter)
			var rowGroups = cups
				.GroupBy(c => c.Row)
				.OrderBy(g => g.Key)
				.ToList();

			// Check for duplicate positions
			var uniquePositions = new HashSet<(char, int)>();
			foreach (var cup in cups)
			{
				if (!uniquePositions.Add((cup.Row, cup.Column)))
					return false;
			}

			int? prevRowCount = null;
			char? prevRow = null;
			HashSet<int> prevRowColumns = null;

			foreach (var group in rowGroups)
			{
				var columns = group.Select(c => c.Column).OrderBy(c => c).ToList();

				// Check contiguous columns (no gaps)
				for (int i = 1; i < columns.Count; i++)
				{
					if (columns[i] != columns[i - 1] + 1)
						return false;
				}

				// Check decreasing (or equal) number of cups per row
				if (prevRowCount.HasValue && columns.Count > prevRowCount.Value)
					return false;

				// For all but the first row, check "touching" (adjacent) to at least one cup in the row above
				if (prevRowColumns != null)
				{
					foreach (var col in columns)
					{
						// Touching means same column or adjacent column in previous row
						if (!prevRowColumns.Contains(col) &&
							!prevRowColumns.Contains(col - 1) &&
							!prevRowColumns.Contains(col + 1))
						{
							return false;
						}
					}
				}

				prevRowCount = columns.Count;
				prevRowColumns = columns.ToHashSet();
				prevRow = group.Key;
			}

			return true;
		}

		public static List<CupModel> ThreeBase =>
		[
			new() { Row = 'A', Column = 1 },
			new() { Row = 'A', Column = 2 },
			new() { Row = 'A', Column = 3 },
			new() { Row = 'B', Column = 1 },
			new() { Row = 'B', Column = 2 },
			new() { Row = 'C', Column = 2 }

		];

		public static List<CupModel> FourBase =>
		[
			new() { Row = 'A', Column = 1 },
			new() { Row = 'A', Column = 2 },
			new() { Row = 'A', Column = 3 },
			new() { Row = 'A', Column = 4 },
			new() { Row = 'B', Column = 1 },
			new() { Row = 'B', Column = 2 },
			new() { Row = 'B', Column = 3 },
			new() { Row = 'C', Column = 2 },
			new() { Row = 'C', Column = 3 },
			new() { Row = 'D', Column = 2 }


		];


	}
}
