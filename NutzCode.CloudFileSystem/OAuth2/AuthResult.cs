using System.Collections.Generic;

namespace NutzCode.CloudFileSystem.OAuth2
{
    public class AuthResult
    {
        public string Code { get; set; }
        public List<string> Scopes { get; set; }
        public Status Status { get; set; }
        public string ErrorString { get; set; }
    }
}