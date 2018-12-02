using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace SayOOOnara
{
	public class SlashCommandReader
	{
		protected string UserId { get; set; }
		protected string UserName { get; set; }
		protected string CommandText { get; set; }
		protected Uri ResponseUri { get; set; }
		protected SlackActionPayload Actions { get; set; }
		protected string PostBody { get; }

		protected SlashCommandReader(string postBody)
		{
			PostBody = postBody;
		}


		protected virtual async Task ReadCommand()
		{
			var bodyNameValueCollection = HttpUtility.ParseQueryString(PostBody);
			Dictionary<string, string> messageBody = bodyNameValueCollection.Keys.Cast<string>()
				.ToDictionary(k => k, v => bodyNameValueCollection[v]);


			foreach (var parameter in messageBody)
			{
				switch (parameter.Key)
				{
					case "user_id":
						if (!parameter.Value.ToUpper().StartsWith('U'))
						{
							throw new ApplicationException(
								$"UserId is in an invalid format. UserID must be in format U###... UserID was {parameter.Value}");
						}

						UserId = parameter.Value;
						break;
					case "user_name":
						UserName = parameter.Value;
						break;
					case "text":
						CommandText = parameter.Value;
						break;
					case "actions":
						Actions = JsonConvert.DeserializeObject<SlackActionPayload>(parameter.Value);
						break;
					case "response_url":
						ResponseUri = new Uri(parameter.Value);
						break;
				}
			}
		}


	}
}