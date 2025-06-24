using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBeerPong.Data.Models;
using MudBlazor;

namespace MudBeerPong.Components.Pages.BeerPong
{
	public partial class GamePlay
	{
		[Parameter]
		public string GameId { get; set; }

		private Game? _game;

		protected override async Task OnInitializedAsync()
		{
			if (string.IsNullOrEmpty(GameId))
			{
				Snackbar.Add("Invalid game URL. Please provide a valid GameId.", Severity.Error);
				NavigationManager.NavigateTo("/"); // Redirect to home if GameId is not provided
				
			}

			// Decode the game id
			var result = Hasher.Decode(GameId);

			if (result == null || result.Count != 1)
			{
				// Invalid GameId, redirect to new
				Snackbar.Add("Invalid game ID.", Severity.Error);
				NavigationManager.NavigateTo("/");
				return;
			}


			// Load the game data based on GameId
			await LoadGameDataAsync(result[0]);
		}

		private async Task LoadGameDataAsync(int id)
		{
			using (var context = await DbContextFactory.CreateDbContextAsync())
			{
				_game = await context.Games
					.Include(g => g.Teams!)
						.ThenInclude((Team t) => t.Players!)
					.Include(g => g.Shots!)
						.ThenInclude((Shot s) => s.ShootingTeam)
					.Include(g => g.Shots!)
						.ThenInclude((Shot s) => s.TargetTeam)
					.Include(g => g.Shots!)
						.ThenInclude((Shot s) => s.Player)
					.FirstOrDefaultAsync(g => g.Id == id);

				if (_game == null)
				{
					Snackbar.Add("Game not found.", Severity.Error);
					NavigationManager.NavigateTo("/");
				}


			}

		}
	}
}