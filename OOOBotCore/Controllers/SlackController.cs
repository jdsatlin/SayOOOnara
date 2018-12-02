using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Newtonsoft.Json;

namespace SayOOOnara.Controllers
{
    [Produces("application/json")]
    public class OooController : Controller
    {

	    public OooController()
	    {
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

		    var handler = new SlashOooHandler(body);
		    var result = Json(handler.HandleRequest().Result);
		    result.StatusCode = 200;
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
			var handler = new SlashReturnHandler(body);
			var result = Content(JsonConvert.SerializeObject(handler.HandleRequest().Result), "application/json");
			result.StatusCode = (int?) HttpStatusCode.OK;
		
			Console.WriteLine(result.Content);
			return result;

		}

		[HttpPost]
		[Produces("application/json")]
		public JsonResult DeletePeriod()
	    {
			Request.EnableRewind();
			Request.Body.Position = 0;
			string body;
			using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
			{
				body = reader.ReadToEnd();
			}

		     var handler = new DeleteButtonHandler(body);
		    var result = Json(handler.HandleRequest().Result);
		    result.StatusCode = (int?) HttpStatusCode.OK;

		    return result;
	    }



	}
}