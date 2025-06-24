using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MudBeerPong.Data.Models
{
	public partial class Board
	{
		public Board()
		{
			InitialPositions = new List<CupModel>();
		}

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string? Name { get; set; }

		public bool IsDefault { get; set; } = false;

		/// <summary>
		/// Initial positions of cups for this board.
		/// </summary>
		public List<CupModel> InitialPositions { get; set; }

		[NotMapped]
		public int CupCount => InitialPositions.Count;

		/// <inheritdoc cref="StandardBoardLayouts.IsValidConfiguration(List{CupModel})"/>
		public bool IsValidConfiguration()
		{
			return StandardBoardLayouts.IsValidConfiguration(InitialPositions);
		}
		public override string ToString()
		{
			return Name ?? $"Board with {CupCount} cups";
		}

		public static Board DefaultBoard()
		{
			return new Board
			{
				Name = "Default Board",
				IsDefault = true,
				InitialPositions = StandardBoardLayouts.ThreeBase
			};
		}
		public static Board NewBoard()
		{
			return new Board
			{
				Name = "New Board",
				IsDefault = false,
				InitialPositions = new List<CupModel>()
			};
		}


		public void AddCup()
		{
			if (InitialPositions == null)
			{
				InitialPositions = new List<CupModel>();
			}

			// Define the grid: 8 rows (A-H), 8 columns (1-8), offset every other row
			char[] rowLabels = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
			int gridRows = 8;
			int gridColumns = 8;

			// Find the first free spot (left to right, top to bottom)
			for (int rowIdx = 0; rowIdx < gridRows; rowIdx++)
			{
				char row = rowLabels[rowIdx];
				int maxCol = (rowIdx % 2 == 1) ? gridColumns - 1 : gridColumns; // Offset rows have one less column

				for (int col = 1; col <= maxCol; col++)
				{
					bool occupied = InitialPositions.Any(c => c.Row == row && c.Column == col);
					if (!occupied)
					{
						int nextId = InitialPositions.Count > 0 ? InitialPositions.Max(c => c.Id) + 1 : 1;
						InitialPositions.Add(new CupModel
						{
							Id = nextId,
							Row = row,
							Column = col
						});
						return;
					}
				}
			}
			// No free spot found; do nothing or throw if desired
		}

	}
}
