using Microsoft.AspNetCore.Components;
using Microsoft.Build.Framework;
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

		bool loading = true;

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
			loading = true;
			await InvokeAsync(StateHasChanged); // Ensure the UI updates to show loading state

			using (var context = await DbContextFactory.CreateDbContextAsync())
			{
				
				_game = await context.Games
					.Include(g => g.Teams!)
						.ThenInclude(t => t.Players)
					.Include(g => g.Shots)
					.FirstOrDefaultAsync(g => g.Id == id);

				// Get starting board
				if (_game != null && _game.Teams != null && _game.Teams.Count > 0)
				{
					for (int i = 0; i < _game.Teams.Count; i++)
					{
						_game.Teams[i].Board = _game.Teams[i].GetStartingBoard(_game, context);
						_game.Teams[i].Cups = _game.Teams[i].Board!.InitialPositions;
					}
				}

				if (_game == null)
				{
					Snackbar.Add("Game not found.", Severity.Error);
					NavigationManager.NavigateTo("/");
				}


			}
			loading = false; 
			await InvokeAsync(StateHasChanged);
		}

		public void MissedShot()
		{
			// Navigate to the missed shot page
			string url = $"/game/play/{GameId}/shot?sunk=false";
			NavigationManager.NavigateTo(url);
		}

		public async Task CupClicked(CupModel cup, Team team)
		{

			// Create url 
			string url = "/game/play/" + GameId + "/shot";

			var parameters = new Dictionary<string, object?>
			{
				{ "target", Hasher.Encode(team.Id) },
				{ "sunk", true },
				{ "cupPosition", cup.ToString() }
			};

			// Navigate to the shot page with parameters
			NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameters(url, parameters));


		}
	}
}