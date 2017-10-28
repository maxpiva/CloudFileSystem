using System;
using System.Collections.Generic;
using Stream = System.IO.Stream;
using System.Threading;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{
    public interface IDirectory : IObject
    {
        List<IDirectory> Directories { get; }
        List<IFile> Files { get; }
        Task<IFile> CreateFileAsync(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties);
        Task<IDirectory> CreateDirectoryAsync(string name, Dictionary<string, object> properties);
        bool IsPopulated { get; }
        bool IsRoot { get; }
        Task<FileSystemResult> PopulateAsync();
        Task<FileSystemSizes> QuotaAsync();

    }
}
