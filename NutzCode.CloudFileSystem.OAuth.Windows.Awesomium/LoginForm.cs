using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Windows.Forms;
using Awesomium.Core;

namespace NutzCode.CloudFileSystem.OAuth.Windows.Awesomium
{
    public partial class LoginForm : Form
    {
        public const string AuthUrl= "{4}?client_id={0}&scope={1}&response_type={2}&redirect_uri={3}";
        public string Code { get; private set; }
        public List<string> Scopes { get; private set; }=new List<string>();
        private WebView webView;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (webView == null)
                return;
            webView.ParentWindow = Handle;
        }

        public LoginForm(string name, string authurl, string clientid, List<string> scopes, string redirect)
        {
            InitializeComponent();
            // ReSharper disable once VirtualMemberCallInConstructor
            Text = string.IsNullOrEmpty(name) ? "Login" : name;
            webView = WebCore.CreateWebView(ClientSize.Width, ClientSize.Height, WebViewType.Window);
            webView.AddressChanged += WebView_AddressChanged;
            webView.LoadingFrame += WebView_LoadingFrame;
            webView.TargetURLChanged += WebView_TargetURLChanged;
            webView.CertificateError += WebView_CertificateError;
            webView.LoadingFrameFailed += WebView_LoadingFrameFailed;
            string responsetype = "code";
            string url = string.Format(AuthUrl, HttpUtility.UrlEncode(clientid),HttpUtility.UrlEncode(string.Join(" ",scopes)),HttpUtility.UrlEncode(responsetype),HttpUtility.UrlEncode(redirect),authurl);
            webView.Source=new Uri(url);
        }

        private void WebView_LoadingFrameFailed(object sender, LoadingFrameFailedEventArgs e)
        {
            CheckUrl(e.Url.ToString());
        }

        private void WebView_CertificateError(object sender, CertificateErrorEventArgs e)
        {
            CheckUrl(e.Url.ToString());
        }

        private void WebView_TargetURLChanged(object sender, UrlEventArgs e)
        {
            CheckUrl(e.Url.ToString());
        }


        private void WebView_LoadingFrame(object sender, LoadingFrameEventArgs e)
        {
            CheckUrl(e.Url.ToString());
        }

        private void CheckUrl(string url)
        {
            if (url.Contains("code="))
            {
                int a = url.IndexOf("code=", StringComparison.Ordinal);
               
                string n = url.Substring(a);
                if (n.EndsWith("/"))
                    n = n.Substring(0, n.Length - 1);
                NameValueCollection col = HttpUtility.ParseQueryString(n);
                foreach (string s in col.Keys)
                {
                    switch (s)
                    {
                        case "code":
                            Code = col[s];
                            break;
                        case "scope":
                            Scopes = col[s].Split(' ').ToList();
                            break;
                    }
                }
                DialogResult = (Code != string.Empty) ? DialogResult.OK : DialogResult.Cancel;
                Close();
            }
        }


        private void WebView_AddressChanged(object sender, UrlEventArgs e)
        {
            CheckUrl(e.Url.ToString());
        }
    }
}