using System.Collections.Generic;

namespace NutzCode.CloudFileSystem.OAuth2
{
    public class BaseUserSettings
    {
        public string ClientId { get; set; }
        public string UserAgent { get; set; }
        public string ClientAppFriendlyName { get; set; }
        public bool AcknowledgeAbuse { get; set; }
        public List<string> Scopes { get; set; }

    }
}