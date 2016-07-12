using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem.Plugins.OneDrive
{
    public class OneDriveFileSystem : OAuth2.OAuth, IFileSystem
    {
        internal const string OneDrivetOAuth = "https://www.googleapis.com//oauth2/v3/token";
        internal const string GoogleOAuthLogin = "https://accounts.google.com/o/oauth2/auth";
        internal List<string> OneDriveScopes = new List<string> { "https://www.googleapis.com/auth/drive" };
        internal bool _ackAbuse = false;


        public GoogleDriveFileSystem()
        {
            _oauthurl = GoogleOAuth;
            _endpointurl = null;
            _oauthloginurl = GoogleOAuthLogin;
            _defaultscopes = GoogleScopes;
        }

        public string GetUserAuthorization()
        {

            return JsonConvert.SerializeObject(_token);
        }

        public async override Task<FileSystemResult<IDirectory>> GetRoot()
        {
            GoogleDriveRoot d = new GoogleDriveRoot(this);
            FileSystemResult r = await d.Populate();
            if (!r.IsOk)
                return new FileSystemResult<IDirectory>(r.Error);
            return new FileSystemResult<IDirectory>(d);
        }


        public override void DeserializeAuth(string auth)
        {
            _token = JsonConvert.DeserializeObject<Token>(auth);
        }

    }
}
