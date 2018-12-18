using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SayOOOnara
{
    public class Program
    {
	    private static IOptions _options;
		private static readonly List<MessageScheduler> ScheduledMessages = new List<MessageScheduler>();
	    public static OooPeriods OooPeriodCollection;
	    public static Users UserCollection;




	    public static async Task Main(string[] args)
	    {
		    _options = new OptionsFile();
			OooPeriodCollection = await OooPeriods.Create();
			UserCollection = new Users();

		    await UserCollection.LoadUsers();
			


		    using (var client = new SlackClient())
		    {


			    foreach (var dailypost in _options.GetBroadcastTimes())
			    {
				    var scheduledMessage = new MessageScheduler(client, dailypost);
				    ScheduledMessages.Add(scheduledMessage);
			    }

			    BuildWebHost(args).Run();
		    }
	    }


	    public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
	            .UseUrls($"{_options.GetBinding()}")
				.Build();
    }
}
