﻿using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NutzCode.CloudFileSystem.OAuth2;


namespace NutzCode.CloudFileSystem.OAuth.Windows.WinForms
{
    public class AuthProvider : IOAuthProvider
    {
        public string Name => "WinForms";

        public async Task<AuthResult> LoginAsync(AuthRequest request, CancellationToken token=default(CancellationToken))
        {
            AuthResult r = new AuthResult();
            await Task.Factory.StartNew(() =>
            {
                LoginForm l = new LoginForm(request.Name, request.LoginUrl, request.ClientId, request.Scopes, request.RedirectUri);
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
            }, token, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext()).ConfigureAwait(true);
            return r;
        }
    }
}
