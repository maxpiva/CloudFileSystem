using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Stream = System.IO.Stream;
using System.Threading;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IFile : IObject
    {
        long Size { get; }
        Task<FileSystemResult<Stream>> OpenReadAsync(CancellationToken token = default(CancellationToken));
        Task<FileSystemResult> OverwriteFileAsync(Stream readstream, IProgress<FileProgress> progress, Dictionary<string, object> properties, CancellationToken token = default(CancellationToken));
        string MD5 { get; }
        string SHA1 { get; }
        string ContentType { get; }
        string Extension { get; }
    }

}
