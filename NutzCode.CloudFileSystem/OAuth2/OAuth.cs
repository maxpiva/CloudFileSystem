using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using NutzCode.Libraries.Web;

[assembly: InternalsVisibleTo("NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive"), InternalsVisibleTo("NutzCode.CloudFileSystem.Plugins.GoogleDrive"), InternalsVisibleTo("NutzCode.CloudFileSystem.Plugins.OneDrive")]
namespace NutzCode.CloudFileSystem.OAuth2
{
    public class OAuth 
    {
        internal Token Token;
        internal BaseUserSettings UserSettings;
        internal EndPoint EndPoint;
        internal string TokenUri;
        internal string OAuthUri;
        internal string EndPointUri;

        public OAuth(string tokenuri, string oauthuri, string endpointuri)
        {
            TokenUri = tokenuri;
            OAuthUri = oauthuri;
            EndPointUri = endpointuri;
        }

        public OAuth(string tokenuri, string oauthuri)
        {
            TokenUri = tokenuri;
            OAuthUri = oauthuri;
            EndPointUri = null;
        }


        internal string DefaultUserAgent = "CloudFileSystem/1.0";

        internal virtual async Task<FileSystemResult> InitAsync(BaseUserSettings settings, CancellationToken token)
        {
            if (string.IsNullOrEmpty(settings.UserAgent))
                settings.UserAgent = DefaultUserAgent;
            UserSettings = settings;
            LocalUserSettingWithCode userwithcode=settings as LocalUserSettingWithCode;
            if (userwithcode != null)
            {
                FileSystemResult fs = await GetTokenAsync(userwithcode,token).ConfigureAwait(false);
                if (fs.Status != Status.Ok)
                    return fs;
            }
            FileSystemResult r = await MayRefreshTokenAsync(false, token).ConfigureAwait(false);
            if (r.Status != Status.Ok)
                return r;
            if (EndPointUri != null)
            {
                r = await MayRefreshEndPointAsync(token).ConfigureAwait(false);
                if (r.Status != Status.Ok)
                    return r;
            }
            return new FileSystemResult();
        }


        /*

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
                */
        /*
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
                */



