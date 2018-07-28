using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NutzCode.CloudFileSystem.OAuth2;



namespace NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive
{
    public class AmazonFileSystem : AmazonRoot, IFileSystem
    {

        internal const string AmazonOAuth = "https://api.amazon.com/auth/o2/token";
        internal const string AmazonEndpoint = "https://drive.amazonaws.com/drive/v1/account/endpoint";
        internal const string AmazonOAuthLogin = "https://www.amazon.com/ap/oa";
        internal const string AmazonQuota = "{0}/account/quota";

        internal static List<string> AmazonScopes = new List<string> { "clouddrive:read_all", "clouddrive:write" };
        internal string AppFriendlyName { get; set; }
        internal OAuth OAuth;
        //internal WeakReferenceContainer Refs=new WeakReferenceContainer();

        internal DirectoryCache.DirectoryCache Refs=new DirectoryCache.DirectoryCache(CloudFileSystemPluginFactory.DirectoryTreeCacheSize);

        public SupportedFlags Supports => SupportedFlags.Assets | SupportedFlags.MD5 | SupportedFlags.Properties;
        private AmazonFileSystem() : base(null)
        {
            FS = this;
            OAuth=new OAuth(AmazonOAuth, AmazonOAuth, AmazonEndpoint);
        }

        internal async Task<FileSystemResult> CheckExpirations()
        {
            FileSystemResult r = await OAuth.MayRefreshToken();
            if (r.Status != Status.Ok) 
                return r;
            return await OAuth.MayRefreshEndPoint();
        }



        public string GetUserAuthorization()
        {
            AuthorizationData dta=new AuthorizationData();
            dta.Token = OAuth.Token;
            dta.EndPoint = OAuth.EndPoint;
            return dta.Serialize();
        }

        public Task<IObject> ResolveAsync(string path)
        {
            return Refs.ObjectFromPath(this, path);
        }

        public FileSystemSizes Sizes { get; private set; }

        public static async Task<IFileSystem> Create(string fname, BaseUserSettings settings, string pluginanme, string userauthorization)
        {
            AmazonFileSystem am = new AmazonFileSystem();
            am.FsName = fname;
            am.AppFriendlyName = settings.ClientAppFriendlyName;
            if (string.IsNullOrEmpty(userauthorization) || (!(settings is LocalUserSettingWithCode)))
            {
                am.Status = Status.LoginRequired;
                am.Error = "Tried to login with an empty usersettings";
                return am;
            }
            if (!string.IsNullOrEmpty(userauthorization))
                am.DeserializeAuth(userauthorization);
            FileSystemResult r = await am.OAuth.InitAsync(settings);
            if (r.Status != Status.Ok)
            {
                r.CopyErrorTo(am);
                return am;
            }
            r = await am.CheckExpirations();
            if (r.Status != Status.Ok)
            {
                r.CopyErrorTo(am);
                return am;
            }
            string url = AmazonRoot.FormatRest(am.OAuth.EndPoint.MetadataUrl);
            FileSystemResult<dynamic> fr = await am.List(url);
            if (fr.Status != Status.Ok)
            {
                fr.CopyErrorTo(am);
                return am;
            }
            foreach (dynamic v in fr.Result)
            {
                if (v.kind == "FOLDER")
                {
                    am.SetData(JsonConvert.SerializeObject(v));
                    am.FsName = fname;
                    await am.PopulateAsync();
                    return am;
                }
            }
            am.Status = Status.NotFound;
            am.Error = "Amazon Root directory not found";
            return am;
        }

        public override async Task<FileSystemSizes> QuotaAsync()
        {
            string url = AmazonQuota.FormatRest(OAuth.EndPoint.MetadataUrl);
            FileSystemResult<Json.Quota> cl = await FS.OAuth.CreateMetadataStream<Json.Quota>(url);
            if (cl.Status != Status.Ok)
            {
                return new FileSystemSizes { Status=cl.Status, Error=cl.Error };
            }
            Sizes = new FileSystemSizes
            {
                AvailableSize = cl.Result.available,
                TotalSize = cl.Result.quota,
                UsedSize = cl.Result.quota - cl.Result.available
            };
            return Sizes;
        }


        public void DeserializeAuth(string auth)
        {
            AuthorizationData d = AuthorizationData.Deserialize(auth);
            OAuth.Token = d.Token;
            OAuth.EndPoint = d.EndPoint;
        }


    }
}
