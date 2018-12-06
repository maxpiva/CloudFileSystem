using System.Collections.Generic;
using System.Threading;
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

        public SupportedFlags Supports => SupportedFlags.Assets | SupportedFlags.SHA1 | SupportedFlags.Properties;

        private OneDriveFileSystem() : base(null)
        {
            FS = this;
            OAuth = new OAuth(OneDriveOAuth,OneDriveOAuth,null);
        }

        internal async Task<FileSystemResult> CheckExpirationsAsync(CancellationToken token = default(CancellationToken))
        {
            FileSystemResult r = await OAuth.MayRefreshTokenAsync(false, token).ConfigureAwait(false);
            if (r.Status!=Status.Ok)
                return r;
            r = await OAuth.MayRefreshEndPointAsync(token).ConfigureAwait(false);
            return r;
        }



        public string GetUserAuthorization()
        {
            return JsonConvert.SerializeObject(OAuth.Token);
        }

        public Task<IObject> ResolveAsync(string path, CancellationToken token = default(CancellationToken))
        {
            return Refs.ObjectFromPathAsync(this, path, token);
        }

        public FileSystemSizes Sizes { get; private set; }


        public static async Task<IFileSystem> CreateAsync(string fname, BaseUserSettings settings, string pluginanme, string userauthorization, CancellationToken token)
        {
            OneDriveFileSystem am = new OneDriveFileSystem();
            am.FsName = fname;
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
            if (r.Status != Status.Ok)
            {
                r.CopyErrorTo(am);
                return am;
            }
            r = await am.QuotaAsync(token).ConfigureAwait(false);
            if (r.Status != Status.Ok)
            {
                r.CopyErrorTo(am);
                return am;
            }
            r = await am.PopulateAsync(token).ConfigureAwait(false);
            if (r.Status != Status.Ok)
                r.CopyErrorTo(am);
            return am;
        }


        public override async Task<FileSystemSizes> QuotaAsync(CancellationToken token = default(CancellationToken)) // In fact we read the drive
        {
            string url = OneDriveRoot.FormatRest(OneDriveUrl);
            FileSystemResult<dynamic> cl = await FS.OAuth.CreateMetadataStreamAsync<dynamic>(url, token).ConfigureAwait(false);
            if (cl.Status!=Status.Ok)
                return new FileSystemSizes { Status = cl.Status, Error=cl.Error};
            Sizes = new FileSystemSizes
            {
                AvailableSize = cl.Result.quota.remaining ?? 0,
                TotalSize = cl.Result.quota.total ?? 0,
                UsedSize = cl.Result.quota.used ?? 0
            };
            return Sizes;
        }


        public void DeserializeAuth(string auth)
        {
            OAuth.Token = JsonConvert.DeserializeObject<Token>(auth);
        }


    }
}
