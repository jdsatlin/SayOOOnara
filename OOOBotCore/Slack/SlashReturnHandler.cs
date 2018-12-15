using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SayOOOnara
{
	public class SlashReturnHandler : SlashCommandReader
	{
		private User _user;
		private bool _foundPeriods;

		public SlashReturnHandler(string postBody)
		: base(postBody)
		{

		}


		public async Task<object> HandleRequest()
		{

			await ReadCommand();
			return await CreateResponse();
		}
		protected override async Task ReadCommand()
		{
			await base.ReadCommand();
			_user = Users.Find(UserId);
		}

		protected async Task<object> CreateResponse()
		{
			var textBuilder = new StringBuilder();
			textBuilder.AppendLine(_user.IsOoo ? "You have been marked back in office" : "You are not currently out of office.");
			foreach (var period in OooPeriods.GetByUserId(_user.Id).Where(p => p.IsCurrentlyActive))
			{
				period.EndNow();
			}
			var upcomingOoo = _user.HasUpcomingOooPeriods;
			textBuilder.AppendLine(upcomingOoo
				? "You have the following upcoming out of office periods:"
				: "You do not have any upcoming periods that can be cancelled.");
			var attachments = new List<object>();
			if (upcomingOoo)
			{
				
				var actions = new List<object>();
				foreach (var period in OooPeriods.GetUpcomingOooPeriodsByUserId(_user.Id))
				{
					actions.Add(await oooCancellationBuilder(period));
				}

				var attachmentBody = new
				{
					text = "Select a period to cancel",
					fallback = "Sorry, something went wrong",
					callback_id = "cancelperiod",
					actions = actions
			};
				attachments.Add(attachmentBody);

			}

			return new
			{
				text = textBuilder.ToString(),
				attachments = attachments
			};

		}

		protected async Task<object> oooCancellationBuilder(OooPeriod period)
		{
			var button = new
			{
				name = "OOOPeriod",
				text = $"{period.StartTime.ToLocalTime().ToShortDateString()}-{period.EndTime.ToLocalTime().ToShortDateString()}" +
					   $"{(period.Message.Length > 0 ? ":" : "")}" +
				       $" {(period.Message.Length > 10 ? period.Message.Substring(0, 10) : period.Message)}",
				type = "button",
				value = $"{period.Id}"
			};
			return button;
		}

		protected async Task DeletePeriod(int periodId)
		{
			OooPeriods.RemoveOooPeriodByPeriodId(periodId);
		}

	
	}
}