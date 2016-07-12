using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("NutzCode.CloudFileSystem.AmazonCloudDrive"), InternalsVisibleTo("NutzCode.CloudFileSystem.GoogleDrive")]
namespace NutzCode.CloudFileSystem
{
    public class FileSystemSizes
    {
        public long AvailableSize { get; set; }
        public long TotalSize { get; set; }
        public long UsedSize { get; set; }
    }
}
