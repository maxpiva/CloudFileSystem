using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;
using NutzCode.CloudFileSystem.Plugins.LocalFileSystem.Properties;

namespace NutzCode.CloudFileSystem.Plugins.LocalFileSystem
{
    public class LocalCloudPlugin : ICloudPlugin
    {
        public string Name => "Local File System";
        public Bitmap Icon => Resources.FileIcon;

        public List<AuthorizationRequirement> AuthorizationRequirements => new List<AuthorizationRequirement>();

        public async Task<FileSystemResult<IFileSystem>> Init(string fname, IOAuthProvider provider, Dictionary<string, object> authorization, string userauthorization = null)
        {
            return await LocalFileSystem.Create(fname);
        }
    }
}
