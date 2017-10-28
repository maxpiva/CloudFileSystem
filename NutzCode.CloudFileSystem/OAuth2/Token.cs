using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace NutzCode.CloudFileSystem.OAuth2
{
    public class Token
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        private int _expiresIn;
        [JsonProperty("expires_in")]
        public int ExpiresIn
        {
            get { return _expiresIn; }
            set
            {
                _expiresIn = value;
                ExpirationDate = DateTime.Now.AddSeconds(value);
            }
        }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires")]
        public DateTime ExpirationDate { get; set; }


    }
}
