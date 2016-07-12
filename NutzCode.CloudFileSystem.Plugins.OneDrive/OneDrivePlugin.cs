using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;

namespace NutzCode.CloudFileSystem.Plugins.OneDrive
{
    public class OneDrivePlugin
    {

        public string Name => "One Drive";

        public List<AuthorizationRequirement> AuthorizationRequirements => OAuth.AuthorizationRequirements;

        public async Task<FileSystemResult<IFileSystem>> Login(Dictionary<string, object> authorization, string userauthorization = null)
        {
            OneDriveFileSystem f = new OneDriveFileSystem();
            FileSystemResult r = await f.Login(authorization, Name, userauthorization);
            if (!r.IsOk)
                return new FileSystemResult<IFileSystem>(r.Error);
            return new FileSystemResult<IFileSystem>(f);
        }

    }
}
}
