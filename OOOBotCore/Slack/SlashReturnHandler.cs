using System.Collections.Generic;
using System.Threading.Tasks;

namespace OOOBotCore
{
	public class SlashReturnHandler : SlashCommandReader
	{
		private User _user;
		private bool _foundPeriods;

		protected override async Task ReadCommand()
		{
			await base.ReadCommand();

			List<OooPeriod> userOooPeriods = new List<OooPeriod>();
			_user = Users.FindOrCreateUser(UserId);
			userOooPeriods = OooPeriods.GetByUserId(UserId);
			_foundPeriods = userOooPeriods.Count > 0;
			userOooPeriods.ForEach(p => OooPeriods.RemoveOooPeriodByPeriodId(p.Id));
		}

		protected async override Task<object> CreateResponse()
		{
			var responseText =
				$"{(_foundPeriods ? "All your current and upcoming out of office periods have been cancelled" : "I could not find any current or upcoming out of office periods for you")}";
			return new {text = responseText};
		}
	}
}