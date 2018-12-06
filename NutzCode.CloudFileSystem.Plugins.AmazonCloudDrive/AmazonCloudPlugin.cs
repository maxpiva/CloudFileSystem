using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;
using NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive.Properties;

namespace NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive
{
    public class AmazonCloudPlugin : ICloudPlugin
    {

        public string Name => "Amazon Cloud Drive";
        public byte[] Icon {
            get {
                using (MemoryStream ms = new MemoryStream())
                {
                    this.GetType().Assembly.GetManifestResourceStream($"{GetType().Namespace}.Resources.Image48x48png").CopyTo(ms);
                    return ms.GetBuffer();                
                }             
            }
        }
        

        public PluginAuthData PluginAuthData => new PluginAuthData {LoginUri = AmazonFileSystem.AmazonOAuthLogin, RequiredScopes = AmazonFileSystem.AmazonScopes, ScopesCommaSeparated = false};

     
        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettings settings, string userauthorization, CancellationToken token=default(CancellationToken))
        {
            return AmazonFileSystem.CreateAsync(filesystemname, settings, Name, userauthorization, token);
        }

        public Task<IFileSystem> InitAsync(string filesystemname, ProxyUserSettings settings, string userauthorization, CancellationToken token = default(CancellationToken))
        {
            return AmazonFileSystem.CreateAsync(filesystemname, settings, Name, userauthorization, token);
        }
        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettingWithCode settings, CancellationToken token = default(CancellationToken))
        {
            return AmazonFileSystem.CreateAsync(filesystemname, settings, Name, null, token);
        }


    }
}
