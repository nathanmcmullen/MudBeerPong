using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MudBeerPong.Data.Models.Joins
{
	public class StartingBoard
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public Game Game { get; set; } = null!;
		public Team Team { get; set; } = null!;
		public Board Board { get; set; } = null!;
	}
}
