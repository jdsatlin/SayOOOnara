using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AuthenticationSchemes = Microsoft.AspNetCore.Server.HttpSys.AuthenticationSchemes;

namespace SayOOOnara
{
    public class Program
    {
	    private static IOptions _options;
		private static List<MessageScheduler> scheduledMessages = new List<MessageScheduler>();
	    private static IStorage<User> _userStorage;
	    private static IStorage<OooPeriod> _oooPeriodStorage;
	    public static OooPeriods OooPeriodCollection;
	    public static Users UserCollection;




	    public static async Task Main(string[] args)
	    {
		    _options = new OptionsFile();
			_userStorage = new JsonStorage<User>();
			_oooPeriodStorage = new JsonStorage<OooPeriod>();
			OooPeriodCollection = new OooPeriods(_oooPeriodStorage);
			UserCollection = new Users(_userStorage);

		    await OooPeriodCollection.LoadOooPeriods();
		    await UserCollection.LoadUsers();
			


		    using (var client = new SlackClient())
		    {


			    foreach (var dailypost in _options.GetBroadcastTimes())
			    {
				    var scheduledMessage = new MessageScheduler(client, dailypost);
				    scheduledMessages.Add(scheduledMessage);
			    }

			    var posted = new SlackClient();

			    BuildWebHost(args).Run();
		    }
	    }


	    public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
	            .UseUrls("http://*:6111")
				.Build();
    }
}
