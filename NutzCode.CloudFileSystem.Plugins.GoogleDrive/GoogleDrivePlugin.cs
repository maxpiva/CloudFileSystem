using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;
using NutzCode.CloudFileSystem.Plugins.GoogleDrive.Properties;

namespace NutzCode.CloudFileSystem.Plugins.GoogleDrive
{
    public class GoogleDrivePlugin : ICloudPlugin
    {

        public string Name => "Google Drive";
        public Bitmap Icon => Resources.Image48x48;
        public const string AcknowledgeAbuse = "AcknowledgeAbuse";

        public List<AuthorizationRequirement> AuthorizationRequirements
        {
            get
            {
                List<AuthorizationRequirement> reqs=new List<AuthorizationRequirement>(OAuth.AuthorizationRequirements);
                reqs.Add(new AuthorizationRequirement {  IsRequired = false,Name=AcknowledgeAbuse,Type=typeof(bool)});
                return reqs;
            }
        }




        public async Task<FileSystemResult<IFileSystem>> Init(string fname, IOAuthProvider provider, Dictionary<string, object> settings, string userauthorization = null)
        {
            FileSystemResult<GoogleDriveFileSystem> r = await GoogleDriveFileSystem.Create(fname, provider, settings, Name, userauthorization);
            if (!r.IsOk)
                return new FileSystemResult<IFileSystem>(r.Error);
            GoogleDriveFileSystem f = r.Result;

            if (settings.ContainsKey(AcknowledgeAbuse) && (settings[AcknowledgeAbuse] is bool))
                f.AckAbuse = (bool)settings[AcknowledgeAbuse];
            return new FileSystemResult<IFileSystem>(f);
        }        
    }
}
