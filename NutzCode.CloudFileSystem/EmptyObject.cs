using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{
    public class EmptyObject : IObject
    {
        public Status Status { get; set; }
        public string Error { get; set; }
        public string Name { get; }
        public string FullName { get; }
        public DateTime? ModifiedDate { get; }
        public DateTime? CreatedDate { get; }
        public DateTime? LastViewed { get; }
        public ObjectAttributes Attributes { get; }
        public IDirectory Parent { get; }
        public IFileSystem FileSystem { get; }
        public ExpandoObject MetadataExpanded { get; }
        public string Metadata { get; }
        public string MetadataMime { get; }
        public Task<FileSystemResult> MoveAsync(IDirectory destination)
        {
            throw new NotImplementedException();
        }

        public Task<FileSystemResult> CopyAsync(IDirectory destination)
        {
            throw new NotImplementedException();
        }

        public Task<FileSystemResult> RenameAsync(string newname)
        {
            throw new NotImplementedException();
        }

        public Task<FileSystemResult> TouchAsync()
        {
            throw new NotImplementedException();
        }

        public Task<FileSystemResult> DeleteAsync(bool skipTrash)
        {
            throw new NotImplementedException();
        }

        public List<IFile> GetAssets()
        {
            throw new NotImplementedException();
        }

        public Task<IFile> CreateAssetAsync(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            throw new NotImplementedException();
        }

        public Task<FileSystemResult> WriteMetadataAsync(ExpandoObject metadata)
        {
            throw new NotImplementedException();
        }

        public Task<FileSystemResult<List<Property>>> ReadPropertiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<FileSystemResult> SavePropertyAsync(Property property)
        {
            throw new NotImplementedException();
        }

        public bool TryGetMetadataValue<T>(string name, out T value)
        {
            throw new NotImplementedException();
        }
    }
}