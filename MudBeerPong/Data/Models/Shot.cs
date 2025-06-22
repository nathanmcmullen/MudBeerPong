using Microsoft.EntityFrameworkCore;

namespace MudBeerPong.Data.Models
{
    public partial class Shot
    {
        public Guid Id { get; set; }

        public Game Game { get; set; }


		/// <summary>
		/// Player who made the shot. May be null if the player is not tracked in the system (e.g., anonymous players, deleted players, etc.)
		/// </summary>
		public Player? Player { get; set; } 
        public DateTimeOffset ShotTime { get; set; }

        public bool IsHit { get; set; }

        public HitType? HitType { get; set; } // null if not hit
        public MissType? MissType { get; set; } // null if hit

        /// <summary>
        /// Cup position for the sunk shot
        /// </summary>
        public CupPosition? CupPosition { get; set; } // null if not hit

        public override string ToString()
        {
            return $"{Player} - {ShotTime} - {(IsHit ? $"Hit ({CupPosition?.ToString() ?? "No position"})" : "Miss")}";
        }

    }

    [Owned]
    public partial record CupPosition
    {
        /// <summary>
        /// Lettered row of the cup
        /// </summary>
        /// <remarks>
        /// e.g. <c>A</c>, <c>B</c>, <c>C</c>
        /// </remarks>
        public required char Row { get; init; } // Row identifier (e.g., "A", "B", "C")

        /// <summary>
        /// Column number of the cup
        /// </summary>
        /// <remarks>
        /// e.g. <c>1</c>, <c>2</c>, <c>3</c>
        /// </remarks>
        public required int Column { get; init; } // Column number (e.g., 1, 2, 3)

        public override string ToString()
        {
            return $"{Row}{Column}";
        }
    }

	public enum HitType
	{
		Straight,
		Bounce,
		Self,
		Other
	}


	public enum MissType
	{
		Airball,
		Rim,
		Table,
		Blocked,
		Other
	}
}
