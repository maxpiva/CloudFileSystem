using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{
    public interface IObject
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

        Task<FileSystemResult> MoveAsync(IDirectory destination);
        Task<FileSystemResult> CopyAsync(IDirectory destination);
        Task<FileSystemResult> RenameAsync(string newname);
        Task<FileSystemResult> TouchAsync();
        Task<FileSystemResult> DeleteAsync(bool skipTrash);

        List<IFile> GetAssets();
        Task<FileSystemResult<IFile>> CreateAssetAsync(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties);    
        Task<FileSystemResult> WriteMetadataAsync(ExpandoObject metadata);
        Task<FileSystemResult<List<Property>>> ReadPropertiesAsync();
        Task<FileSystemResult> SavePropertyAsync(Property property);
                
        bool TryGetMetadataValue<T>(string name, out T value);
    }


}
