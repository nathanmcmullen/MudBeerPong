using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MudBeerPong.Data.Models
{
	public partial class Board
	{

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

	}
}
