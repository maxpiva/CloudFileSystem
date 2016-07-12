using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Windows.Forms;

namespace NutzCode.CloudFileSystem.OAuth.Windows.WinForms
{
    public partial class LoginForm : Form
    {
        public const string AuthUrl= "{4}?client_id={0}&scope={1}&response_type={2}&redirect_uri={3}";
        public string Code { get; private set; }
        public List<string> Scopes { get; private set; }=new List<string>();
        private WebBrowser webView;



        public LoginForm(string name, string authurl, string clientid, List<string> scopes, string redirect)
        {
            InitializeComponent();
            // ReSharper disable once VirtualMemberCallInConstructor
            Text = string.IsNullOrEmpty(name) ? "Login" : name;
            webView = new WebBrowser();
            webView.EncryptionLevelChanged += WebView_EncryptionLevelChanged;
            webView.Navigated += WebView_Navigated;
            webView.Navigating += WebView_Navigating;
            string responsetype = "code";
            string url = string.Format(AuthUrl, HttpUtility.UrlEncode(clientid),HttpUtility.UrlEncode(string.Join(" ",scopes)),HttpUtility.UrlEncode(responsetype),HttpUtility.UrlEncode(redirect),authurl);
            webView.Navigate(new Uri(url));
            webView.Dock=DockStyle.Fill;
            Controls.Add(webView);
        }

        private void WebView_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            CheckUrl(e.Url.ToString());
        }

        private void WebView_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            CheckUrl(e.Url.ToString());
        }

        private void WebView_EncryptionLevelChanged(object sender, EventArgs e)
        {
            CheckUrl(webView.Url.ToString());
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


    }
}