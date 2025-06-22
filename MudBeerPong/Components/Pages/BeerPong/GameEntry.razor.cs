using MudBeerPong.Data.Models;
using MudBlazor;
using System.Threading.Tasks;

namespace MudBeerPong.Components.Pages.BeerPong
{
	public partial class GameEntry
	{
		private Game _game = new Game();

		private Team newTeam = new Team();

		private List<Player> existingPlayers = [];
		private List<Team> existingTeams = [];

		private string playerSearchString = string.Empty;
		private bool teamNameDisabled = false;

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				// Initialize or load data here

				// Seed player data for testing
				existingPlayers = new List<Player>
				{
					new Player { Id = Guid.NewGuid(), Name = "Alice" },
					new Player { Id = Guid.NewGuid(), Name = "Bob" },
					new Player { Id = Guid.NewGuid(), Name = "Charlie" },
					new Player { Id = Guid.NewGuid(), Name = "Diana" },
					new Player { Id = Guid.NewGuid(), Name = "Eve" },
				};

				// Seed team data for testing
				existingTeams = new List<Team>
				{
					new Team { Id = Guid.NewGuid(), Name = "Team A", Players = [existingPlayers[0], existingPlayers[1]] },
					new Team { Id = Guid.NewGuid(), Name = "Team B", Players = [existingPlayers[2], existingPlayers[3]] }
				};
			}

