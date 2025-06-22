using Microsoft.EntityFrameworkCore;
using MudBeerPong.Data.Models;

namespace MudBeerPong.Data.Services
{
	public class GameService
	{
		private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

		public GameService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
		{
			_dbContextFactory = dbContextFactory;
		}

		public async Task<Game?> GetGameAsync(Guid gameId)
		{
			using var context = await _dbContextFactory.CreateDbContextAsync();
			return await context.Games
				.Include(g => g.Teams)
				.Include(g => g.Shots)
				.FirstOrDefaultAsync(g => g.Id == gameId);
		}

		public async Task<List<Game>> GetAllGamesAsync()
		{
			using var context = await _dbContextFactory.CreateDbContextAsync();
			return await context.Games
				.Include(g => g.Teams)
				.Include(g => g.Shots)
				.ToListAsync();
		}

		public async Task<Game> CreateGameAsync(Game game)
		{
			using var context = await _dbContextFactory.CreateDbContextAsync();
			context.Games.Add(game);
			await context.SaveChangesAsync();
			return game;
		}


	}
}
