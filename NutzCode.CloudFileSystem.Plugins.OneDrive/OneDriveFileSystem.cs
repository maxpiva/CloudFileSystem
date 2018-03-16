using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NutzCode.CloudFileSystem.OAuth2;

namespace NutzCode.CloudFileSystem.Plugins.OneDrive
{
    public class OneDriveFileSystem : OneDriveRoot, IFileSystem
    {
        internal const string OneDriveOAuth = "https://login.live.com/oauth20_token.srf";
        internal const string OneDriveOAuthLogin = "https://login.live.com/oauth20_authorize.srf";
        public string OneDriveUrl = "https://api.onedrive.com/v1.0";
        internal static List<string> OneDriveScopes = new List<string> { "wl.signin", "onedrive.readwrite", "offline_access" };

        public const string OneDriveRoot = "{0}/drive";

        internal OAuth OAuth;


        internal DirectoryCache.DirectoryCache Refs = new DirectoryCache.DirectoryCache(CloudFileSystemPluginFactory.DirectoryTreeCacheSize);

        public FileSystemResult<IObject> ResolveSynchronous(string path)
        {
            return ResolveAsync(path).Result;
        }

        public SupportedFlags Supports => SupportedFlags.Assets | SupportedFlags.SHA1 | SupportedFlags.Properties;

        private OneDriveFileSystem(IOAuthProvider provider) : base(null)
        {
            FS = this;
            OAuth = new OAuth(provider);
        }

        internal async Task<FileSystemResult> CheckExpirations()
        {
            FileSystemResult r = await OAuth.MayRefreshToken();
            if (!r.IsOk)
                return r;
            r = await OAuth.MayRefreshEndPoint();
            return r;
        }



        public string GetUserAuthorization()
        {
            return JsonConvert.SerializeObject(OAuth.Token);
        }

        public async Task<FileSystemResult<IObject>> ResolveAsync(string path)
        {
            return await Refs.ObjectFromPath(this, path);
        }

        public FileSystemSizes Sizes { get; private set; }

        public static async Task<FileSystemResult<OneDriveFileSystem>> Create(string fname, IOAuthProvider provider, Dictionary<string, object> settings, string pluginanme, string userauthorization = null)
        {
            OneDriveFileSystem am = new OneDriveFileSystem(provider);
            am.FS = am;
            am.OAuth.OAuthUrl = OneDriveOAuth;
            am.OAuth.EndPointUrl = null;
            am.OAuth.OAuthLoginUrl = OneDriveOAuthLogin;
            am.OAuth.DefaultScopes = OneDriveScopes;
            bool userauth = !string.IsNullOrEmpty(userauthorization);
            if (userauth)
                am.DeserializeAuth(userauthorization);
            FileSystemResult r = await am.OAuth.Login(settings, pluginanme, userauth,true);
            if (!r.IsOk)
                return new FileSystemResult<OneDriveFileSystem>(r.Error);
            r = await am.OAuth.MayRefreshToken();
            if (!r.IsOk)
                return new FileSystemResult<OneDriveFileSystem>(r.Error);
            r = await am.QuotaAsync();
            if (!r.IsOk)
                return new FileSystemResult<OneDriveFileSystem>(r.Error);
            r = await am.PopulateAsync();
            if (!r.IsOk)
                return new FileSystemResult<OneDriveFileSystem>(r.Error);
            return new FileSystemResult<OneDriveFileSystem>(am);
        }

        public new async Task<FileSystemResult<FileSystemSizes>> QuotaAsync() // In fact we read the drive
        {
            string url = OneDriveRoot.FormatRest(OneDriveUrl);
            FileSystemResult<dynamic> cl = await FS.OAuth.CreateMetadataStream<dynamic>(url);
            if (!cl.IsOk)
                return new FileSystemResult<FileSystemSizes>(cl.Error);
            Sizes = new FileSystemSizes
            {
                AvailableSize = cl.Result.quota.remaining ?? 0,
                TotalSize = cl.Result.quota.total ?? 0,
                UsedSize = cl.Result.quota.used ?? 0
            };
            return new FileSystemResult<FileSystemSizes>(Sizes);
        }


        public void DeserializeAuth(string auth)
        {
            OAuth.Token = JsonConvert.DeserializeObject<Token>(auth);
        }


    }
}
