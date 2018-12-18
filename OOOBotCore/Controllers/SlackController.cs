using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SayOOOnara.Controllers
{
    [Produces("application/json")]
    public class OooController : Controller
    {
	    private ISlackClient SlackClient { get; }
	    public OooController(ISlackClient slackClient)
	    {
		    SlackClient = slackClient;
	    }

	    [HttpPost]
	    [Produces("application/json")]
		public JsonResult SlashOoo()
	    {
		    Request.EnableRewind();
		    Request.Body.Position = 0;
		    string body;
		    using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
		    {
			    body = reader.ReadToEnd();
		    }

		    var handler = new SlashOooHandler(body, SlackClient);
		    var result = Json(handler.HandleRequest().Result);
		    result.StatusCode = (int?) HttpStatusCode.OK;
			return result;

	    }

		[HttpPost]
		public ContentResult Return()
		{
			Request.EnableRewind();
			Request.Body.Position = 0;
			string body;
			using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
			{
				body = reader.ReadToEnd();
			}

			var handler = new SlashReturnHandler(body, SlackClient);
			var result = Content(JsonConvert.SerializeObject(handler.HandleRequest().Result), "application/json");
			result.StatusCode = (int?) HttpStatusCode.OK;
		
			return result;

		}

		[HttpPost]
		[Produces("application/json")]
		public JsonResult Interactive()
	    {
			Request.EnableRewind();
			Request.Body.Position = 0;
			string body;
			using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
			{
				body = reader.ReadToEnd();
			}

		    var handler = new InteractiveMessageDispatcher(body);
		    var result = Json(handler.Dispatch().Result);
			
		    result.StatusCode = (int?) HttpStatusCode.OK;

		    return result;
	    }



	}
}