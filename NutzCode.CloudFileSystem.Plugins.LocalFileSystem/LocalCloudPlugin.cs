using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem;
using NutzCode.CloudFileSystem.OAuth2;
using NutzCode.CloudFileSystem.Plugins.LocalFileSystem.Properties;
using Path = Pri.LongPath.Path;
using Directory = Pri.LongPath.Directory;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;
using File = Pri.LongPath.File;
using FileSystemInfo = Pri.LongPath.FileSystemInfo;
using FileInfo = Pri.LongPath.FileInfo;
using Stream = System.IO.Stream;

namespace NutzCode.CloudFileSystem.Plugins.LocalFileSystem
{
    public class LocalCloudPlugin : ICloudPlugin
    {
        public string Name => "Local File System";
        public byte[] Icon => Resources.Image48x48;

        public List<AuthorizationRequirement> AuthorizationRequirements => new List<AuthorizationRequirement>();

        public async Task<FileSystemResult<IFileSystem>> InitAsync(string fname, IOAuthProvider provider, Dictionary<string, object> authorization, string userauthorization = null)
        {
            return await LocalFileSystem.Create(string.Empty);
        }
    }
}
