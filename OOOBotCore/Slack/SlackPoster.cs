using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace SayOOOnara
{
	public class SlackClient : HttpClient
	{
	    private readonly OAuthClient _oAuthClient;
		private string _authToken => _oAuthClient.AuthToken;
		private const string PostMessageUrl = "https://slack.com/api/chat.postMessage";
		private static IOptions _options;



		public SlackClient()
	    {
	        _oAuthClient = new OAuthClient();
			DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
			_options = new OptionsFile();
		    AuthTest();



	    }

		private enum MimeType
		{
			Json
		,UrlEncoded
		}

		private string getMediaType(MimeType mt)
		{
			switch (mt)
			{
				case MimeType.Json: return "application/json";
				case MimeType.UrlEncoded: return "application/x-www-form-urlencoded";
				default: return "text/plain";
			}
		}

		private async void AuthTest()
		{
			var body = new StringContent(JsonConvert.SerializeObject(""));
			body.Headers.ContentType = MediaTypeHeaderValue.Parse(getMediaType(MimeType.Json));
			Console.WriteLine(body.AsString());
			var response = await PostAsync(new Uri("https://slack.com/api/auth.test"), body);
			Console.WriteLine(response.AsFormattedString());


		}

		public async void PostBroadcast()
		{
			string message = await BuildBroadcastMessage();
			var channel = _options.GetBroadcastChannel();
			var json = new {channel = channel, text = message};
			var requestBody = new StringContent(JsonConvert.SerializeObject(json));
			Console.WriteLine(requestBody.AsString());
			
			requestBody.Headers.ContentType = MediaTypeHeaderValue.Parse(getMediaType(MimeType.Json));
			var response = await PostAsync(PostMessageUrl, requestBody);
			Console.WriteLine(response.StatusCode);
			Console.WriteLine(response.AsFormattedString());

		}

		private async Task<string> BuildBroadcastMessage()
		{
			var activeOooPeriods = OooPeriods.GetAllActive();
			bool usersAreOoo = activeOooPeriods.Count > 0;
			var baseMessage =
				$"Today is {DateTime.Now.ToShortDateString()} \n"
				+ $"{(usersAreOoo ? "The following users are out of office today:\n" : "No one is out of office today.")}";
			StringBuilder sb = new StringBuilder();
			string oooUserMessages = string.Empty;
			if (usersAreOoo)
			{
				for (var i = 0; i <activeOooPeriods.Count; i++)
				{
					var period = activeOooPeriods[i];
					var userName = Users.FindOrCreateUser(period.UserId).UserName;
					var startTime = period.StartTime.ToLocalTime();
					var startTimeText = "From: "
					                    + (startTime.Hour == 0
						                    ? startTime.ToShortDateString()
						                    : startTime.ToString("g", CultureInfo.CurrentCulture));
					var endTime = period.EndTime.ToLocalTime();
					var endTimeText = "To: "
					                  + (endTime.Hour == 0
						                  ? endTime.ToShortDateString()
						                  : endTime.ToString("g", CultureInfo.CurrentCulture));
					var userMessageText = $"Their message is: {period.Message}";
					if (i < activeOooPeriods.Count - 1)
					{
						sb.AppendLine(userName + " " + startTimeText + " " + endTimeText + " " + userMessageText);
					}
					else
					{
						sb.Append(userName + " " + startTimeText + " " + endTimeText + " " + userMessageText);
					}

				}
				oooUserMessages = sb.ToString();
			}

			return baseMessage + oooUserMessages;
		}

		
	}
}