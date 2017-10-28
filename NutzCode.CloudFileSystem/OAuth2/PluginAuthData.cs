using System;
using System.Collections.Generic;
using System.Text;

namespace NutzCode.CloudFileSystem.OAuth2
{
    public class PluginAuthData
    {
        public string LoginUri { get; set; }
        public List<string> RequiredScopes { get; set; }
        public bool ScopesCommaSeparated { get; set; }

    }
}
