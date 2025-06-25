using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBeerPong.Data.Models;
using MudBlazor;
using MudExtensions;

namespace MudBeerPong.Components.Pages.BeerPong
{
	public partial class GameShot
	{
		[Parameter]
		public string GameId { get; set; } = default!;

		[SupplyParameterFromQuery]
		public bool? Sunk { get; set; }

		[SupplyParameterFromQuery]
		public int? Type { get; set; }

		[SupplyParameterFromQuery]
		public string? CupPosition { get; set; }

		[SupplyParameterFromQuery(Name = "target")]
		public string? TargetHash { get; set; }

		CupModel? SunkCup;
		HitType? _HitType;
		MissType? _MissType;

		Game? _game;

		MudStepperExtended _stepper = default!;

		Shot _shot = new Shot();

		protected override async Task OnInitializedAsync()
		{
			if (string.IsNullOrEmpty(GameId))
			{
				Snackbar.Add("Invalid game URL. Please provide a valid GameId.", Severity.Error);
				NavigationManager.NavigateTo("/"); // Redirect to home if GameId is not provided

			}

			// Decode the game id
			var ids = Hasher.Decode(GameId);

			if (ids == null || ids.Count != 1)
			{
				// Invalid GameId, redirect to new
				Snackbar.Add("Invalid game ID.", Severity.Error);
				NavigationManager.NavigateTo("/");
				return;
			}

			// Load the game data based on GameId
			var result = await LoadGame(ids[0]);

			if (!result)
			{
				// If the game could not be loaded, redirect to home
				Snackbar.Add("Game not found.", Severity.Error);
				NavigationManager.NavigateTo("/");
				return;
			}
			else
			{
				_shot.Game = _game;
				// If the game was loaded successfully, process the shot parameters
				if (Sunk.HasValue && Sunk.Value)
				{
					if (CupPosition != null)
					{
						SunkCup = new CupModel(CupPosition);
						_shot.CupPosition = SunkCup;
					}
					if (Type.HasValue)
					{
						_HitType = (HitType)Type.Value;
						_shot.HitType = _HitType;
					}
				}
				else if (Sunk.HasValue && !Sunk.Value)
				{
					if (Type.HasValue)
					{
						_MissType = (MissType)Type.Value;
						_shot.MissType = _MissType;
					}
				}

				// Find the suggested team based on the TeamHash
				if (!string.IsNullOrEmpty(TargetHash) && _game.Teams != null)
				{

					int? teamId = DecodeSingleHash(TargetHash);

					if (teamId == null)
					{
						Snackbar.Add("Invalid suggested team.", Severity.Warning);
						return;
					}

					var target = _game.Teams.FirstOrDefault(t => t.Id == teamId);

					if (target == null)
					{
						Snackbar.Add("Team not found.", Severity.Warning);
					}
					else
					{
						_shot.TargetTeam = target;
					}
				}
			}
		}

		async Task<bool> LoadGame(int id)
		{
			using (var context = await DbContextFactory.CreateDbContextAsync())
			{
				_game = await context.Games
					.Include(g => g.Teams!)
						.ThenInclude(t => t.Players)
					.Include(g => g.Shots!)
						.ThenInclude(s => s.TargetTeam)
					.FirstOrDefaultAsync(g => g.Id == id);
				if (_game == null)
				{
					Snackbar.Add("Game not found.", Severity.Error);
					return false;
				}
				else
				{
					if (_game.Teams != null && _game.Teams.Count > 0)
					{
						for (int i = 0; i < _game.Teams.Count; i++)
						{

							// Populate starting board
							_game.Teams[i].Board = _game.Teams[i].GetStartingBoard(_game, context);
							_game.Teams[i].Cups = _game.Teams[i].Board!.InitialPositions;

							// Apply shots to board
							if (_game.Shots != null && _game.Shots.Count > 0)
							{
								_game.Teams[i].Cups = _game.Teams[i].Board!.ApplyShots(_game.Shots.Where(s => s.TargetTeam?.Id == _game.Teams[i].Id).ToList());
							}
						}
					}

				}
					return true;
			}

		}

		private async Task SubmitShot()
		{
			if (_shot.ShootingTeam == null)
			{
				Snackbar.Add("Please select a shooting team.", Severity.Warning);
				return;
			}
			if (_shot.Player == null)
			{
				Snackbar.Add("Please select a player.", Severity.Warning);
				return;
			}
			if (_shot.CupPosition == null && _shot.HitType == null && _shot.MissType == null)
			{
				Snackbar.Add("Please select a cup position or hit/miss type.", Severity.Warning);
				return;
			}
			using (var context = await DbContextFactory.CreateDbContextAsync())
			{
				var targetTeam = new Team
				{
					Id = _shot.TargetTeam?.Id ?? 0,
				};
				var shotTeam = new Team
				{
					Id = _shot.ShootingTeam.Id,
				};



				await context.SaveChangesAsync();
			}
			Snackbar.Add("Shot submitted successfully!", Severity.Success);
			// Redirect to the game play page
			GoBack();
		}

		

		private async Task TeamClicked(Team team)
		{
			// Set the shooting team to the clicked team
			_shot.ShootingTeam = team;
			// Set the target team to the other team
			_shot.TargetTeam = _game?.Teams?.FirstOrDefault(t => t.Id != team.Id);

			//// Move to the next step
			await _stepper.CompleteStep(_stepper.GetActiveIndex());

		}

		private async Task PlayerClicked(Player player)
		{
			// Set the player who made the shot
			_shot.Player = player;
			// Move to the next step
			await _stepper.CompleteStep(_stepper.GetActiveIndex());
		}

		private async Task ContinueAsMiss()
		{
			// Remove cup
			_shot.CupPosition = null;

			// If the shot is a miss, we can continue to the next step
			await _stepper.CompleteStep(_stepper.GetActiveIndex());
		}

		private async Task CupClicked(CupModel cup)
		{
			// Set the cup position for the shot
			_shot.CupPosition = cup;
			// Move to the next step
			await _stepper.CompleteStep(_stepper.GetActiveIndex());
		}

		private async Task HitTypeClicked(HitType hitType)
		{
			// Set the hit type for the shot
			_shot.HitType = hitType;
			// Move to the next step		
			await _stepper.CompleteStep(_stepper.GetActiveIndex());
		}
		private async Task MissTypeClicked(MissType missType)
		{
			// Set the miss type for the shot
			_shot.MissType = missType;
			// Move to the next step
			await _stepper.CompleteStep(_stepper.GetActiveIndex());
		}
		public void GoBack()
		{
			string returnUrl = "/game/play/" + GameId + "?resume=true";

			NavigationManager.NavigateTo(returnUrl);

		}

		public int? DecodeSingleHash(string? hash)
		{
			if (string.IsNullOrEmpty(hash))
			{
				return null;
			}
			var ids = Hasher.Decode(hash);
			if (ids == null || ids.Count != 1)
			{
				return null; // Invalid hash
			}
			return ids[0];
		}
	}
}