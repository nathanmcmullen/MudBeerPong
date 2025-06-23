using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MudBeerPong.Data.Models;
using MudBeerPong.Data.Models.Joins;

namespace MudBeerPong.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
	public DbSet<Game> Games { get; set; } = null!;
	public DbSet<Team> Teams { get; set; } = null!;
	public DbSet<Player> Players { get; set; } = null!;
	public DbSet<Shot> Shots { get; set; } = null!;
	public DbSet<Board> Boards { get; set; } = null!;
	public DbSet<StartingBoard> StartingBoards { get; set; } = null!;


	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);


		builder.Entity<Game>()
			.HasMany(g => g.Teams)
			.WithMany(t => t.Games);
		builder.Entity<Game>()
			.HasMany(g => g.Shots)
			.WithOne(s => s.Game)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<Team>()
			.HasMany(t => t.Games)
			.WithMany(g => g.Teams);
		builder.Entity<Team>()
			.HasMany(t => t.Players)
			.WithMany(p => p.Teams);

		builder.Entity<Player>()
			.HasMany(p => p.Teams)
			.WithMany(t => t.Players);
		builder.Entity<Player>()
			.HasMany(p => p.Games)
			.WithMany();
		builder.Entity<Player>()
			.HasMany(p => p.Shots)
			.WithOne(s => s.Player)
			.OnDelete(DeleteBehavior.SetNull);

		builder.Entity<Shot>()
			.HasOne(s => s.Game)
			.WithMany(g => g.Shots);
		builder.Entity<Shot>()
			.HasOne(s => s.Player)
			.WithMany(p => p.Shots)
			.OnDelete(DeleteBehavior.SetNull);
		builder.Entity<Shot>()
			.OwnsOne(s => s.CupPosition, cp =>
			{
				cp.Property(c => c.Row).IsRequired().HasColumnName("PositionRow");
				cp.Property(c => c.Column).IsRequired().HasColumnName("PositionColumn");
			});

		builder.Entity<Board>()
			.Property(b => b.InitialPositions)
			.HasConversion<ListConverter>();

		
		builder.Entity<StartingBoard>()
			.HasOne(sb => sb.Game)
			.WithMany()
			.OnDelete(DeleteBehavior.Cascade);
		builder.Entity<StartingBoard>()
			.HasOne(sb => sb.Team)
			.WithMany()
			.OnDelete(DeleteBehavior.Cascade);
		builder.Entity<StartingBoard>()
			.HasOne(sb => sb.Board)
			.WithMany()
			.OnDelete(DeleteBehavior.Cascade);


		Console.WriteLine(builder.Model.ToDebugString());

	}


}
