using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NutzCode.CloudFileSystem.OAuth2;

namespace NutzCode.CloudFileSystem.OAuth.Windows.Awesomium
{
    public class AuthProvider : IOAuthProvider
    {
        public string Name => "Awesomium";

        public async Task<AuthResult> Login(AuthRequest request)
        {
            AuthResult r=new AuthResult();
            await Task.Factory.StartNew(() =>
            {
                LoginForm l = new LoginForm(request.Name, request.LoginUrl,request.ClientId,request.Scopes,request.RedirectUri);
                DialogResult dr = l.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    r.Code = l.Code;
                    r.Scopes = l.Scopes;
                    r.Status = Status.Ok;
                }
                else
                {
                    r.Status = Status.LoginRequired;
                    r.ErrorString = "Unable to login";
                }
            }, new CancellationToken(), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            return r;
        }
    }
}
