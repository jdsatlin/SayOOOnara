using System;
using System.Timers;

namespace SayOOOnara
{
	public class MessageScheduler
	{
		private readonly SlackClient _client;
		private Timer Timer;
		private DateTime _timeOfDay;
		private DateTime TimeOfDay
		{
			get
			{
				_timeOfDay = UpdateToNextOccurence(_timeOfDay);
				return _timeOfDay;
			}
			set => _timeOfDay = value;
		}
		private static readonly IOptions _options = new OptionsFile();

		

		private double MillisecondsAway => (TimeOfDay - DateTime.Now).TotalMilliseconds;

		public MessageScheduler(SlackClient client, DateTime timeOfDay)
		{
			_client = client;
			TimeOfDay = UpdateToNextOccurence(timeOfDay);
			Timer =  new Timer(MillisecondsAway);
			Timer.Elapsed += TimerOnElapsed;
			Timer.AutoReset = true;
			Timer.Enabled = true;

		}

		private void TimerOnElapsed(object sender, ElapsedEventArgs e)
		{
			var poster = _client;
			poster.PostBroadcast();
			TimeOfDay = UpdateToNextOccurence(TimeOfDay);
			Timer.Interval = MillisecondsAway;
		}

		private static DateTime UpdateToNextOccurence(DateTime input)
		{
			if (input > DateTime.Now && DetermineNextValidDayOfWeek(input) == input)
			{
				return input;
			}

			var gap = DetermineNextValidDayOfWeek(DateTime.Now.Date) - input.Date;
			return input.AddDays(gap.Days + 1);
		}

		private static DateTime DetermineNextValidDayOfWeek(DateTime input)
		{
			if (!_options.GetBroadcastDays().Contains(input.DayOfWeek.ToString().ToLower()))
			{
				DetermineNextValidDayOfWeek(input.AddDays(1));
			}

			return input;
		}

		public class SlackParser : Chronic.Core.Parser
		{
			
		}

	}
}