        internal async Task<FileSystemResult> MayRefreshTokenAsync(bool force = false, CancellationToken token=default(CancellationToken))
        {
            if (Token == null)
                return new FileSystemResult(Status.LoginRequired, "Authorization Token not found");
            if (Token.ExpirationDate.AddMinutes(3) < DateTime.Now || force)
            {
                ProxyUserSettings prox=UserSettings as ProxyUserSettings;
                if (prox != null)
                {
                    //Convert To Base64Url
                    string refreshToken = Token.RefreshToken.Replace("+", "-").Replace("/", "_");
                    string uri = prox.RefreshTokenProxyUri;
                    if (uri.EndsWith("/"))
                        uri += refreshToken;
                    else
                        uri += "/" + refreshToken;
                    Token = null;
                    FileSystemResult<Token> fs = await CreateMetadataStreamAsync<Token>(uri,token).ConfigureAwait(false);
                    if (fs.Status != Status.Ok)
                        return new FileSystemResult(Status.LoginRequired, fs.Error);
                    Token = fs.Result;
                    if (string.IsNullOrEmpty(Token.RefreshToken))
                        Token.RefreshToken = refreshToken;
                }
                {
                    LocalUserSettings loc=UserSettings as LocalUserSettings;
                    string refreshToken = Token.RefreshToken;
                    Dictionary<string, string> postdata = new Dictionary<string, string>();
                    postdata.Add("grant_type", "refresh_token");
                    postdata.Add("refresh_token", Token.RefreshToken);
                    postdata.Add("client_id", loc.ClientId);
                    postdata.Add("client_secret", loc.ClientSecret);
                    Token = null;
                    FileSystemResult<Token> fs = await CreateMetadataStreamAsync<Token>(OAuthUri, token, Encoding.UTF8.GetBytes(postdata.PostFromDictionary())).ConfigureAwait(false);
                    if (fs.Status != Status.Ok)
                        return new FileSystemResult(Status.LoginRequired, fs.Error);
                    Token = fs.Result;
                    if (string.IsNullOrEmpty(Token.RefreshToken))
                        Token.RefreshToken = refreshToken;
                }
            }
            return new FileSystemResult();
        }
        internal async Task<FileSystemResult> MayRefreshEndPointAsync(CancellationToken token)
        {
            
            if (EndPoint == null || EndPoint.ExpirationDate < DateTime.Now)
            {
                FileSystemResult<EndPoint> fs = await CreateMetadataStreamAsync<EndPoint>(EndPointUri, token).ConfigureAwait(false);
                if (fs.Status != Status.Ok)
                    return fs;
                EndPoint = fs.Result;
                if (EndPoint.ContentUrl.EndsWith("/"))
                    EndPoint.ContentUrl = EndPoint.ContentUrl.Substring(0, EndPoint.ContentUrl.Length - 1);
                if (EndPoint.MetadataUrl.EndsWith("/"))
                    EndPoint.MetadataUrl = EndPoint.MetadataUrl.Substring(0, EndPoint.MetadataUrl.Length - 1);
            }
            return new FileSystemResult();
        }
        private async Task<bool> ErrorCallbackAsync(WebStream w, object p, CancellationToken token)
        {
            SeekableWebParameters swb = (SeekableWebParameters)p;
            if (w.StatusCode == HttpStatusCode.Unauthorized)
            {
                await MayRefreshTokenAsync(true,token).ConfigureAwait(false);
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
            if (UserSettings.UserAgent != null)
                pars.UserAgent = UserSettings.UserAgent;
            NameValueCollection nm = new NameValueCollection();
            nm.Add("Authorization", "Bearer " + Token.AccessToken);
            pars.Headers = nm;
            pars.ErrorCallbackParameter = pars;
            pars.ErrorCallback = ErrorCallbackAsync;
            return pars;
        }

        internal async Task<FileSystemResult<T>> CreateMetadataStreamAsync<T>(string url,  CancellationToken token, byte[] postdata = null, string contenttype = "application/x-www-form-urlencoded", HttpMethod method = null) where T : class
        {
            bool retry = false;
            do
            {
                if (method == null)
                    method = HttpMethod.Get;
                WebParameters pars = new WebParameters(new Uri(url), TokenUri);
                pars.Method = method;
                if (postdata != null)
                {
                    pars.PostData = postdata;
                    pars.PostEncoding = contenttype;
                    if (method==null || method==HttpMethod.Get)
                        pars.Method = HttpMethod.Post;
                }
                if (UserSettings.UserAgent != null)
                    pars.UserAgent = UserSettings.UserAgent;

                if (Token != null)
                {
                    NameValueCollection nm = new NameValueCollection();
                    nm.Add("Authorization", "Bearer " + Token.AccessToken);
                    pars.Headers = nm;
                }
                using (WebStream w = await WebStreamFactory.Instance.CreateStreamAsync(pars,token).ConfigureAwait(false))
                {
                    if ((w.StatusCode == HttpStatusCode.OK) || (w.StatusCode == HttpStatusCode.Created) || (w.StatusCode == HttpStatusCode.Accepted) || (w.StatusCode == HttpStatusCode.NoContent)) //TODO move this to a parameter
                    {
                        StreamReader rd = new StreamReader(w);
                        string d = await rd.ReadToEndAsync().ConfigureAwait(false);
                        if (typeof(T).Name.Equals("string",StringComparison.InvariantCultureIgnoreCase))
                            return new FileSystemResult<T>((T)(object)d);
                        return new FileSystemResult<T>(JsonConvert.DeserializeObject<T>(d));
                    }
                    if (w.StatusCode != HttpStatusCode.Unauthorized || retry)
                    {
                        return new FileSystemResult<T>(Status.HttpError, "'url' responds with Http code: " + w.StatusCode);
                    }
                    retry = true;
                    await MayRefreshTokenAsync(true,token).ConfigureAwait(false);
                }
            } while (true);
        }

        private async Task<FileSystemResult> GetTokenAsync(LocalUserSettingWithCode sets, CancellationToken token)
        {
            Dictionary<string, string> postdata = new Dictionary<string, string>();
            postdata.Add("grant_type", "authorization_code");
            postdata.Add("code", sets.Code);
            postdata.Add("client_id",sets.ClientId);
            postdata.Add("client_secret", sets.ClientSecret);
            postdata.Add("redirect_uri", sets.OriginalRedirectUri);
            FileSystemResult<Token> fs = await CreateMetadataStreamAsync<Token>(TokenUri, token, Encoding.UTF8.GetBytes(postdata.PostFromDictionary())).ConfigureAwait(false);
            if (fs.Status!=Status.Ok)
                return new FileSystemResult(Status.LoginRequired, fs.Error);
            Token = fs.Result;
            return new FileSystemResult();
        }
        /*
        public async Task<FileSystemResult> GetTokenAndEndPoints(UserAuthRequest req)
        {
            FileSystemResult r = await GetToken(req);
            if (r.Status!=Status.Ok)
                return r;
            if (EndPointUrl != null)
            {
                r = await MayRefreshEndPoint();
                if (r.Status != Status.Ok)
                    return r;
            }
            return new FileSystemResult();
        }*/
        /*
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
        */
    }
}
