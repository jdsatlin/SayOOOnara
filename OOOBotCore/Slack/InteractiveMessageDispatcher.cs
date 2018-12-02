using System.Threading.Tasks;

namespace SayOOOnara
{
	public class InteractiveMessageDispatcher : InteractiveMessageReader
	{

		public InteractiveMessageDispatcher(string postBody)
			:base(postBody)
		{
			
		}

		public async Task<object> Dispatch()
		{
			await ReadCommand();
			switch (CallbackId)
			{
				case "cancelperiod":
					var handler = new DeleteButtonHandler(Actions);
					return await handler.HandleRequest();
				default:
					return new { };

			}
			
		}
	}
}