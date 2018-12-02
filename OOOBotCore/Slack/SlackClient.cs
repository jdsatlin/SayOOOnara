using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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

	public interface ISlackClient : IDisposable
	{
		Task DeletePeriod(int periodId, Uri responseUri);
		Task UpdateLastMessage();


	}
	public class SlackClient : HttpClient, ISlackClient
	{
	    private readonly OAuthClient _oAuthClient;
		private string _authToken => _oAuthClient.AuthToken;
		private const string PostMessageUrl = "https://slack.com/api/chat.postMessage";
		private const string updateMessageUrl = "https://slack.com/api/chat.update";
		private static IOptions _options;
		private static string LastMessageIdentifier { get; set; }
		private static string MessageChannel { get; set; }



		public SlackClient()
	    {
	        _oAuthClient = new OAuthClient();
			DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
			_options = new OptionsFile();
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

		private async Task AuthTest()
		{
			var body = new StringContent(JsonConvert.SerializeObject(""));
			body.Headers.ContentType = MediaTypeHeaderValue.Parse(getMediaType(MimeType.Json));
			var response = await PostAsync(new Uri("https://slack.com/api/auth.test"), body);


		}

		public async Task PostBroadcast()
		{
			string message = await BuildBroadcastMessage();
			var channel = _options.GetBroadcastChannel();
			var json = new {channel = channel, text = message};
			var requestBody = new StringContent(JsonConvert.SerializeObject(json));
			requestBody.Headers.ContentType = MediaTypeHeaderValue.Parse(getMediaType(MimeType.Json));

			var response = await PostAsync(PostMessageUrl, requestBody);

			await SetLastMessageIdentifier(await response.Content.ReadAsStringAsync());
		}

		private async Task SetLastMessageIdentifier(string postBody)
		{

			dynamic responseBody = JsonConvert.DeserializeObject<ExpandoObject>(postBody);

			LastMessageIdentifier = responseBody.ts.ToString();
			MessageChannel = responseBody.channel;

		}

		public async Task UpdateLastMessage()
		{
			if (string.IsNullOrWhiteSpace(LastMessageIdentifier))
			{
				return;
			}
			string message = await BuildBroadcastMessage();
			var json = new {channel = MessageChannel, text = message, ts = LastMessageIdentifier};
			var requestBody = new StringContent(JsonConvert.SerializeObject(json));
			requestBody.Headers.ContentType = MediaTypeHeaderValue.Parse(getMediaType(MimeType.Json));

			var response = await PostAsync(updateMessageUrl, requestBody);

			var responseBody = await response.Content.ReadAsStringAsync();

			Console.WriteLine(responseBody);

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


		public async Task DeletePeriod(int periodId, Uri responseUri)
		{
			var period = OooPeriods.GetById(periodId);
			OooPeriods.RemoveOooPeriodByPeriodId(periodId);
			var messageText =
				$"Your upcoming out of office period beginning {period.StartTime.ToLocalTime().ToShortDateString()}," +
				$" and ending {period.EndTime.ToLocalTime().ToShortDateString()} has been cancelled.";
			var requestBody = new StringContent(JsonConvert.SerializeObject(new {text = messageText}));
			requestBody.Headers.ContentType = MediaTypeHeaderValue.Parse(getMediaType(MimeType.Json));
			var response = await PostAsync(responseUri, requestBody);

		}
	}
}