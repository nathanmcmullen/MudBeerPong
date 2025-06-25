using Microsoft.EntityFrameworkCore;
using MudBeerPong.Data.Models;

namespace MudBeerPong.Data
{
	/// <summary>
	/// A static class for updating the graph of entities in the database for disconnected entities.
	/// </summary>
	public static class GraphUpdater
	{
		/// <summary>
		/// Updates the graph of entities in the database for disconnected entities. Generic method to handle any entity type using Entity Framework logic.
		/// </summary>
		/// <param name="context">The database context.</param>
		/// <param name="entity">The disconnected entity to update.</param>
		public static async Task UpdateGraph<T>(this ApplicationDbContext context, T entity) where T : class
		{
			context.Update(entity);
			await context.SaveChangesAsync();
		}

		public static async Task SaveGame(this ApplicationDbContext context, Game entity)
		{
			// Retrieving the existing game to update its properties
			var existingGame = await context.Games
				.Include(g => g.Teams!)
					.ThenInclude(t => t.Players)
				.FirstOrDefaultAsync(g => g.Id == entity.Id);

			if (existingGame == null)
			{
				// If the game does not exist, add it as a new entity
				context.Entry(entity).State = EntityState.Added;

				// Add the teams
				foreach (var team in entity.Teams ?? [])
				{
					var existingTeam = await context.Teams
						.Include(t => t.Players)
						.FirstOrDefaultAsync(t => t.Id == team.Id);

					if (existingTeam == null)
					{
						context.Entry(team).State = EntityState.Added;

						var playerCollection = new List<Player>();
						foreach (var player in team.Players ?? [])
						{
							var existingPlayer = await context.Players
								.FirstOrDefaultAsync(p => p.Id == player.Id);

							if (existingPlayer == null)
							{
								context.Entry(player).State = EntityState.Added;
								playerCollection.Add(player);
							}
							else
							{
								playerCollection.Add(existingPlayer);
							}
						}
						team.Players = playerCollection;
					}
					else
					{
						context.Entry(existingTeam);
					}
				}

			}
			else
			{
				context.Entry(existingGame).CurrentValues.SetValues(entity);

			}

		}


	}
}
