using System.ComponentModel.DataAnnotations;

namespace MudBeerPong.Data.Models
{    public partial class Team
    {
        [Key]
		public Guid Id { get; set; }

        public List<Game>? Games { get; set; } = new List<Game>();

        public IEnumerable<Player>? Players { get; set; } = new List<Player>();

        public string? Name { get; set; }

		public override string ToString()
		{
			return Name ?? "Unnamed Player";
		}
	}
}
