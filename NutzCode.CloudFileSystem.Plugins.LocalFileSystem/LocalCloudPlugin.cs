using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;
using NutzCode.CloudFileSystem.Plugins.LocalFileSystem.Properties;


namespace NutzCode.CloudFileSystem.Plugins.LocalFileSystem
{
    public class LocalCloudPlugin : ICloudPlugin
    {
        public string Name => "Local File System";
        public byte[] Icon => Resources.Image48x48;

        public PluginAuthData PluginAuthData => new PluginAuthData();


        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettings settings, string userauthorization)
        {
            return LocalFileSystem.Create(string.Empty);
        }

        public Task<IFileSystem> InitAsync(string filesystemname, ProxyUserSettings settings, string userauthorization)
        {
            return LocalFileSystem.Create(string.Empty);
        }
        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettingWithCode settings)
        {
            return LocalFileSystem.Create(string.Empty);
        }

    }
}
