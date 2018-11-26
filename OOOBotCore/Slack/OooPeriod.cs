using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Chronic.Core.System;

namespace OOOBotCore
{
	public class OooPeriod
	{
		private DateTime _startTime;
		public DateTime StartTime
		{
			get => _startTime;
			private set => _startTime = value < DateTime.UtcNow ? DateTime.UtcNow : value;
		}

		public bool IsCurrentlyActive => StartTime <= DateTime.UtcNow && EndTime > DateTime.UtcNow;


		public TimeSpan OooLength { get; private set; }
		private string _message;
		public string Message
		{
			get => _message;
			set => _message = value.Trim();
		}
		public DateTime EndTime => StartTime + OooLength;
		public int Id { get; }
		private static int NextId { get; set; }
		public string UserId { get; }
		private static readonly object Lock = new object();


		public OooPeriod(string userId, DateTime startTime) 
			: this(userId, startTime, DateTime.MaxValue)
		{
		}

		public OooPeriod(string userId, DateTime startTime, DateTime endTime)
			: this(userId, startTime, endTime, string.Empty)
		{
		}

		public OooPeriod(string userId, DateTime startTime, DateTime endTime, string message)
		{
			lock (Lock)
			{
				Id = NextId;
				NextId++;
			}

			UserId = userId;
			var utcStart = startTime.ToUniversalTime();
			StartTime = utcStart;
			var utcEnd = endTime.ToUniversalTime();
			var oooSpan = utcEnd - StartTime;

			OooLength = oooSpan;
			Message = message;

			OooPeriods.Periods.Add(Id, this);
		}

		public string OooPeriodSummary()
		{
			var startTime = StartTime.ToLocalTime();
			var endTime = StartTime.ToLocalTime() + OooLength;
			var message = Message;
			bool periodInEffect = startTime <= DateTime.Now && endTime >= DateTime.Now;

			return $"You {(periodInEffect ? "have been" : "will be")} marked Out of Office beginning" +
			       $" {(startTime.Date == DateTime.Today ? startTime.ToString("t", CultureInfo.CurrentCulture) : StartTime.Hour == 0 ? StartTime.ToShortDateString() : StartTime.ToString("g", CultureInfo.CurrentCulture))}" +
			       $" {(endTime == DateTime.MaxValue ? "with no return date set." : "and returning " + (endTime.Hour == 0 ? endTime.ToShortDateString() : endTime.ToString("g", CultureInfo.CurrentCulture)))}.\n" +
			       $" You{(string.IsNullOrWhiteSpace(message) ? " do not have an an out of office message." : "r out of office message is: " + message)}";
		}


	}

	public static class OooPeriods
	{
		private static readonly Dictionary<int, OooPeriod> _oooPeriods = new Dictionary<int, OooPeriod>();
		public static Dictionary<int, OooPeriod> Periods
		{
			get
			{
				var periodsToRemove = _oooPeriods.Where(p => p.Value.EndTime < DateTime.UtcNow).ToList();
				periodsToRemove.ForEach(p => HistoricalOooPeriods[p.Key] = p.Value);
				periodsToRemove.ForEach(p => _oooPeriods.Remove(p.Key));

				return _oooPeriods;
			}
		}

		private static Dictionary<int, OooPeriod> HistoricalOooPeriods { get; } = new Dictionary<int, OooPeriod>();

		static OooPeriod GetById(int id)
		{
			return Periods[id];
		}

		public static List<OooPeriod> GetByUserId(string userId)
		{
			return Periods.Values.Where(p => p.UserId == userId).ToList();
		}

		public static void RemoveOooPeriodByPeriodId(int periodId)
		{
			var period = Periods[periodId];
			if (period.IsCurrentlyActive)
			{
				HistoricalOooPeriods.Add(period.Id, period);
			}

			Periods.Remove(periodId);
		}

		public static List<OooPeriod> GetAllActive()
		{
			var activePeriods = new List<OooPeriod>();
			Periods.Where(p => p.Value.IsCurrentlyActive).ForEach(p => activePeriods.Add(p.Value));
			return activePeriods;
		}
	}
}