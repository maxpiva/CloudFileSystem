using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;
using NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive.Properties;

namespace NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive
{
    public class AmazonCloudPlugin : ICloudPlugin
    {

        public string Name => "Amazon Cloud Drive";
        public byte[] Icon => Resources.Image48x48;

        public const string ClientAppFriendlyName = "ClientAppFriendlyName";

        public List<AuthorizationRequirement> AuthorizationRequirements
        {
            get
            {
                    List<AuthorizationRequirement> reqs = new List<AuthorizationRequirement>(OAuth.AuthorizationRequirements);
                    reqs.Add(new AuthorizationRequirement { IsRequired = true, Name = ClientAppFriendlyName, Type = typeof(string) });
                    return reqs;
            }
        }
        
        public async Task<FileSystemResult<IFileSystem>> InitAsync(string fname, IOAuthProvider provider, Dictionary<string, object> settings, string userauthorization = null)
        {
            string frname;
            if (settings.ContainsKey(ClientAppFriendlyName) && (settings[ClientAppFriendlyName] is string))
                frname = (string) settings[ClientAppFriendlyName];
            else
                return new FileSystemResult<IFileSystem>("Unable to find " + fname + " '" + ClientAppFriendlyName + "' in settings");
            FileSystemResult<AmazonFileSystem> r = await AmazonFileSystem.Create(fname,provider, settings, Name, userauthorization);
            if (!r.IsOk)
                return new FileSystemResult<IFileSystem>(r.Error);
            r.Result.AppFriendlyName = frname;
            return new FileSystemResult<IFileSystem>(r.Result);
        }

        
    }
}
