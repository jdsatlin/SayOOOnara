using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Chronic.Core.System;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SayOOOnara;
using SayOOOnara.Persistance;

namespace SayOOOnara
{
	public class OooPeriod
	{
		public int Id { get; set; }

		private DateTime _startTime;
		public DateTime StartTime { get; set; }
		
		public bool IsCurrentlyActive => StartTime <= DateTime.UtcNow && EndTime > DateTime.UtcNow;

		public bool IsHistorical
		{
			get { return EndTime <= DateTime.UtcNow; }
			private set
			{ 
				//unfortunate hack to allow saving in entity framework
            }
		}

		public TimeSpan OooLength { get; set; }
		private string _message;
		public string Message
		{
			get => _message;
			set => _message = value.Trim();
		}
		public DateTime EndTime => StartTime + OooLength;
		public string UserId { get; set; }
		public virtual User User { get; set; }

		public bool IsActiveToday => StartTime.ToLocalTime().Date <= DateTime.Now.Date &&
		                             EndTime.ToLocalTime().Date >= DateTime.Now.Date;
		private static readonly object Lock = new object();

		private OooPeriod()
		{

		}


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
			UserId = userId;
			var utcStart = startTime.ToUniversalTime();
			StartTime = utcStart > DateTime.UtcNow ? utcStart : DateTime.UtcNow;
			var utcEnd = endTime.ToUniversalTime();
			var oooSpan = utcEnd - StartTime;

			OooLength = oooSpan;
			Message = message;

			OooPeriods.AddOooPeriod(this);
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

		public async void EndNow()
		{
			OooLength = DateTime.UtcNow - StartTime;
			await OooPeriods.Save(this);
		}


	}

	public class OooPeriods
	{
		private static readonly Dictionary<int, OooPeriod> _oooPeriods = new Dictionary<int, OooPeriod>();
		private static Dictionary<int, OooPeriod> Periods
		{
			get
			{
				var periodsToRemove = _oooPeriods.Where(p => p.Value.IsHistorical).ToList();

				if (periodsToRemove.Any())
				{
					periodsToRemove.ForEach(p => HistoricalOooPeriods[p.Key] = p.Value);
					periodsToRemove.ForEach(p => _oooPeriods.Remove(p.Key));

					var periodsToUpdate = new List<OooPeriod>();
					periodsToRemove.ForEach(p => periodsToUpdate.Add(p.Value));

					Context.UpdateRange(periodsToUpdate);
					Context.SaveChangesAsync();

				}

				return _oooPeriods;
			}
		}

		private static Dictionary<int, OooPeriod> HistoricalOooPeriods { get; } = new Dictionary<int, OooPeriod>();
		private static OooContext Context { get; set; }

		public OooPeriods()
		{
			Context = new OooContext();

		}

		public static OooPeriod GetById(int id)
		{
			return Periods[id] ?? HistoricalOooPeriods[id];
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
				Context.Periods.Update(period);
			}
			else
			{
				Periods.Remove(period.Id);
				Context.Remove(period);
			}

			Context.SaveChanges();
		}

		public static List<OooPeriod> GetAllForDay()
		{
			var activePeriods = new List<OooPeriod>();
			Periods.Where(p => p.Value.IsActiveToday).ForEach(p => activePeriods.Add(p.Value));
			return activePeriods;
		}

		public static async void AddOooPeriod(OooPeriod period)
		{
			Context.Add(period);
			await Context.SaveChangesAsync();
			Periods.Add(period.Id, period);
		}

		public async Task LoadAllOooPeriods()
		{
			var periodList = Context.Periods
				.Include(p => p.User)
				.ToList();

			periodList.ForEach(p =>
			{
				if (p.IsHistorical)
				{
					HistoricalOooPeriods.Add(p.Id, p);
				}
				else
				{
					Periods.Add(p.Id, p);
				}
			});

		}

		public async Task LoadCurrentAndFuturePeriods()
		{
			var periodList = Context.Periods.Where(p => !p.IsHistorical)
				.Include(p => p.User)
				.ToList();

			periodList.ForEach(p => Periods.Add(p.Id, p));
		}

		public static List<OooPeriod> GetUpcomingOooPeriodsByUserId(string userId)
		{
			return Periods.Values.Where(p => p.UserId == userId && p.StartTime > DateTime.UtcNow).ToList();
		}


		public static async Task Save(OooPeriod period)
		{
			if (!Periods.ContainsKey(period.Id) && !HistoricalOooPeriods.ContainsKey(period.Id))
			{
				Context.Periods.Add(period);
				if (period.IsHistorical)
				{
					HistoricalOooPeriods.Add(period.Id, period);
				}
				else
				{
					Periods.Add(period.Id, period);
				}
			}
			else
			{
				Context.Periods.Update(period);
			}

			await Context.SaveChangesAsync();
		}

	}
}