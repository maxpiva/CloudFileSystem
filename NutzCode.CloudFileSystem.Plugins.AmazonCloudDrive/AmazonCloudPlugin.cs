using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;
using NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive.Properties;

namespace NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive
{
    public class AmazonCloudPlugin : ICloudPlugin
    {

        public string Name => "Amazon Cloud Drive";
        public byte[] Icon => Resources.Image48x48;

        public PluginAuthData PluginAuthData => new PluginAuthData {LoginUri = AmazonFileSystem.AmazonOAuthLogin, RequiredScopes = AmazonFileSystem.AmazonScopes, ScopesCommaSeparated = false};

     
        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettings settings, string userauthorization)
        {
            return AmazonFileSystem.Create(filesystemname, settings, Name, userauthorization);
        }

        public Task<IFileSystem> InitAsync(string filesystemname, ProxyUserSettings settings, string userauthorization)
        {
            return AmazonFileSystem.Create(filesystemname, settings, Name, userauthorization);
        }
        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettingWithCode settings)
        {
            return AmazonFileSystem.Create(filesystemname, settings, Name, null);
        }


    }
}
