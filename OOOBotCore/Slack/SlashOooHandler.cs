using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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
		const string MMDDRegexPattern = "(^[\\d]{1,2})?\\/([\\d]{1,2}$)";

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
				? "Sorry, I had some trouble understanding that. Please try again and be specific, using commas to separate your start, return, and message. \n"
			    + "Or, just type /ooo with no parameters to mark yourself out now and back in tomorrow. \n"
				+ "Example: /ooo today at 3pm, next friday, On vacation with limited access to email."
				: UserOooPeriod.OooPeriodSummary()};
		}

		private async Task InterpretCommandText(string commandText)
		{ 
			List<string> commands = new List<string>();
			DateTime startTime = DateTime.Now;
			Console.WriteLine(CultureInfo.CurrentCulture.Name);
			var parser = new Parser();
			
			var regex = new Regex(MMDDRegexPattern);
			if (commandText.Length > 0)
			{
				commandText = RemoveDoubleSpaces(commandText);
				var commaCount = commandText.ToCharArray().Count(c => c == ',') + 1;
				commaCount = commaCount > 3 ? 3 : commaCount;
				commands = commandText.Split(',', commaCount).ToList();

				startTime = regex.IsMatch(commands[0].Trim())
					? MMDDConverter(commands[0])
					: parser.Parse(commands[0])?.Start ?? startTime;

				if (startTime < DateTime.Today)
				{
					_difficultyParsing = true;
					return;
				}


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

					if (startTimeRangeCheck.Start != null
					    && startTimeRangeCheck.End != null 
					    && startTimeRangeCheck.Width > SecondsInADay
					    && !regex.IsMatch(commands[0].Trim()))
					{
						UserOooPeriod = new OooPeriod(UserId, 
							(DateTime) startTimeRangeCheck.Start, (DateTime) startTimeRangeCheck.End);
					}
					else
					{
						var automaticEndTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, 0, 0, 0).AddDays(1);
						UserOooPeriod = new OooPeriod(UserId, startTime, automaticEndTime);
					}
					break;
				case 2:
				{
					var endTime = regex.IsMatch(commands[1].Trim()) 
						? MMDDConverter(commands[1]) > startTime 
						  ? MMDDConverter(commands[1]) : MMDDConverter(commands[1]).AddYears(1)
						: parseEndTime(commands[1]);
					if (endTime < startTime)
					{
						_difficultyParsing = true;
						return;
					}
					UserOooPeriod = new OooPeriod(UserId, startTime, endTime);
					break;
				}
				case 3:
				{
					var endTime = regex.IsMatch(commands[1].Trim())
						? MMDDConverter(commands[1]) > startTime
							? MMDDConverter(commands[1]) : MMDDConverter(commands[1]).AddYears(1)
						: parseEndTime(commands[1]);
					if (endTime < startTime)
					{
						_difficultyParsing = true;
						return;
					}
						UserOooPeriod = new OooPeriod(UserId, startTime, endTime, commands[2]);
					break;
				}
				default: UserOooPeriod = new OooPeriod(UserId, DateTime.Now,
						new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0).AddDays(1));
					break;
			}
		}

		private DateTime parseEndTime(string input)
		{
			var parser = new Parser();
			
			return parser.Parse(input)?.Start ?? DateTime.MinValue;

		}

		private DateTime MMDDConverter(string input)
		{
			var tempDate = input.Trim() + "/" + $"{DateTime.Today.Year}";
			if (DateTime.TryParse(tempDate, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var date))
			{
				date = date < DateTime.Today ? date.AddYears(1) : date;
			}

			return date;
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