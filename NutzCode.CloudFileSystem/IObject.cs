using System;
using System.Collections.Generic;
using System.Dynamic;
using Stream = System.IO.Stream;
using System.Threading;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{
    public interface IObject : IResult
    {
        string Name { get; }
        string FullName { get; }
        DateTime? ModifiedDate { get; }
        DateTime? CreatedDate { get; }
        DateTime? LastViewed { get; }
        ObjectAttributes Attributes { get; }
        IDirectory Parent { get; }        
        IFileSystem FileSystem { get; }
        ExpandoObject MetadataExpanded { get; }
        string Metadata { get; }
        string MetadataMime { get; }

        Task<FileSystemResult> MoveAsync(IDirectory destination, CancellationToken token = default(CancellationToken));
        Task<FileSystemResult> CopyAsync(IDirectory destination, CancellationToken token = default(CancellationToken));
        Task<FileSystemResult> RenameAsync(string newname, CancellationToken token = default(CancellationToken));
        Task<FileSystemResult> TouchAsync(CancellationToken token = default(CancellationToken));
        Task<FileSystemResult> DeleteAsync(bool skipTrash, CancellationToken token = default(CancellationToken));

        List<IFile> GetAssets();
        Task<IFile> CreateAssetAsync(string name, Stream readstream, IProgress<FileProgress> progress, Dictionary<string, object> properties, CancellationToken token = default(CancellationToken));    
        Task<FileSystemResult> WriteMetadataAsync(ExpandoObject metadata, CancellationToken token = default(CancellationToken));
        Task<FileSystemResult<List<Property>>> ReadPropertiesAsync(CancellationToken token = default(CancellationToken));
        Task<FileSystemResult> SavePropertyAsync(Property property, CancellationToken token = default(CancellationToken));       
        
        bool TryGetMetadataValue<T>(string name, out T value);
    }


}
