using Microsoft.EntityFrameworkCore;
using MudBeerPong.Data.Models;
using MudBeerPong.Data.Models.Joins;
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
		private int activeStepIndex = 0;
		private bool teamsComplete, boardsComplete, SettingsComplete = false;

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				// Initialize or load data here
				using (var context = await DbContextFactory.CreateDbContextAsync())
				{
					// Load existing players and teams from the database
					existingPlayers = await context.Players.ToListAsync();
					existingTeams = await context.Teams.Include(t => t.Players).ToListAsync();
				}
				//existingPlayers.Add(new Player { Id = 1, Name = "Alice" });
				// Seed player data for testing
				//existingPlayers = new List<Player>
				//{
				//	new Player { Id = 1, Name = "Alice" },
				//	new Player { Id = 2, Name = "Bob" },
				//	new Player { Id = 3, Name = "Charlie" },
				//	new Player { Id = 4, Name = "Diana" },
				//	new Player { Id = 5, Name = "Eve" },
				//};

				// Seed team data for testing
				//existingTeams = new List<Team>
				//{
				//	new Team { Id = 1, Name = "Team A", Players = [existingPlayers[0], existingPlayers[1]] },
				//	new Team { Id = 2, Name = "Team B", Players = [existingPlayers[2], existingPlayers[3]] }
				//};


			}

			await base.OnAfterRenderAsync(firstRender);
		}

		private async Task CreateGame()
		{

			Dictionary<int, int> linkedBoards = new Dictionary<int, int>();

			using (var context = await DbContextFactory.CreateDbContextAsync())
			{
				var strategy = context.Database.CreateExecutionStrategy();
				await strategy.ExecuteAsync(async () =>
				{

				var transaction = await context.Database.BeginTransactionAsync();
				try
				{
				// Fixup teams
				var linkedTeams = new List<Team>();

				foreach (var team in _game.Teams ?? [])
				{
					// Link the board
					Board linkedBoard = new Board();
					if (team.Board != null && team.Board.Id == 0)
					{
						// This board is new, must be added to the database
						context.Boards.Add(team.Board);
						await context.SaveChangesAsync();
						// After saving, link the board to the team
						linkedBoard = team.Board;
					}
					else if (team.Board != null)
					{
						// This board already exists, link it
						var existingBoard = context.Boards.FirstOrDefault(b => b.Id == team.Board.Id);
						if (existingBoard != null)
						{
							linkedBoard = existingBoard;
						}
					}

					int teamId = 0;
					if (team.Id == 0)
					{
						// This team is new, must be added to the database
						var linkedPlayers = new List<Player>();
						foreach (var player in team.Players ?? [])
						{
							if (player.Id == 0)
							{
								// This player is new, must be added to the database
								context.Players.Add(player);
								await context.SaveChangesAsync();
							}
							else
							{
								// This player already exists, link it
								var existingPlayer = context.Players.FirstOrDefault(p => p.Id == player.Id);
								if (existingPlayer != null)
								{
									linkedPlayers.Add(existingPlayer);
								}
							}
						}
						// Create a new team with linked players
						var newTeam = new Team
						{
							Name = team.Name
						};

						context.Teams.Add(newTeam);

								newTeam.Players = linkedPlayers;
								context.Teams.Entry(newTeam).Collection(x => x.Players!).IsModified = true;
								await context.SaveChangesAsync();

						teamId = newTeam.Id;
						linkedTeams.Add(newTeam);

					}
					else
					{
						// This team already exists, link it
						var existingTeam = context.Teams.FirstOrDefault(t => t.Id == team.Id);
						if (existingTeam != null)
						{
							// Link the existing team
							linkedTeams.Add(existingTeam);
							teamId = existingTeam.Id;
						}
						else
						{
							Snackbar.Add($"Team with ID {team.Id} not found in the database.", Severity.Error);
							continue; // Skip this team if it doesn't exist. Check that the id is valid
						}
					}

					// Save the board connection to process after game creation
					if (linkedBoard != null && linkedBoard.Id > 0)
					{
						// Ensure the board is linked to the team
						linkedBoards.Add(teamId, linkedBoard.Id);
					}


				}
				// Create the game with linked teams
				_game.Teams = linkedTeams;
				context.Games.Add(_game);
				await context.SaveChangesAsync();

				// Use raw sql to insert join (to avoid tracking issues)

				var sql = "INSERT INTO StartingBoards (GameId, TeamId, BoardId) VALUES (@p0, @p1, @p2)";
				foreach (var kvp in linkedBoards)
				{
					int teamId = kvp.Key;
					int boardId = kvp.Value;
					await context.Database.ExecuteSqlRawAsync(sql, _game.Id, teamId, boardId);
				}

					// Commit the transaction
					await transaction.CommitAsync();


				}
				catch (Exception ex)
				{
					Snackbar.Add($"Error creating game: {ex.Message}", Severity.Error);
					//await transaction.RollbackAsync();

					//throw ex;
					return;
				}

				});

			}

			Snackbar.Add("Game created successfully!", Severity.Success);

			// Navigate to the game play page
			string hash = Hasher.Encode(_game.Id);
			NavigationManager.NavigateTo($"/game/play/{hash}");
		}
		

		private async Task OnPreviewInteraction(StepperInteractionEventArgs arg)
		{
			// activeStepIndex is the index of the current step in the Stepper (to be validated)
			// arg.StepIndex is the index of the step that is being interacted with (to be navigated to)

			switch (activeStepIndex)
			{
				case 0:
					// Validate team selection
					arg.Cancel = !ValidateTeams();
					break;
				case 1:
					// Validate board selection
					arg.Cancel = !ValidateBoards();
					break;
				default:
					// Handle other steps if necessary
					break;

			}
			
			if (arg.Action == StepAction.Activate)
			{
				if (!arg.Cancel)
				{
					// Mark previous steps as complete
					if(arg.StepIndex > 0)
					{
						teamsComplete = true;
					} 
					if (arg.StepIndex > 1)
					{
						boardsComplete = true;
					}
					if (arg.StepIndex > 2)
					{
						SettingsComplete = true;
					}

				}
			}

			if (arg.Action == StepAction.Complete && arg.StepIndex == 3)
			{
				// Final step, validate the game creation
				if (!ValidateTeams() || !ValidateBoards())
				{
					arg.Cancel = true;
					Snackbar.Add("Please fix the errors before completing the game setup.", Severity.Error);
					return;
				}
				// Proceed with game creation logic
				await CreateGame();
			}

			//Console.WriteLine($"Step {arg.StepIndex} interaction from {activeStepIndex}: Action={arg.Action}, Cancel={arg.Cancel}");
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
				Id = 0,
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
			if (string.IsNullOrWhiteSpace(newTeam.Name) || newTeam.Players == null || !newTeam.Players.Any())
			{
				Snackbar.Add("Please enter a team name", Severity.Warning);
				return;
			}

			if (newTeam.Players != null && newTeam.Players.Any())
			{
				// If the team has players, add it to the game
				_game.Teams ??= new List<Team>();

				// Check if the game already has this team set
				if (_game.Teams.Any(t => t.Name?.Equals(newTeam.Name, StringComparison.OrdinalIgnoreCase) ?? false))
				{
					Snackbar.Add("This team already exists in the game.", Severity.Warning);
					return;
				}

				// Check if the team already exists
				var existingTeam = existingTeams.FirstOrDefault(t => t.Name?.Equals(newTeam.Name, StringComparison.OrdinalIgnoreCase) ?? false);
				if (existingTeam == null)
				{
					// If the team does not exist, add it
					newTeam.Board ??= StandardBoardLayouts.ThreeBaseBoard; // Ensure the team has a board
					_game.Teams.Add(newTeam);
				}
				else
				{
					// If the team exists, take the existing team
					existingTeam.Board ??= StandardBoardLayouts.ThreeBaseBoard; // Ensure the existing team has a board
					_game.Teams.Add(existingTeam);
				}
				ResetTeamFields();
			}
			else
			{
				Snackbar.Add("Please add players to the team", Severity.Warning);
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

				teamNameDisabled = true;
			}


		}

		private bool ValidateTeams()
		{
			// Validate that the game has at least two teams and each team has at least one player
			if (_game.Teams == null || _game.Teams.Count < 2)
			{
				Snackbar.Add("Please add at least two teams.", Severity.Error);
				return false;
			}
			foreach (var team in _game.Teams)
			{
				if (team.Players == null || !team.Players.Any())
				{
					Snackbar.Add($"Team '{team.Name}' must have at least one player.", Severity.Error);
					return false;
				}
			}

			return true;

		}

		private bool ValidateBoards()
		{
			// Validate that each team has a board, and each board has at least one cup
			if (_game.Teams == null || !_game.Teams.Any())
			{
				Snackbar.Add("Please add at least one team.", Severity.Error);
				return false;
			}
			foreach (var team in _game.Teams)
			{
				if (team.Board == null || team.Board.InitialPositions.Count == 0)
				{
					Snackbar.Add($"Team '{team.Name}' must have a valid board with at least one cup.", Severity.Error);
					return false;
				}
			}

			return true;
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

		private void ApplyBoardToAll(Board? board)
		{
			foreach (var team in _game.Teams ?? [])
			{
				team.Board = board;
			}
		}


		private async Task<IEnumerable<string?>> SearchTeams(string searchString, CancellationToken token)
		{
			IEnumerable<string?> returnVal = [];
			if (string.IsNullOrWhiteSpace(searchString))
			{
				return existingTeams.Select(t => t.Name);
			}

			returnVal = existingTeams
				.Where(t => t.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
				.Select(t => t.Name);

			if (!returnVal.Contains(searchString, StringComparer.OrdinalIgnoreCase))
			{
				returnVal = returnVal.Append(searchString);
			}

			return returnVal;

		}
	}
}