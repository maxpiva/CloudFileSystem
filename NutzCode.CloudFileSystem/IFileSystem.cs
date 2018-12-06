using System.Threading;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{

    public interface IFileSystem : IDirectory
    {
        string GetUserAuthorization();
        Task<IObject> ResolveAsync(string path, CancellationToken token = default(CancellationToken));
        SupportedFlags Supports { get; }
    }
}
