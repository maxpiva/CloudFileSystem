using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;
using NutzCode.CloudFileSystem.Plugins.OneDrive.Properties;

namespace NutzCode.CloudFileSystem.Plugins.OneDrive
{
    public class OneDrivePlugin : ICloudPlugin
    {

        public string Name => "One Drive";

        public byte[] Icon => Resources.Image48x48;

        public PluginAuthData PluginAuthData => new PluginAuthData { LoginUri = OneDriveFileSystem.OneDriveOAuthLogin, RequiredScopes = OneDriveFileSystem.OneDriveScopes, ScopesCommaSeparated = false };


        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettings settings, string userauthorization)
        {
            return OneDriveFileSystem.Create(filesystemname, settings, Name, userauthorization);
        }

        public Task<IFileSystem> InitAsync(string filesystemname, ProxyUserSettings settings, string userauthorization)
        {
            return OneDriveFileSystem.Create(filesystemname, settings, Name, userauthorization);
        }
        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettingWithCode settings)
        {
            return OneDriveFileSystem.Create(filesystemname, settings, Name, null);
        }

    }
}
