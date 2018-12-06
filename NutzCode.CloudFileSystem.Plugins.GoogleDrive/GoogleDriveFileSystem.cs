using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
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

        internal bool AckAbuse;

        internal OAuth OAuth;
        internal DirectoryCache.DirectoryCache Refs = new DirectoryCache.DirectoryCache(CloudFileSystemPluginFactory.DirectoryTreeCacheSize);
        public FileSystemSizes Sizes { get; private set; }

        public SupportedFlags Supports { get; } = SupportedFlags.MD5 | SupportedFlags.Assets | SupportedFlags.Properties;
        
        public GoogleDriveFileSystem() : base(null)
        {
            OAuth=new OAuth(GoogleOAuth, GoogleOAuth,null);
            FS = this;
        }

        public string GetUserAuthorization()
        {

            return JsonConvert.SerializeObject(OAuth.Token);
        }
        public Task<IObject> ResolveAsync(string path, CancellationToken token = default(CancellationToken))
        {
            return Refs.ObjectFromPathAsync(this, path, token);
        }
        public static async Task<IFileSystem> CreateAsync(string fname, BaseUserSettings settings, string pluginanme, string userauthorization, CancellationToken token = default(CancellationToken))
        {
            GoogleDriveFileSystem am = new GoogleDriveFileSystem();
            am.FsName = fname;
            am.AckAbuse = settings.AcknowledgeAbuse;
            if (string.IsNullOrEmpty(userauthorization) || (!(settings is LocalUserSettingWithCode)))
            {
                am.Status = Status.LoginRequired;
                am.Error = "Tried to login with an empty usersettings";
                return am;
            }
            if (!string.IsNullOrEmpty(userauthorization))
                am.DeserializeAuth(userauthorization);
            FileSystemResult r = await am.OAuth.InitAsync(settings, token).ConfigureAwait(false);
            if (r.Status != Status.Ok)
            {
                r.CopyErrorTo(am);
                return am;
            }
            r = await am.OAuth.MayRefreshTokenAsync(false,token).ConfigureAwait(false);
            if (r.Status!=Status.Ok)
            {
                r.CopyErrorTo(am);
                return am;
            }
            r = await am.PopulateAsync(token).ConfigureAwait(false);
            if (r.Status != Status.Ok)
                r.CopyErrorTo(am);
            return am;
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

        public override async Task<FileSystemSizes> QuotaAsync(CancellationToken token = default(CancellationToken))
        {
            FileSystemResult<ExpandoObject> cl = await FS.OAuth.CreateMetadataStreamAsync<ExpandoObject>(GoogleQuota,token).ConfigureAwait(false);
            if (cl.Status != Status.Ok)
                return new FileSystemSizes { Status = cl.Status, Error = cl.Error};
            IDictionary<string, object> dic = cl.Result;
            Sizes = new FileSystemSizes();
            if (dic.ContainsKey("quotaBytesTotal"))
                Sizes.TotalSize = ParseLong(dic["quotaBytesTotal"]);
            if (dic.ContainsKey("quotaBytesUsed"))
                Sizes.UsedSize = ParseLong(dic["quotaBytesUsed"]);
            if (dic.ContainsKey("quotaBytesUsedAggregate"))
                Sizes.AvailableSize= Sizes.TotalSize-ParseLong(dic["quotaBytesUsedAggregate"]);
            return Sizes;
        }

    }
}
