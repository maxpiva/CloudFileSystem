using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{
    public interface IDirectory : IObject
    {
        List<IDirectory> Directories { get; }
        List<IFile> Files { get; }
        Task<FileSystemResult<IFile>> CreateFileAsync(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties);
        Task<FileSystemResult<IDirectory>> CreateDirectoryAsync(string name, Dictionary<string, object> properties);
        bool IsPopulated { get; }
        bool IsRoot { get; }
        Task<FileSystemResult> PopulateAsync();
        Task<FileSystemResult> RefreshAsync();
    }
}
