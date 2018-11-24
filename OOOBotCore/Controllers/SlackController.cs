using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Newtonsoft.Json;

namespace OOOBotCore.Controllers
{
    [Produces("application/json")]
    [Route("ooo")]
    public class SlackController : Controller
    {
	    public SlackController()
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

			
		    var handler = new SlashOooHandler();
		    var result = Json(handler.HandleRequest(body).Result);
		    result.StatusCode = 200;
		    return result;

	    }
    }
}