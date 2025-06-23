using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MudBeerPong.Data.Models;

namespace MudBeerPong.Data
{
	public class ListConverter : ValueConverter<List<CupModel>, string>
	{
		public ListConverter() : base(
			v => string.Join(",", v.Select(c => $"{c.Row}{c.Column}")),
			v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Select(s => new CupModel
				{
					Row = s[0],
					Column = int.Parse(s.Substring(1))
				}).ToList())
		{
		}
	}

}
