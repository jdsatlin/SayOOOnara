using System;
using System.IO;
using System.Timers;
using SayOOOnara.Slack;

namespace SayOOOnara
{
	public class MessageScheduler
	{
		private readonly ISlackClient _client;
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
		private static readonly IOptions Options = new OptionsFile();

		

		private double MillisecondsAway => (TimeOfDay - DateTime.Now).TotalMilliseconds;

		public MessageScheduler(ISlackClient client, DateTime timeOfDay)
		{
			_client = client;
			TimeOfDay = UpdateToNextOccurence(timeOfDay);
			Debug.AddToDebugLog($"Doing initial timer create for {DateTime.Now + new TimeSpan(Convert.ToInt64(MillisecondsAway * TimeSpan.TicksPerMillisecond))}");
			Timer =  new Timer(MillisecondsAway);
			Timer.Elapsed += TimerOnElapsed;
			Timer.AutoReset = true;
			Timer.Enabled = true;

		}

		private async void TimerOnElapsed(object sender, ElapsedEventArgs e)
		{
			var poster = _client;
			await poster.PostBroadcast();
			Debug.AddToDebugLog("Timer elapsed, broadcast posted.");
			System.Threading.Thread.Sleep(1000);
			Timer.Interval = MillisecondsAway;
			Debug.AddToDebugLog($"Timer interval set to reoccur at {DateTime.Now + new TimeSpan(Convert.ToInt64(MillisecondsAway * TimeSpan.TicksPerMillisecond))}");
		}

		private static DateTime UpdateToNextOccurence(DateTime input)
		{
			if (input > DateTime.Now && DetermineNextValidDayOfWeek(input) == input)
			{
				return input;
			}

			var gap = DetermineNextValidDayOfWeek(DateTime.Now.Date.AddDays(1)) - input.Date;
			Debug.AddToDebugLog($"Set next timer occurence for {gap.Days} days");
			return input.AddDays(gap.Days);
		}

		private static DateTime DetermineNextValidDayOfWeek(DateTime input)
		{
			DateTime  inputOverride = DateTime.MinValue;
			if (!Options.GetBroadcastDays().Contains(input.DayOfWeek.ToString().ToLower()))
			{
				inputOverride = DetermineNextValidDayOfWeek(input.AddDays(1));
			}


			Debug.AddToDebugLog($"Determined next valid day of week is {(inputOverride != DateTime.MinValue ? inputOverride.DayOfWeek : input.DayOfWeek)}");
			return inputOverride != DateTime.MinValue ? inputOverride : input;
		}


	}
}