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
	public class DeleteButtonHandler : SlackCommandReader
	{
		public DeleteButtonHandler(string postBody)
			: base(postBody)
		{
		}

		public async Task<object> HandleRequest()
		{
			await ReadCommand();
			return await DeletePeriod();
		}

		protected async override Task ReadCommand()
		{
			var bodyNameValueCollection = HttpUtility.ParseQueryString(PostBody);
			Dictionary<string, string> messageBody = bodyNameValueCollection.Keys.Cast<string>()
				.ToDictionary(k => k, v => bodyNameValueCollection[v]);

			var payload = JObject.Parse(messageBody["payload"]);

			Actions = payload["actions"].First.ToObject<SlackActionPayload>();

			ResponseUri = new Uri(payload["response_url"].ToString());

		}

		public async Task<object> DeletePeriod()
		{
			
			var period = OooPeriods.GetById(Actions.Value);
			OooPeriods.RemoveOooPeriodByPeriodId(Actions.Value);
			var messageText =
				$"Your upcoming out of office period beginning {period.StartTime.ToLocalTime().ToShortDateString()}," +
				$" and ending {period.EndTime.ToLocalTime().ToShortDateString()} has been cancelled.";
			return new {text = messageText};

		}

	}
}