			await base.OnAfterRenderAsync(firstRender);
		}

		private async Task OnPreviewInteraction(StepperInteractionEventArgs arg)
		{
			if (arg.Action == StepAction.Complete)
			{
				// occurrs when clicking next
				await CheckStepCompletion(arg);
			}
			else if (arg.Action == StepAction.Activate)
			{
				// occurrs when clicking a step header with the mouse
				await CheckStepNavigation(arg);
			}
		}

		private async Task CheckStepCompletion(StepperInteractionEventArgs arg)
		{
			// Logic to check if the step is complete
			// If not complete, you can set arg.Cancel = true;

			await Task.CompletedTask;
		}

		private async Task CheckStepNavigation(StepperInteractionEventArgs arg)
		{
			// Logic to check if the step can be navigated to
			// If not allowed, you can set arg.Cancel = true;


			await Task.CompletedTask;
		}

		private async Task AddPlayer()
		{
			string name = playerSearchString.Trim();
			// Validate the player name
			if (string.IsNullOrWhiteSpace(name) || !PlayerNameUnique(name))
			{
				return;
			}

			// Create a new player and add it to the existing players list
			var newPlayer = new Player
			{
				Id = Guid.Empty,
				Name = name
			};
			existingPlayers.Add(newPlayer);

			// Add the player to the selection
			newTeam.Players = newTeam.Players?.Append(newPlayer).ToList() ?? [newPlayer];


		}

		private void ResetTeamFields()
		{
			// Reset the team fields to their initial state
			newTeam = new Team();
			teamNameDisabled = false;
		}

		private void AddTeam()
		{
			// Add the newTeam to the game teams

			if (newTeam.Players != null && newTeam.Players.Any())
			{
				// If the team has players, add it to the game
				if (_game.Teams == null)
				{
					_game.Teams = new List<Team>();
				}

				// Check if the game already has this team set
				if (_game.Teams.Any(t => t.Name?.Equals(newTeam.Name, StringComparison.OrdinalIgnoreCase) ?? false))
				{
					Snackbar.Add("This team already exists in the game.", Severity.Warning);
					return;
				}

				// Check if the team already exists
				var existingTeam = _game.Teams.FirstOrDefault(t => t.Name?.Equals(newTeam.Name, StringComparison.OrdinalIgnoreCase) ?? false);
				if (existingTeam == null)
				{
					// If the team does not exist, add it
					_game.Teams.Add(newTeam);
				}
				else
				{
					// If the team exists, take the existing team

					_game.Teams.Add(existingTeam);
				}
				ResetTeamFields();
			}
			else
			{
				Snackbar.Add("Please enter a team to add.", Severity.Warning);
			}


		}

		private void DeselectTeam(Team team)
		{
			_game.Teams?.Remove(team);

		}

		private void SetPlayersForExistingTeam()
		{
			// Check if team name is from an existing team
			var team = existingTeams.FirstOrDefault(t => t.Name?.Equals(newTeam.Name, StringComparison.OrdinalIgnoreCase) ?? false);
			if (team != null)
			{
				// If the team exists, set the players from the existing team
				newTeam.Players = team.Players?.ToList() ?? [];
			}

			teamNameDisabled = true;

		}

		private void OnPlayersChanged()
		{
			// Check if players already exist in a team
			if (newTeam.Players == null || !newTeam.Players.Any())
			{
				// If no players are selected, reset the team name
				newTeam.Name = string.Empty;
				teamNameDisabled = false;
				return;
			}

			var existingTeamWithPlayers = ExistingTeamWithPlayers(newTeam.Players);

			if (existingTeamWithPlayers != null)
			{
				// If players already exist in a team, set the team name to the existing team's name
				newTeam.Name = existingTeamWithPlayers.Name;
				teamNameDisabled = true;
			}
			else
			{
				// If players do not exist in a team, allow setting a new team name
				newTeam.Name = string.Empty;
				teamNameDisabled = false;
			}
		}
		private bool TeamAlreadyAssigned(string? team)
		{
			// Check if the team is already assigned to the game
			return _game.Teams?.Any(t => t.Name?.Equals(team, StringComparison.OrdinalIgnoreCase) ?? false) ?? false;
		}

		private bool PlayerAlreadyAssigned(Player player)
		{
			// Check if the player is already assigned to a team in the game
			return _game.Teams?.Any(t => t.Players?.Any(p => p.Name?.Equals(player.Name, StringComparison.OrdinalIgnoreCase) ?? false) ?? false) ?? false;
		}

		private bool PlayerNameUnique(string name)
		{
			// Check if the name is unique among existing players
			return !existingPlayers.Any(p => p.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false);

		}
		private Team? ExistingTeamWithPlayers(IEnumerable<Player> players)
		{
			// Check if this specific set of players already exists in an existing team
			return existingTeams.FirstOrDefault(t => t.Players != null && t.Players.Count() == players.Count() &&
				t.Players.All(p => players.Any(np => np.Name?.Equals(p.Name, StringComparison.OrdinalIgnoreCase) ?? false)));
		}

		private bool SearchPlayer(Player? value, string? text, string? searchString)
		{
			playerSearchString = searchString ?? string.Empty;

			if (string.IsNullOrWhiteSpace(searchString))
			{
				return true; // Show all players if no search string is provided
			}
			// Perform a case-insensitive search
			return value?.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false;

		}

		private async Task<IEnumerable<string?>> SearchTeams(string searchString, CancellationToken token)
		{
			await Task.Delay(5, token); 
			//IEnumerable<Team> returns = [.. existingTeams];
			//if (!string.IsNullOrWhiteSpace(searchString))
			//{
			//	returns = returns.Where(t => t.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false);
			//}
			//return returns.Select(t =>
			//{
			//	var teamName = t.Name ?? "Unnamed Team";
			//	var playerNames = t.Players != null
			//		? string.Join(", ", t.Players.Where(p => !string.IsNullOrWhiteSpace(p.Name)).Select(p => p.Name))
			//		: string.Empty;
			//	return string.IsNullOrWhiteSpace(playerNames)
			//		? teamName
			//		: $"{teamName} ({playerNames})";
			//});

			if (string.IsNullOrWhiteSpace(searchString))
			{
				return existingTeams.Select(t => t.Name);
			}

			return existingTeams
				.Where(t => t.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
				.Select(t => t.Name);
		}
	}
}