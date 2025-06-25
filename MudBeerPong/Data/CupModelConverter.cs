using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MudBeerPong.Data.Models;

namespace MudBeerPong.Data
{
	public class CupModelConverter : ValueConverter<CupModel, string>
	{
		public CupModelConverter() : base(
			v => v.ToString(),
			v => new CupModel(v))
		{
		}
	}

}
