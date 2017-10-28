using System.Collections.Generic;

namespace NutzCode.CloudFileSystem.OAuth2
{
    public class AuthRequest
    {
        public string Name { get; set; }
        public string LoginUrl { get; set; }
        public string ClientId { get; set; }
        public List<string> Scopes { get; set; }
        public string RedirectUri { get; set; }
        public bool ScopesCommaSeparated { get; set; }
    }
}