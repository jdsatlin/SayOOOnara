using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace OOOBotCore
{
    public interface IOptions
    {
        string GetClientId();

        string GetClientSecret();

        string GetAuthToken();



    }
	public class OptionsFile : IOptions
	{
	    public OptionsFile()
	    {
            LoadOptions();
	    }

		private static string BaseDirectory
		{
			get
			{
				var directory = AppDomain.CurrentDomain.BaseDirectory;
				if (directory.Contains("bin"))
				{
					directory = Path.GetFullPath(Path.Combine(BaseDirectory, @"..\..\"));
				}

				return directory;
			}
		}

	    private static string OptionsFileLocation => BaseDirectory + @"Options.Config";

        private string _clientId;
	    private string _clientSecret;
	    private string _authToken;


		public void LoadOptions()
		{
            if (!File.Exists(OptionsFileLocation))
            {
                throw new InvalidOperationException("Options.Config file could not be acccessed in root directory.");
            }

            var savedOptions = XDocument.Load(OptionsFileLocation);
		    var json = JsonConvert.SerializeXNode(savedOptions);
		    dynamic option = JsonConvert.DeserializeObject<ExpandoObject>(json);

		    _clientId = option.ClientID ?? string.Empty;
		    _clientSecret = option.ClientSecret ?? string.Empty;
		    _authToken = option.AuthToken ?? string.Empty;

		}

	    public string GetClientId()
	    {
	        return _clientId;
	    }

	    public string GetClientSecret()
	    {
	        return _clientSecret;
	    }

	    public string GetAuthToken()
	    {
	        return _authToken;
	    }
	}
}