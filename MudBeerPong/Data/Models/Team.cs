using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MudBeerPong.Data.Models
{    public partial class Team
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string? Name { get; set; }

		public List<Game>? Games { get; set; } = new List<Game>();

		public IEnumerable<Player>? Players { get; set; } = new List<Player>();

		[NotMapped]
		public Board? Board { get; set; } = new Board();

		[NotMapped]
		public List<CupModel>? Cups { get; set; } = new List<CupModel>();


		public override string ToString()
		{
			return Name ?? "Unnamed Player";
		}

		/// <summary>
		/// Returns the starting board for this team based on the game's board configuration. Retrieves from the GameTeamBoard join table if available, otherwise uses the default board configuration.
		/// </summary>
		/// <param name="game"></param>
		/// <returns></returns>
		public Board GetStartingBoard(Game game, ApplicationDbContext context)
		{
			if (game == null)
			{
				throw new ArgumentNullException(nameof(game), "Game cannot be null");
			}
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context), "ApplicationDbContext cannot be null");
			}
			int gameId = game.Id;
			int teamId = Id;

			// Check if there is a custom board configuration for this team in the game
			var startingBoard = context.StartingBoards
				.FirstOrDefault(sb => sb.Game.Id == gameId && sb.Team.Id == teamId);


			if (startingBoard != null)
			{
				return startingBoard.Board;
			}
			return Board.DefaultBoard(); // Return default board if no custom board is set

		}
	}
}
