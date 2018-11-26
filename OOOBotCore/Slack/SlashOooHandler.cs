using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chronic.Core;

namespace OOOBotCore
{
	public class SlashOooHandler : SlashCommandReader
	{
		private User OooUser { get; set; }
		private OooPeriod UserOooPeriod { get; set; }

		protected override async Task ReadCommand()
		{
			await base.ReadCommand();

			OooUser = Users.FindOrCreateUser(UserId);
			OooUser.UserName = UserName;
			await InterpretCommandText(CommandText);
		}

		protected override async Task<object> CreateResponse()
		{
			return new {text = UserOooPeriod.OooPeriodSummary()};
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
				var spaceCount = commandText.ToCharArray().Count(c => c == ' ') + 1;
				spaceCount = spaceCount > 3 ? 3 : spaceCount;
				commaCount = commaCount > 3 ? 3 : commaCount;
				commands = commaCount > 0
					? commandText.Split(',', commaCount).ToList()
					: commandText.Split(' ', spaceCount).ToList();

				startTime = parser.Parse(commands[0])?.ToTime() ?? startTime;
			}

			switch (commands.Count)
			{

				case 1: UserOooPeriod = new OooPeriod(UserId, startTime);
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
				default: UserOooPeriod = new OooPeriod(UserId, DateTime.Now);
					break;
			}
		}

		private DateTime parseEndTime(string input)
		{
			var parser = new Chronic.Core.Parser();
			var endTime = DateTime.MaxValue;
			endTime = parser.Parse(input)?.ToTime() ?? endTime;
			return endTime = endTime < DateTime.Now ? DateTime.Now : endTime;

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