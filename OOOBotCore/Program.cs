using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SayOOOnara.Persistance;

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

		    var context = new OooContext();

		    if (!context.Database.CanConnect())
		    {
			    try
			    {
				    context.Database.Migrate();
				}
			    catch (Exception e)
			    {
				    Console.WriteLine("Unable to migrate the database. If this is your first run, please run SayOOOnara under administrative permisions.");
					Console.WriteLine(e);
			    }
				
		    }
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
				.Build();
    }
}
