using Newtonsoft.Json;
using NutzCode.CloudFileSystem.OAuth2;

namespace NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive
{
    internal class AuthorizationData
    {
        public Token Token { get; set; }
        public EndPoint EndPoint { get; set; }

        public static AuthorizationData Deserialize(string data)
        {
            return JsonConvert.DeserializeObject<AuthorizationData>(data);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
