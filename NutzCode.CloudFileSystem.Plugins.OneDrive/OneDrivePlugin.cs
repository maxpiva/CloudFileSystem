using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;
using NutzCode.CloudFileSystem.Plugins.OneDrive.Properties;

namespace NutzCode.CloudFileSystem.Plugins.OneDrive
{
    public class OneDrivePlugin : ICloudPlugin
    {

        public string Name => "One Drive";

        public byte[] Icon {
            get {
                using (MemoryStream ms = new MemoryStream())
                {
                    this.GetType().Assembly.GetManifestResourceStream($"{this.GetType().Namespace}.Resources.Image48x48png").CopyTo(ms);
                    return ms.GetBuffer();                
                }
            }
        }

        public PluginAuthData PluginAuthData => new PluginAuthData { LoginUri = OneDriveFileSystem.OneDriveOAuthLogin, RequiredScopes = OneDriveFileSystem.OneDriveScopes, ScopesCommaSeparated = false };


        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettings settings, string userauthorization, CancellationToken token = default(CancellationToken))
        {
            return OneDriveFileSystem.CreateAsync(filesystemname, settings, Name, userauthorization, token);
        }

        public Task<IFileSystem> InitAsync(string filesystemname, ProxyUserSettings settings, string userauthorization, CancellationToken token = default(CancellationToken))
        {
            return OneDriveFileSystem.CreateAsync(filesystemname, settings, Name, userauthorization, token);
        }
        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettingWithCode settings, CancellationToken token = default(CancellationToken))
        {
            return OneDriveFileSystem.CreateAsync(filesystemname, settings, Name, null, token);
        }

    }
}
