using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace MudBeerPong.Components.Pages.BeerPong
{
	public partial class GameLeadership
	{
		private List<LeaderboardPlayer> _leaderboard = new List<LeaderboardPlayer>();
		private DateRange _dateRange = new(DateTime.Now.AddDays(-7), DateTime.Now);

		private string gameCount = "0";

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			await LoadLeaderboard();
		}

		private async Task LoadLeaderboard()
		{
			_leaderboard.Clear();

			using (var context = await DbContextFactory.CreateDbContextAsync())
			{
				var shots = await context.Shots
					.Include(s => s.Player)
					.Include(s => s.Game)
					.Where(s => s.Game != null && s.Game.StartTime >= _dateRange.Start && s.Game.StartTime <= _dateRange.End)
					.Where(s => s.CupRemoved == true)
					.ToListAsync();

				gameCount = shots.Select(s => s.Game).Distinct().Count().ToString();

				var players = shots
					.GroupBy(s => s.Player)
					.Select(g => new
					{
						Name = g.Key.Name,
						Shots = g.ToList()
					})
					.OrderByDescending(g => g.Shots.Count)
					.ToList();

				if (players != null)
				{
					int rank = 1;
					foreach (var player in players)
					{
						_leaderboard.Add(new LeaderboardPlayer(player.Name, player.Shots.Count, rank++));
					}
				}
			}

			StateHasChanged();
		}

	}

	internal class LeaderboardPlayer
	{
		public string Name { get; set; }
		public int SunkShots { get; set; }
		public int Rank { get; set; }
		public LeaderboardPlayer(string name, int score, int rank)
		{
			Name = name;
			SunkShots = score;
			Rank = rank;
		}
	}
}