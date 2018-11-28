using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SayOOOnara
{
	public class SlashCommandReader
	{
		protected string UserId { get; set; }
		protected string UserName { get; set; }
		protected string CommandText { get; set; }
		protected Uri ResponseUri { get; set; }
		protected string PostBody { get; set; }

		public virtual async Task<object> HandleRequest(string postBody)
		{
			PostBody = postBody;
			await ReadCommand();
			return await CreateResponse();
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
						if (!parameter.Value.StartsWith('U'))
						{
							throw new ApplicationException(string.Format(
								$"UserId is in an invalid format. UserID must be in format U###... UserID was {0}",
								parameter.Value));
						}

						UserId = parameter.Value;
						break;
					case "user_name":
						UserName = parameter.Value;
						break;
					case "text":
						CommandText = parameter.Value;
						break;
					case "response_url":
						ResponseUri = new Uri(parameter.Value);
						break;
				}
			}
		}


		protected virtual async Task<object> CreateResponse()
		{
			return new {};
		}
	}
}