using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using NutzCode.Libraries.Web;

[assembly: InternalsVisibleTo("NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive"), InternalsVisibleTo("NutzCode.CloudFileSystem.Plugins.GoogleDrive"), InternalsVisibleTo("NutzCode.CloudFileSystem.Plugins.OneDrive")]
namespace NutzCode.CloudFileSystem.OAuth2
{
    public class OAuth 
    {
        internal Token Token;
        internal string ClientId;
        internal string ClientSecret;
        internal string UserAgent;
        internal string RedirectUri;
        private IOAuthProvider _provider;
     
        

        internal string OAuthLoginUrl;
        internal string OAuthUrl;
        internal string EndPointUrl;
        internal List<string> DefaultScopes;
        internal string DefaultUserAgent = "CloudFileSystem/1.0";
        internal string DefaultRedirectUri = "https://localhost";
        internal List<string> Scopes;

        public OAuth(IOAuthProvider provider)
        {
            _provider = provider;
        }

        internal EndPoint EndPoint;


        public const string ClientIdString = "ClientId";
        public const string ClientSecretString = "ClientSecret";
        public const string RedirectUriString = "RedirectUri";
        public const string ScopesString = "Scopes";
        public const string UserAgentString = "UserAgent";




        public static  List<AuthorizationRequirement> AuthorizationRequirements => new List<AuthorizationRequirement>
                {
                    new AuthorizationRequirement {IsRequired = true, Name = ClientIdString, Type = typeof (string)},
                    new AuthorizationRequirement {IsRequired = true, Name = ClientSecretString, Type = typeof (string)},
                    new AuthorizationRequirement {IsRequired = false, Name = RedirectUriString, Type = typeof (string)},
                    new AuthorizationRequirement {IsRequired = false, Name = ScopesString, Type = typeof (List<string>)},
                    new AuthorizationRequirement {IsRequired = false, Name = UserAgentString, Type = typeof (string)},
                };


        internal virtual async Task<FileSystemResult> Login(Dictionary<string, object> authorization, string name, bool isUserAuth, bool scopescommaseparated)
        {

            if (!authorization.ContainsKey(ClientIdString) || !(authorization[ClientIdString] is string))
                return new FileSystemResult("Unable to find "+name+" '" + ClientIdString + "' in settings");
            ClientId = (string) authorization[ClientIdString];
            if (!authorization.ContainsKey(ClientSecretString) || !(authorization[ClientSecretString] is string))
                return new FileSystemResult("Unable to find " + name + " '" + ClientSecretString + "' in settings");
            ClientSecret = (string) authorization[ClientSecretString];
            if (!authorization.ContainsKey(RedirectUriString) || !(authorization[RedirectUriString] is string))
                RedirectUri = DefaultRedirectUri;
            else
                RedirectUri = (string) authorization[RedirectUriString];
            if (!authorization.ContainsKey(ScopesString) || !(authorization[ScopesString] is List<string>))
                Scopes = DefaultScopes;
            else
                Scopes = (List<string>) authorization[ScopesString];
            if (!authorization.ContainsKey(UserAgentString) || !(authorization[UserAgentString] is string))
                UserAgent = "CloudFileSystem/1.0";
            else
                UserAgent = (string) authorization[UserAgentString];
            if (isUserAuth)
            {
                FileSystemResult r = await FillFromUserAuth();
                return r;
            }
            if (_provider==null)
                return new FileSystemResult<IFileSystem>("Cannot find valid Authorization Provider for " + name);
            AuthRequest request = new AuthRequest { Name = name, LoginUrl = OAuthLoginUrl, ClientId = ClientId, Scopes = Scopes, RedirectUri = RedirectUri , ScopesCommaSeparated = scopescommaseparated};
            AuthResult result = await _provider.Login(request);

            if (result.HasError)
                return new FileSystemResult<IFileSystem>(result.ErrorString);
            return await FillFromLogin(result.Code);
        }

