using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NutzCode.CloudFileSystem.OAuth.Windows.WPF
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        public const string AuthUrl = "{4}?client_id={0}&scope={1}&response_type={2}&redirect_uri={3}";
        public string Code { get; private set; }
        public List<string> Scopes { get; private set; } = new List<string>();
        private Uri uri;
        public LoginForm(string name, string authurl, string clientid, List<string> scopes, string redirect)
        {
            InitializeComponent();
            this.Title=string.IsNullOrEmpty(name) ? "Login" : name;
            WebView.Navigated += WebView_Navigated;
            WebView.Navigating += WebView_Navigating;
            string responsetype = "code";
            string url = string.Format(AuthUrl, WebUtility.UrlEncode(clientid), WebUtility.UrlEncode(string.Join(" ", scopes)), WebUtility.UrlEncode(responsetype), WebUtility.UrlEncode(redirect), authurl);
            uri=new Uri(url);
            this.Visibility=Visibility.Visible;
        }

        private void WebView_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            CheckUrl(e.Uri.ToString());
        }

        private void WebView_Navigated(object sender, NavigationEventArgs e)
        {
            CheckUrl(e.Uri.ToString());
        }


        public static NameValueCollection ParseQueryString(string s)
        {
            NameValueCollection nvc = new NameValueCollection();

            // remove anything other than query string from url
            if (s.Contains("?"))
            {
                s = s.Substring(s.IndexOf('?') + 1);
            }

            foreach (string vp in Regex.Split(s, "&"))
            {
                string[] singlePair = Regex.Split(vp, "=");
                if (singlePair.Length == 2)
                {
                    nvc.Add(singlePair[0], singlePair[1]);
                }
                else
                {
                    // only one key with no value specified in query string
                    nvc.Add(singlePair[0], string.Empty);
                }
            }

            return nvc;
        }





        private void CheckUrl(string url)
        {
            if (url.Contains("code="))
            {
                int a = url.IndexOf("code=", StringComparison.Ordinal);

                string n = url.Substring(a);
                if (n.EndsWith("/"))
                    n = n.Substring(0, n.Length - 1);
                NameValueCollection col = ParseQueryString(n);
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
                DialogResult = Code != string.Empty;
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WebView.Navigate(uri);
        }
    }
}
