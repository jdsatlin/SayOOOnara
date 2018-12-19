using System.Threading.Tasks;

namespace SayOOOnara
{
	public class InteractiveMessageDispatcher : InteractiveMessageReader
	{
		private ISlackClient SlackClient { get; set; }

		public InteractiveMessageDispatcher(string postBody, ISlackClient slackClient)
			:base(postBody)
		{
			SlackClient = slackClient;
		}

		public async Task<object> Dispatch()
		{
			await ReadCommand();
			switch (CallbackId)
			{
				case "cancelperiod":
					var handler = new DeleteButtonHandler(Actions, SlackClient);
					return await handler.HandleRequest();
				default:
					return new { };

			}
			
		}
	}
}