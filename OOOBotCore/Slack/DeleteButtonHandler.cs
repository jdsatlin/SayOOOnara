using System;
using System.Collections.Generic;
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

		public DeleteButtonHandler(SlackActionPayload action)
		{
			Action = action;
		}

		public async Task<object> HandleRequest()
		{
			return await DeletePeriod();
		}


		public async Task<object> DeletePeriod()
		{
			
			var period = OooPeriods.GetById(Action.Value);
			OooPeriods.RemoveOooPeriodByPeriodId(Action.Value);
			var messageText =
				$"Your upcoming out of office period beginning {period.StartTime.ToLocalTime().ToShortDateString()}," +
				$" and ending {period.EndTime.ToLocalTime().ToShortDateString()} has been cancelled.";
			return new {text = messageText};

		}

	}
}