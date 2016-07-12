using System;
using Newtonsoft.Json;

namespace NutzCode.CloudFileSystem.OAuth2
{
    internal class EndPoint
    {
        private const long EndpointExpirationDays = 4;


        [JsonProperty("customerExists")]
        public bool CustomerExists { get; set; }
        [JsonProperty("contentUrl")]
        public string ContentUrl { get; set; }
        [JsonProperty("metadataUrl")]
        public string MetadataUrl { get; set; }
        [JsonProperty("expires")]
        public DateTime ExpirationDate { get; set; }

        public EndPoint()
        {
            ExpirationDate = DateTime.Now.AddDays(EndpointExpirationDays);
        }
    }
}
