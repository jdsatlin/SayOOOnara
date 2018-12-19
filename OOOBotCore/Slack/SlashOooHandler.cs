using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chronic.Core;

namespace SayOOOnara
{
	public class SlashOooHandler : SlashCommandReader
	{
		private User OooUser { get; set; }
		private OooPeriod UserOooPeriod { get; set; }
		private const int SecondsInADay = 86400;
		private ISlackClient SlackClient { get; }
		private bool _difficultyParsing;

		public SlashOooHandler(string postBody, ISlackClient slackClient)
		: base(postBody)
		{
			SlackClient = slackClient;
		}

		public async Task<object> HandleRequest()
		{
			await ReadCommand();
			await InterpretCommandText(CommandText);
			if (!_difficultyParsing && UserOooPeriod.StartTime.ToLocalTime().Date == DateTime.Now.Date)
			{
				await SlackClient.UpdateLastMessage();
			}
			return await CreateResponse();
		}

		protected override async Task ReadCommand()
		{
			await base.ReadCommand();

			OooUser = await Users.FindOrCreate(UserId, UserName);
		}

		protected async Task<object> CreateResponse()
		{
			return new {text = _difficultyParsing 
				? "Sorry, I had some trouble understanding that. Please try again using commas to separate your beginning, end, and message"
			    + " or just type /ooo with no parameters to mark yourself out now and back in tomorrow"
				: UserOooPeriod.OooPeriodSummary()};
		}

		private async Task InterpretCommandText(string commandText)
		{ 
			List<string> commands = new List<string>();
			DateTime startTime = DateTime.Now;
			var parser = new Parser();
			if (commandText.Length > 0)
			{
				commandText = RemoveDoubleSpaces(commandText);
				var commaCount = commandText.ToCharArray().Count(c => c == ',') + 1;
				commaCount = commaCount > 3 ? 3 : commaCount;
				commands = commandText.Split(',', commaCount).ToList();

				startTime = parser.Parse(commands[0])?.Start ?? startTime;
			}

			switch (commands.Count)
			{

				case 1:
					var startTimeRangeCheck = parser.Parse(commands[0]);

					if (startTimeRangeCheck == null)
					{
						_difficultyParsing = true;
						return;
					}

					if (startTimeRangeCheck?.Start != null
					    && startTimeRangeCheck?.End != null 
					    && startTimeRangeCheck?.Width > SecondsInADay)
					{
						UserOooPeriod = new OooPeriod(UserId, 
							(DateTime) startTimeRangeCheck.Start, (DateTime) startTimeRangeCheck.End);
					}
					else
					{
						UserOooPeriod = new OooPeriod(UserId, startTime,
							new DateTime(startTime.Year, startTime.Month, startTime.Day + 1, 0, 0, 0));
					}
					break;
				case 2:
				{
					var endTime = parseEndTime(commands[1]);
					UserOooPeriod = new OooPeriod(UserId, startTime, endTime);
					break;
				}
				case 3:
				{
					var endTime = parseEndTime(commands[1]);
					UserOooPeriod = new OooPeriod(UserId, startTime, endTime, commands[2]);
					break;
				}
				default: UserOooPeriod = new OooPeriod(UserId, DateTime.Now,
						new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 0, 0, 0));
					break;
			}
		}

		private DateTime parseEndTime(string input)
		{
			var parser = new Parser();
			var endTime = DateTime.MaxValue;
			endTime = parser.Parse(input)?.Start ?? endTime;
			return endTime < DateTime.Now ? DateTime.Now : endTime;

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
}