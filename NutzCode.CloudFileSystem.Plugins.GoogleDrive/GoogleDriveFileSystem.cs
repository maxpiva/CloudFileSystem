using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NutzCode.CloudFileSystem.OAuth2;


namespace NutzCode.CloudFileSystem.Plugins.GoogleDrive
{
    public class GoogleDriveFileSystem : GoogleDriveRoot, IFileSystem
    {
        internal const string GoogleOAuth = "https://www.googleapis.com/oauth2/v3/token";
        internal const string GoogleOAuthLogin = "https://accounts.google.com/o/oauth2/auth";
        internal static List<string> GoogleScopes = new List<string> { "https://www.googleapis.com/auth/drive" };
        internal const string GoogleQuota = "https://www.googleapis.com/drive/v2/about";

        internal bool AckAbuse = false;

        internal OAuth OAuth;
        internal DirectoryCache.DirectoryCache Refs = new DirectoryCache.DirectoryCache(CloudFileSystemPluginFactory.DirectoryTreeCacheSize);
        public FileSystemSizes Sizes { get; private set; }

        public SupportedFlags Supports { get; } = SupportedFlags.MD5 | SupportedFlags.Assets | SupportedFlags.Properties;
        
        public GoogleDriveFileSystem(IOAuthProvider provider) : base(null)
        {
            OAuth=new OAuth(provider);
        }

        public string GetUserAuthorization()
        {

            return JsonConvert.SerializeObject(OAuth.Token);
        }
        public async Task<FileSystemResult<IObject>> ResolveAsync(string path)
        {
            return await Refs.ObjectFromPath(this, path);
        }

        public static async Task<FileSystemResult<GoogleDriveFileSystem>> Create(string fname, IOAuthProvider provider, Dictionary<string, object> settings, string pluginanme, string userauthorization = null)
        {
            GoogleDriveFileSystem am = new GoogleDriveFileSystem(provider);
            am.FS = am;
            am.FsName = fname;
            am.OAuth.OAuthUrl = GoogleOAuth;
            am.OAuth.EndPointUrl = null;
            am.OAuth.OAuthLoginUrl = GoogleOAuthLogin;
            am.OAuth.DefaultScopes = GoogleScopes;
            bool userauth = !string.IsNullOrEmpty(userauthorization);
            if (userauth)
                am.DeserializeAuth(userauthorization);
            FileSystemResult r = await am.OAuth.Login(settings, pluginanme, userauth,false);
            if (!r.IsOk)
                return new FileSystemResult<GoogleDriveFileSystem>(r.Error);
            r = await am.OAuth.MayRefreshToken();
            if (!r.IsOk)
                return new FileSystemResult<GoogleDriveFileSystem>(r.Error);
            r = await am.PopulateAsync();
            if (!r.IsOk)
                return new FileSystemResult<GoogleDriveFileSystem>(r.Error);
            return new FileSystemResult<GoogleDriveFileSystem>(am);
        }

        public void DeserializeAuth(string auth)
        {
            OAuth.Token = JsonConvert.DeserializeObject<Token>(auth);
        }

        private long ParseLong(object o)
        {
            if (o is long)
                return (long) o;
            if (o is string)
                return long.Parse((string) o);
            return 0;
        }

        public async Task<FileSystemResult<FileSystemSizes>> QuotaAsync()
        {
            FileSystemResult<ExpandoObject> cl = await FS.OAuth.CreateMetadataStream<ExpandoObject>(GoogleQuota);
            if (!cl.IsOk)
                return new FileSystemResult<FileSystemSizes>(cl.Error);
            IDictionary<string, object> dic = cl.Result;
            Sizes = new FileSystemSizes();
            if (dic.ContainsKey("quotaBytesTotal"))
                Sizes.TotalSize = ParseLong(dic["quotaBytesTotal"]);
            if (dic.ContainsKey("quotaBytesUsed"))
                Sizes.UsedSize = ParseLong(dic["quotaBytesUsed"]);
            if (dic.ContainsKey("quotaBytesUsedAggregate"))
                Sizes.AvailableSize= Sizes.TotalSize-ParseLong(dic["quotaBytesUsedAggregate"]);
            return new FileSystemResult<FileSystemSizes>(Sizes);
        }

    }
}
