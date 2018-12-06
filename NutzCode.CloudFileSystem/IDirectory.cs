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
        Task<IFile> CreateFileAsync(string name, Stream readstream, IProgress<FileProgress> progress, Dictionary<string, object> properties, CancellationToken token = default(CancellationToken));
        Task<IDirectory> CreateDirectoryAsync(string name, Dictionary<string, object> properties, CancellationToken token = default(CancellationToken));
        bool IsPopulated { get; }
        bool IsRoot { get; }
        Task<FileSystemResult> PopulateAsync(CancellationToken token = default(CancellationToken));
        Task<FileSystemSizes> QuotaAsync(CancellationToken token = default(CancellationToken));



    }
}
