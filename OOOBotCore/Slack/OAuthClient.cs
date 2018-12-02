using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SayOOOnara
{
    public class OAuthClient : HttpClient
    {
        private const string OAuthAddress = "https://slack.com/oauth/authorize";
        private const string OAuthAccessAddress = "https://slack.com/api/oauth.access";
        private string _authToken;
        private readonly IOptions _options;

	    public OAuthClient()
	    {
		    _options = new OptionsFile();
	    }
        public string AuthToken
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_authToken))
                {
                    _authToken = _options.GetAuthToken();
                    if (_authToken == string.Empty)
                    {
                        throw new InvalidOperationException("Could not locate authorization token, please check that the app is installed in the workspace");
                    }
                }

                return _authToken;
            }
        }

        private string ClientId => _options.GetClientId();
        private string ClientSecret => _options.GetClientSecret();

        public async Task<string> InitialOAuthRequest()
        {
            string accessCode = "";
            var url = new Uri(OAuthAddress + "?scope=" + SetScope() + "&client_id=" + ClientId + "&redirect_uri=" + OAuthAddress);
            HttpResponseMessage response = await GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
                accessCode = responseBody["code"];
            }

            return accessCode;

        }

        public async Task<Dictionary<string, string>> ExchangeOAuthCodeForAccessToken(string accessCode)
        {

            var url = new Uri(OAuthAccessAddress);

            var authCodeBody = new KeyValuePair<string, string>("Code", accessCode);
            var clientIdBody = new KeyValuePair<string, string>("client_id", ClientId);
            var clientSecretBody = new KeyValuePair<string, string>("client_secret", ClientSecret);

            var body = new FormUrlEncodedContent(
                new List<KeyValuePair<string, string>>() {authCodeBody, clientIdBody, clientSecretBody});
            HttpResponseMessage response = await PostAsync(url, body);

            var responseBody = new Dictionary<string, string>();

            if (response.IsSuccessStatusCode)
            {
                responseBody = JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
            }

            return responseBody;

        }

        private string SetScope()
        {
            const string postToChannel = "chat:write:bot";
            const string allowSlashCommands = "commands";
            const string joinChannel = "channels:write";
            const string getChannels = "channels:read";

            string[] oAuthScopes = {postToChannel, allowSlashCommands, joinChannel, getChannels};
            return string.Join(',', oAuthScopes);
        }
    }
}