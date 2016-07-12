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

        Task<FileSystemResult> Move(IDirectory destination);
        Task<FileSystemResult> Copy(IDirectory destination);
        Task<FileSystemResult> Rename(string newname);
        Task<FileSystemResult> Touch();
        Task<FileSystemResult> Delete(bool skipTrash);

        List<IFile> GetAssets();
        Task<FileSystemResult<IFile>> CreateAsset(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties);    
        Task<FileSystemResult> WriteMetadata(ExpandoObject metadata);
        Task<FileSystemResult<List<Property>>> ReadProperties();
        Task<FileSystemResult> SaveProperty(Property property);
                
        bool TryGetMetadataValue<T>(string name, out T value);
    }


}