        internal async Task<FileSystemResult> MayRefreshToken(bool force = false)
        {
            if (Token == null)
                return new FileSystemResult("Authorization Token not found");
            if (Token.ExpirationDate.AddMinutes(3) < DateTime.Now || force)
            {
                string refreshToken = Token.RefreshToken;
                Dictionary<string, string> postdata = new Dictionary<string, string>();
                postdata.Add("grant_type", "refresh_token");
                postdata.Add("refresh_token", Token.RefreshToken);
                postdata.Add("client_id", ClientId);
                postdata.Add("client_secret", ClientSecret);
                Token = null;
                FileSystemResult<Token> fs = await CreateMetadataStream<Token>(OAuthUrl, Encoding.UTF8.GetBytes(postdata.PostFromDictionary()));
                if (!fs.IsOk)
                    return new FileSystemResult(fs.Error);
                Token = fs.Result;
                if (string.IsNullOrEmpty(Token.RefreshToken))
                    Token.RefreshToken = refreshToken;
            }
            return new FileSystemResult();
        }
        internal async Task<FileSystemResult> MayRefreshEndPoint()
        {
            
            if (EndPoint == null || EndPoint.ExpirationDate < DateTime.Now)
            {
                FileSystemResult<EndPoint> fs = await CreateMetadataStream<EndPoint>(EndPointUrl);
                if (!fs.IsOk)
                    return new FileSystemResult(fs.Error);
                EndPoint = fs.Result;
                if (EndPoint.ContentUrl.EndsWith("/"))
                    EndPoint.ContentUrl = EndPoint.ContentUrl.Substring(0, EndPoint.ContentUrl.Length - 1);
                if (EndPoint.MetadataUrl.EndsWith("/"))
                    EndPoint.MetadataUrl = EndPoint.MetadataUrl.Substring(0, EndPoint.MetadataUrl.Length - 1);
            }
            return new FileSystemResult();
        }
        private async Task<bool> ErrorCallback(WebStream w, object p)
        {
            SeekableWebParameters swb = (SeekableWebParameters)p;
            if (w.StatusCode == HttpStatusCode.Unauthorized)
            {
                await MayRefreshToken(true);
                NameValueCollection nm = new NameValueCollection();
                nm.Add("Authorization", "Bearer " + Token.AccessToken);
                swb.Headers = nm;
                return true;
            }
            return false;
        }

        internal SeekableWebParameters CreateSeekableWebParameters(IFile file, string url, string key)
        {
            SeekableWebParameters pars = new SeekableWebParameters(new Uri(url),key,file.Size);
            if (UserAgent != null)
                pars.UserAgent = UserAgent;
            NameValueCollection nm = new NameValueCollection();
            nm.Add("Authorization", "Bearer " + Token.AccessToken);
            pars.Headers = nm;
            pars.ErrorCallback = ErrorCallback;
            return pars;
        }

        internal async Task<FileSystemResult<T>> CreateMetadataStream<T>(string url, byte[] postdata = null, string contenttype = "application/x-www-form-urlencoded", HttpMethod method = null) where T : class
        {
            bool retry = false;
            do
            {
                if (method == null)
                    method = HttpMethod.Get;
                WebParameters pars = new WebParameters(new Uri(url));
                pars.Method = method;
                if (postdata != null)
                {
                    pars.PostData = postdata;
                    pars.PostEncoding = contenttype;
                    if (method==null || method==HttpMethod.Get)
                        pars.Method = HttpMethod.Post;
                }
                if (UserAgent != null)
                    pars.UserAgent = UserAgent;

                if (Token != null)
                {
                    NameValueCollection nm = new NameValueCollection();
                    nm.Add("Authorization", "Bearer " + Token.AccessToken);
                    pars.Headers = nm;
                }
                using (WebStream w = await WebStreamFactory.Instance.CreateStreamAsync(pars))
                {
                    if ((w.StatusCode == HttpStatusCode.OK) || (w.StatusCode == HttpStatusCode.Created) || (w.StatusCode == HttpStatusCode.Accepted) || (w.StatusCode == HttpStatusCode.NoContent)) //TODO move this to a parameter
                    {
                        StreamReader rd = new StreamReader(w);
                        string d = await rd.ReadToEndAsync();
                        if (typeof(T).Name.Equals("string",StringComparison.InvariantCultureIgnoreCase))
                            return new FileSystemResult<T>((T)(object)d);
                        return new FileSystemResult<T>(JsonConvert.DeserializeObject<T>(d));
                    }
                    if (w.StatusCode != HttpStatusCode.Unauthorized || retry)
                    {
                        return new FileSystemResult<T>("'url' responds with Http code: " + w.StatusCode);
                    }
                    retry = true;
                    await MayRefreshToken(true);
                }
            } while (true);
        }

        private async Task<FileSystemResult> GetToken(string code)
        {
            Dictionary<string, string> postdata = new Dictionary<string, string>();
            postdata.Add("grant_type", "authorization_code");
            postdata.Add("code", code);
            postdata.Add("client_id", ClientId);
            postdata.Add("client_secret", ClientSecret);
            postdata.Add("redirect_uri", RedirectUri);
            FileSystemResult<Token> fs = await CreateMetadataStream<Token>(OAuthUrl, Encoding.UTF8.GetBytes(postdata.PostFromDictionary()));
            if (!fs.IsOk)
                return new FileSystemResult(fs.Error);
            Token = fs.Result;
            return new FileSystemResult();
        }
        private async Task<FileSystemResult> FillFromLogin(string code)
        {
            FileSystemResult r = await GetToken(code);
            if (!r.IsOk)
                return r;
            if (EndPointUrl != null)
            {
                r = await MayRefreshEndPoint();
                if (!r.IsOk)
                    return r;
            }
            return new FileSystemResult();
        }
        private async Task<FileSystemResult> FillFromUserAuth() 
        {

            FileSystemResult r = await MayRefreshToken();
            if (!r.IsOk)
                return r;
            if (EndPointUrl != null)
            {
                r = await MayRefreshEndPoint();
                if (!r.IsOk)
                    return r;
            }
            return new FileSystemResult();
        }

    }
}
