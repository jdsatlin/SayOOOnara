using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AuthenticationSchemes = Microsoft.AspNetCore.Server.HttpSys.AuthenticationSchemes;

namespace OOOBotCore
{
    public class Program
    {
	    private static IOptions _options;
		private static List<MessageScheduler> scheduledMessages = new List<MessageScheduler>();


	    public static void Main(string[] args)
	    {
		    _options = new OptionsFile();

		    foreach (var dailypost in _options.GetBroadcastTimes())
		    {
			    var scheduledMessage = new MessageScheduler(dailypost);
			    scheduledMessages.Add(scheduledMessage);
		    }

		    var posted = new SlackClient();

		    BuildWebHost(args).Run();
	    }


	    public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
	            .UseUrls("http://*:6111")
				.Build();
    }
}
