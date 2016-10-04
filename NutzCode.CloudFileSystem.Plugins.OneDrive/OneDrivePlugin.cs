using System.Collections.Generic;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;
using NutzCode.CloudFileSystem.Plugins.OneDrive.Properties;

namespace NutzCode.CloudFileSystem.Plugins.OneDrive
{
    public class OneDrivePlugin : ICloudPlugin
    {

        public string Name => "One Drive";

        public byte[] Icon => Resources.Image48x48;




        public async Task<FileSystemResult<IFileSystem>> InitAsync(string fname, IOAuthProvider provider, Dictionary<string, object> settings, string userauthorization = null)
        {
            FileSystemResult<OneDriveFileSystem> r = await OneDriveFileSystem.Create(fname, provider, settings, Name, userauthorization);
            if (!r.IsOk)
                return new FileSystemResult<IFileSystem>(r.Error);
            OneDriveFileSystem f = r.Result;
            return new FileSystemResult<IFileSystem>(f);
        }

        public List<AuthorizationRequirement> AuthorizationRequirements => OAuth.AuthorizationRequirements;

    }
}
