using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace SayOOOnara
{
	public class InteractiveMessageReader
	{
		protected string PostBody { get; }
		protected SlackActionPayload Actions { get; set; }
		protected Uri ResponseUri { get; set; }
		protected string CallbackId { get; set; }
		protected string TeamId { get; set; }
		protected string UserId { get; set; }
		


		protected InteractiveMessageReader(string postBody)
		{
			PostBody = postBody;
		}

		protected async virtual Task ReadCommand()
		{
			var bodyNameValueCollection = HttpUtility.ParseQueryString(PostBody);
			Dictionary<string, string> messageBody = bodyNameValueCollection.Keys.Cast<string>()
				.ToDictionary(k => k, v => bodyNameValueCollection[v]);

			var payload = JObject.Parse(messageBody["payload"]);

			Actions = payload["actions"].First.ToObject<SlackActionPayload>();
			ResponseUri = new Uri(payload["response_url"].ToString());
			CallbackId = payload["callback_id"].ToString();
			TeamId = payload["team"].Value<string>("id");
			UserId = payload["user"].Value<string>("id");

		}

	}
}