using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{

    public interface IFileSystem : IDirectory
    {
        string GetUserAuthorization();
        Task<IObject> ResolveAsync(string path);
        SupportedFlags Supports { get; }
    }
}
