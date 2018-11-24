using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Chronic.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Newtonsoft.Json;

namespace OOOBotCore
{
	public class SlackClient
	{
	    const string address = "www.slack.com";

	    private readonly OAuthClient _oAuthClient;

	    public SlackClient()
	    {
	        _oAuthClient = new OAuthClient();
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
	}

    public class SlashOooHandler : SlashCommandReader
    {
	    private User OooUser { get; set; }
	    private OooPeriod UserOooPeriod { get; set; }

		protected override async Task ReadCommand()
        {
	        await base.ReadCommand();

			OooUser = Users.FindOrCreateUser(UserId);
	        OooUser.UserName = UserName;
	        await interpretCommandText(CommandText);
        }

	    protected override async Task<object> CreateResponse()
	    {
		    return new {text = UserOooPeriod.OooPeriodSummary()};
	    }

	    private async Task interpretCommandText(string commandText)
	    { 
		    List<string> commands = new List<string>();
		    DateTime startTime = DateTime.Now;
			var parser = new Parser();
		    if (commandText.Length > 0)
		    {
			    commandText = RemoveDoubleSpaces(commandText);
			    var spaceCount = commandText.ToCharArray().Count(c => c == ' ');
			    spaceCount = spaceCount > 3 ? 3 : spaceCount;
			    commands = commandText.Split(' ', spaceCount).ToList();

			    startTime = parser.Parse(commands[0])?.ToTime() ?? startTime;
		    }

		    switch (commands.Count)
		    {

				case 1: UserOooPeriod = new OooPeriod(UserId, startTime);
					break;
			    case 2:
			    {
				    var endTime = DateTime.MaxValue;
				    endTime = parser.Parse(commands[1])?.ToTime() ?? endTime;
				    UserOooPeriod = new OooPeriod(UserId, startTime, endTime);
				    break;
			    }
			    case 3:
			    {
				    var endTime = DateTime.MaxValue;
				    endTime = parser.Parse(commands[1])?.ToTime() ?? endTime;
				    UserOooPeriod = new OooPeriod(UserId, startTime, endTime, commands[2]);
				    break;
			    }
				default: UserOooPeriod = new OooPeriod(UserId, DateTime.Now);
					break;
			}
	    }

	   

	    private string RemoveDoubleSpaces(string text)
	    {
		    while (true)
		    {
			    if (!text.Contains("  ")) return text;
			    text = text.Replace("  ", " ");
		    }
	    }

	}

	public class SlashReturnHandler : SlashCommandReader
	{
		private User OooUser { get; set; }
	


	}

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