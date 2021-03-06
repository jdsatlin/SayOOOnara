using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chronic.Core.System;

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
			var response = await CreateResponse();

			return response;
		}
		protected override async Task ReadCommand()
		{
			await base.ReadCommand();
			_user = Users.Find(UserId);
		}

		protected async Task<object> CreateResponse()
		{
			
			var textBuilder = new StringBuilder();
			var attachments = new List<object>();

			if (_user != null)
			{
				textBuilder.AppendLine(_user.IsOoo ? "You are currently out of office" : "You are not currently out of office.");
				var currentOooPeriods = OooPeriods.GetByUserId(_user.Id).Where(p => p.IsCurrentlyActive).ToList();

				var cancellablePeriods = _user.HasUpcomingOooPeriods || currentOooPeriods.Any();
				textBuilder.AppendLine(cancellablePeriods
					? "You have the following current & upcoming out of office periods:"
					: "You do not have any current or upcoming periods that can be cancelled.");
				if (cancellablePeriods)
				{

					var actions = new List<object>();
					currentOooPeriods.ForEach(async p => actions.Add(await OooCancellationBuilder(p)));

					foreach (var period in OooPeriods.GetUpcomingOooPeriodsByUserId(_user.Id))
					{
						actions.Add(await OooCancellationBuilder(period));
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

			}
			else
			{
				textBuilder.AppendLine("You are not currently out of office.");
				textBuilder.AppendLine("You do not have any current or upcoming periods that can be cancelled.");
			}


			return new
			{
				text = textBuilder.ToString(),
				attachments = attachments
			};

		}

		protected async Task<object> OooCancellationBuilder(OooPeriod period)
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

	}
}