using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Chronic.Core.System;
using Newtonsoft.Json;
using SayOOOnara;

namespace SayOOOnara
{
	public class OooPeriod
	{
		private DateTime _startTime;
		public DateTime StartTime
		{
			get => _startTime;
			private set => _startTime = value < DateTime.UtcNow ? DateTime.UtcNow : value;
		}
		
		[JsonIgnore]
		public bool IsCurrentlyActive => StartTime <= DateTime.UtcNow && EndTime > DateTime.UtcNow;


		public TimeSpan OooLength { get; set; }
		private string _message;
		public string Message
		{
			get => _message;
			set => _message = value.Trim();
		}
		[JsonIgnore]
		public DateTime EndTime => StartTime + OooLength;
		public int Id { get; }
		[JsonIgnore]
		private static int NextId { get; set; }
		public string UserId { get; }
		[JsonIgnore]
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

			OooPeriods.AddOooPeriod(this);
		}

		
		/// <summary>
		/// For use with direct loads only. ID should set by constructor for all new periods.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="startTime"></param>
		/// <param name="endTime"></param>
		/// <param name="message"></param>
		/// <param name="id"></param>
		[JsonConstructor]
		public OooPeriod(string userId, DateTime startTime, DateTime endTime, string message, int id)
		{
			lock (Lock)
			{
				NextId = NextId > id ? NextId : id + 1;
			}

			Id = id;
			UserId = userId;
			StartTime = startTime;
			OooLength = endTime - StartTime;
			Message = message;
			
		}

		public string OooPeriodSummary()
		{
			var startTime = StartTime.ToLocalTime();
			var endTime = StartTime.ToLocalTime() + OooLength;
			var message = Message;
			bool periodInEffect = startTime <= DateTime.Now && endTime >= DateTime.Now;

			return $"You {(periodInEffect ? "have been" : "will be")} marked Out of Office beginning" +
			       $" {(startTime.Date == DateTime.Today ? startTime.ToString("t", CultureInfo.CurrentCulture) : startTime.Hour == 0 ? startTime.ToShortDateString() : startTime.ToString("g", CultureInfo.CurrentCulture))}" +
			       $" {(endTime.Year == DateTime.MaxValue.Year ? "with no return date set" : "and returning " + (endTime.Hour == 0 ? endTime.ToShortDateString() : endTime.ToString("g", CultureInfo.CurrentCulture)))}.\n" +
			       $" You{(string.IsNullOrWhiteSpace(message) ? " do not have an an out of office message." : "r out of office message is: " + message)}";
		}

		public void EndNow()
		{
			OooLength = DateTime.UtcNow - StartTime;
			OooPeriods.ForceSave();
		}


	}

	public class OooPeriods
	{
		private static readonly Dictionary<int, OooPeriod> _oooPeriods = new Dictionary<int, OooPeriod>();
		private static Dictionary<int, OooPeriod> Periods
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
		private static IStorage<OooPeriod> _storageProvider;

		public OooPeriods(IStorage<OooPeriod> storageProvider)
		{
			_storageProvider = storageProvider;
		}

		public static OooPeriod GetById(int id)
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
				period.OooLength = DateTime.UtcNow - period.StartTime;
				HistoricalOooPeriods.Add(period.Id, period);
			}

			Periods.Remove(periodId);

			ForceSave();
		}

		public static List<OooPeriod> GetAllActive()
		{
			var activePeriods = new List<OooPeriod>();
			Periods.Where(p => p.Value.IsCurrentlyActive).ForEach(p => activePeriods.Add(p.Value));
			return activePeriods;
		}

		public static void AddOooPeriod(OooPeriod period)
		{
			Periods.Add(period.Id, period);
			ForceSave();
		}

		public async Task LoadOooPeriods()
		{
			var periodList = await _storageProvider.GetAll();
			periodList.ForEach(p =>
			{
				if (p.EndTime <= DateTime.UtcNow)
					HistoricalOooPeriods.Add(p.Id, p);
				else
					Periods.Add(p.Id, p);
			});
		}

		public static List<OooPeriod> GetUpcomingOooPeriodsByUserId(string userId)
		{
			return Periods.Values.Where(p => p.UserId == userId && p.StartTime > DateTime.UtcNow).ToList();
		}

		public static async Task ForceSave()
		{
			var completeList = new List<OooPeriod>();
			completeList.AddRange(Periods.Select(p => p.Value));
			completeList.AddRange(HistoricalOooPeriods.Select(p => p.Value));
			_storageProvider.SaveAll(completeList);
		}
	}
}