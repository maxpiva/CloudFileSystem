using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;
using NutzCode.CloudFileSystem.Plugins.GoogleDrive.Properties;

namespace NutzCode.CloudFileSystem.Plugins.GoogleDrive
{
    public class GoogleDrivePlugin : ICloudPlugin
    {

        public string Name => "Google Drive";
        public byte[] Icon => Resources.Image48x48;

        public PluginAuthData PluginAuthData => new PluginAuthData { LoginUri = GoogleDriveFileSystem.GoogleOAuthLogin, RequiredScopes = GoogleDriveFileSystem.GoogleScopes, ScopesCommaSeparated = false };


        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettings settings, string userauthorization)
        {
            return GoogleDriveFileSystem.Create(filesystemname, settings, Name, userauthorization);
        }

        public Task<IFileSystem> InitAsync(string filesystemname, ProxyUserSettings settings, string userauthorization)
        {
            return GoogleDriveFileSystem.Create(filesystemname, settings, Name, userauthorization);
        }
        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettingWithCode settings)
        {
            return GoogleDriveFileSystem.Create(filesystemname, settings, Name, null);
        }

       
    }
}
