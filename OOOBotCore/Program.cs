using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

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
