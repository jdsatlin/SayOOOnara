using System;
using System.Collections.Generic;
using System.Linq;

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
		public string Message { get; private set; }
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

			OooPeriods.Periods.Add(this);
		}

		public string OooPeriodSummary()
		{
			var startTime = StartTime.ToLocalTime();
			var endTime = StartTime.ToLocalTime() + OooLength;
			var message = Message;
			bool periodInEffect = startTime <= DateTime.Now && endTime >= DateTime.Now;

			return $"You {(periodInEffect ? "have been" : "will be")} marked Out of Office beginning" +
			       $" {(startTime.Date == DateTime.Today.Date ? "now" : StartTime.ToShortDateString())}" +
			       $" {(endTime == DateTime.MaxValue ? "with no return date set." : "and returning " + endTime.ToShortDateString())}.\n" +
			       $" You{(string.IsNullOrWhiteSpace(message) ? " do not have an an out of office message." : "r out of office message is: " + message)}";
		}


	}

	public static class OooPeriods
	{
		private static readonly List<OooPeriod> _oooPeriods = new List<OooPeriod>();
		public static List<OooPeriod> Periods
		{
			get
			{
				HistoricalOooPeriods.AddRange(_oooPeriods.Where(p => p.EndTime < DateTime.UtcNow));
				_oooPeriods.RemoveAll(p => p.EndTime < DateTime.UtcNow);

				return _oooPeriods;
			}
		}

		private static List<OooPeriod> HistoricalOooPeriods { get; } = new List<OooPeriod>();

		static OooPeriod GetById(int id)
		{
			return Periods.Find(p => p.Id == id);
		}

		public static List<OooPeriod> GetByUserId(string userId)
		{
			return Periods.FindAll(p => p.UserId == userId);
		}
	}
}