using System.Collections.Generic;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;
namespace NutzCode.CloudFileSystem
{
 
    public interface ICloudPlugin
    {
        string Name { get; }
        byte[] Icon { get; }
        PluginAuthData PluginAuthData { get; }
        Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettings settings, string userauthorization);
        Task<IFileSystem> InitAsync(string filesystemname, ProxyUserSettings settings, string userauthorization);
        Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettingWithCode settings);

    }
}
