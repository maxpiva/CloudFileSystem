using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{

    public interface IFileSystem : IDirectory
    {
        string GetUserAuthorization();
        Task<FileSystemResult<IObject>> ResolveAsync(string path);
        Task<FileSystemResult<FileSystemSizes>> QuotaAsync();
        SupportedFlags Supports { get; }
    }
}
