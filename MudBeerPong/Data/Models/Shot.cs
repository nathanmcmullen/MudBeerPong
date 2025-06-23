using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MudBeerPong.Data.Models
{
    public partial class Shot
    {

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

        public Game Game { get; set; }

        public Team? ShootingTeam { get; set; } // The team that made the shot.

		public Team? TargetTeam { get; set; } // The team that was shot at. 


		/// <summary>
		/// Player who made the shot. May be null in the following cases:
		/// <list type="bullet">
		/// <item>
		/// <description>The player is not tracked by the system (e.g. anonymous, spectator).</description>
		/// </item>
		/// <item>
		/// <description>The player was deleted after the game, and the shot records still remain.</description>
		/// </item>
		/// <item>
		/// <description>The shot represents a penalty or other hit type that doesnt required a shooter.</description>
		/// </item>
		/// </list>
		/// </summary>
		public Player? Player { get; set; } 
        public DateTimeOffset ShotTime { get; set; }

		/// <summary>
		/// Indicates whether the cup was removed from the game after this shot. Allows for flexible games and redemption rules.
		/// By default, this is true, meaning the cup is removed after a hit.
		/// </summary>
		public bool CupRemoved { get; set; }

        public HitType? HitType { get; set; } // null if not hit
        public MissType? MissType { get; set; } // null if hit

        /// <summary>
        /// Cup position for the sunk shot
        /// </summary>
        internal CupPosition? CupPosition { get; set; } // null if not hit

        public override string ToString()
        {
            return $"{Player} - {ShotTime} - {(HitType != null ? $"Hit ({CupPosition?.ToString() ?? "No position"})" : "Miss")}";
        }

    }

    [Owned]
    internal partial record CupPosition
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
		/// <summary>
		/// Shot that goes directly into the cup without bouncing off anything.
		/// </summary>
		Straight,
		/// <summary>
		/// Shot that bounces off the table and then goes into the cup.
		/// </summary>
		Bounce,
		/// <summary>
		/// Shot that is assisted by the target team, e.g., a player from the target team accidentally knocks the ball into their own cup.
		/// </summary>
		Self,
		/// <summary>
		/// The cup that is taken in addition to the cup that was hit when the shooter calls island.
		/// </summary>
		Island,
		/// <summary>
		/// The cup is taken as a penalty for a rule violation.
		/// </summary>
		Penalty,
		Other
	}


	public enum MissType
	{
		Airball,
		Rim,
		Table,
		/// <summary>
		/// Shot that is blocked by the target team, e.g., deflecting on bounce or blowing the ball out of a cup.
		/// </summary>
		Blocked,
		Other
	}
}
