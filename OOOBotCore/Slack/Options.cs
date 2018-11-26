using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using Chronic.Core;
using Chronic.Core.System;
using Newtonsoft.Json;

namespace OOOBotCore
{
    public interface IOptions
    {
        string GetClientId();

        string GetClientSecret();

        string GetAuthToken();

	    List<string> GetBroadcastDays();

	    List<DateTime> GetBroadcastTimes();

	    string GetBroadcastChannel();



    }
	public class OptionsFile : IOptions
	{
	    public OptionsFile()
	    {
            LoadOptions();
	    }

		private static string BaseDirectory
		{
			get
			{
				var directory = AppDomain.CurrentDomain.BaseDirectory;

				if (directory.Contains("bin"))
				{
					directory = Path.GetFullPath(Path.Combine(directory, @"..\..\..\"));
				}

				return directory;
			}
		}

	    private static string OptionsFileLocation => BaseDirectory + @"Options.Config";

        private string _clientId;
	    private string _clientSecret;
	    private string _authToken;
		private readonly List<string> _broadcastDays = new List<string>();
		private readonly List<DateTime> _broadcastTimesUtc = new List<DateTime>();
		private string _broadcastChannel;

		public void LoadOptions()
		{
            if (!File.Exists(OptionsFileLocation))
            {
                throw new InvalidOperationException("Options.Config file could not be acccessed in root directory.");
            }

            var savedOptions = XDocument.Load(OptionsFileLocation);
		    var json = JsonConvert.SerializeXNode(savedOptions);
		    dynamic optionxml = JsonConvert.DeserializeObject<ExpandoObject>(json);
			var option = optionxml.configuration;

		    _clientId = option.ClientID ?? string.Empty;
			_clientSecret = option.ClientSecret ?? string.Empty;
		    _authToken = option.AuthToken ?? string.Empty;;
			_broadcastChannel = option.BroadcastChannel ?? string.Empty;

			string broadcastDays = option.BroadcastDays;
			_broadcastDays.AddRange(broadcastDays.ToLower().Replace(" ", "").Split(',')
				.Where(d => DaysOfTheWeek.Contains(d)));

			string[] broadcastTimes = option.BroadcastTimes.ToLower().Replace(" ", "").Split(',');
			var parser = new Parser();
			foreach (var time in broadcastTimes)
			{
				Span timeOfDay = parser.Parse(time);
				if (timeOfDay != null)
				{
					_broadcastTimesUtc.Add(timeOfDay.ToTime().ToUniversalTime());
				}
			}
		}

		private static readonly string[] DaysOfTheWeek =
		{
			"monday",
			"tuesday",
			"wednesday",
			"thursday",
			"friday",
			"saturday",
			"sunday"
		};


	    public string GetClientId()
	    {
	        return _clientId;
	    }

	    public string GetClientSecret()
	    {
	        return _clientSecret;
	    }

	    public string GetAuthToken()
	    {
	        return _authToken;
	    }

		public List<string> GetBroadcastDays()
		{
			return _broadcastDays;
		}

		public List<DateTime> GetBroadcastTimes()
		{
			return _broadcastTimesUtc.Select(t => t.ToLocalTime()).ToList();
		}

		public string GetBroadcastChannel()
		{
			return _broadcastChannel;
		}
	}

}