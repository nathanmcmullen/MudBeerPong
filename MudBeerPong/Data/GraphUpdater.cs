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

		public static async Task SaveGame(this ApplicationDbContext context, Game game)
		{
			// This method is specifically for saving a Game entity, which may have additional logic or relationships.
			context.Update(game);
			await context.SaveChangesAsync();
		}


	}
}
