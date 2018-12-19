using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SayOOOnara
{
	public class DeleteButtonHandler
	{
		private SlackActionPayload Action { get; }
		private ISlackClient SlackClient { get; set; }

		public DeleteButtonHandler(SlackActionPayload action, ISlackClient slackClient)
		{
			Action = action;
			SlackClient = slackClient;
		}

		public async Task<object> HandleRequest()
		{
			return await DeletePeriod();
		}


		public async Task<object> DeletePeriod()
		{
			
			var period = OooPeriods.GetById(Action.Value);
			string messageText;
			if (period.IsCurrentlyActive)
			{
				period.EndNow();
				messageText = 
					"Your current out of office period beginning: "
					+ $"{(period.StartTime.ToLocalTime().Hour == 0 ? period.StartTime.ToLocalTime().ToShortDateString() : period.StartTime.ToLocalTime().ToString("g", CultureInfo.CurrentCulture))}"
					+ " has now been marked as over.";
				await SlackClient.UpdateLastMessage();
			}
			else
			{
				OooPeriods.RemoveOooPeriodByPeriodId(Action.Value);
				messageText =
					$"Your upcoming out of office period beginning {period.StartTime.ToLocalTime().ToShortDateString()}," +
					$" and ending {period.EndTime.ToLocalTime().ToShortDateString()} has been cancelled.";
			}
			
			return new {text = messageText};

		}

	}
}