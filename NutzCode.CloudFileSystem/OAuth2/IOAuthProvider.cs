using System.Threading;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem.OAuth2
{

    public interface IOAuthProvider
    {
        string Name { get; }
        Task<AuthResult> LoginAsync(AuthRequest request, CancellationToken token=default(CancellationToken));
    }

}
