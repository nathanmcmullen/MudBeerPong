namespace MudBeerPong.Data.Models
{
    public partial class Player
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        // Collection navigation property
        public List<Team>? Teams { get; set; } = new List<Team>();

        public List<Game>? Games { get; set; } = new List<Game>();

        public List<Shot>? Shots { get; set; } = new List<Shot>();

        public override string ToString()
        {
            return Name ?? string.Empty;
        }
    }
}
