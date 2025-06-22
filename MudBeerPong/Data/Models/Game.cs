namespace MudBeerPong.Data.Models
{
    public partial class Game
    {
        public Guid Id { get; set; }
        public List<Team>? Teams { get; set; } = new List<Team>();

        public List<Shot>? Shots { get; set; } = new List<Shot>();

        public string? Name { get; set; }

        public string? Description { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }

        public override string ToString()
        {
            return Name ?? string.Empty;
        }
    }
}
