using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IFile : IObject
    {
        long Size { get; }
        Task<FileSystemResult<Stream>> OpenRead();
        Task<FileSystemResult> OverwriteFile(Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties);
        string MD5 { get; }
        string SHA1 { get; }
        string ContentType { get; }
        string Extension { get; }
    }

}
