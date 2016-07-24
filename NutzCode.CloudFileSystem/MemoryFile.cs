using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{
    public class MemoryFile : IFile
    {
        private byte[] _data;
        private string _name;
        private string _fullname;
        private string _mime;

        public string Name => _name;
        public string FullName => _fullname;
        public DateTime? ModifiedDate => DateTime.Now;
        public DateTime? CreatedDate => DateTime.Now;
        public DateTime? LastViewed => DateTime.Now;
        public ObjectAttributes Attributes => (ObjectAttributes) 0;
        public IDirectory Parent => null;
        public IFileSystem FileSystem => null;
        public ExpandoObject MetadataExpanded => null;
        public string Metadata => null;
        public string MetadataMime => null;


        public MemoryFile(string fullname, string name, string mime, byte[] data)
        {
            if (data==null || data.Length==0 || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(fullname))
                throw new ArgumentException();
            _fullname = fullname;
            _name = name;
            _data = data;
            _mime = mime;
        }


        public Task<FileSystemResult> MoveAsync(IDirectory destination)
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult> CopyAsync(IDirectory destination)
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult> RenameAsync(string newname)
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult> TouchAsync()
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult> DeleteAsync(bool skipTrash)
        {
            throw new NotSupportedException();
        }

        public List<IFile> GetAssets()
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult<IFile>> CreateAssetAsync(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult<ExpandoObject>> ReadMetadata()
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult> WriteMetadataAsync(ExpandoObject metadata)
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult<List<Property>>> ReadPropertiesAsync()
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult> SavePropertyAsync(Property property)
        {
            throw new NotSupportedException();
        }


        public Task<FileSystemResult> SaveProperties(Dictionary<string, string> properties)
        {
            throw new NotSupportedException();
        }

        public bool TryGetMetadataValue<T>(string name, out T value)
        {
            value = default(T);
            return false;
        }

        public long Size => _data.Length;
        public async Task<FileSystemResult<Stream>> OpenReadAsync()
        {
            return await Task.FromResult(new FileSystemResult<Stream>(new MemoryStream(_data)));
        }

        public async Task<FileSystemResult> OverwriteFileAsync(Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            return await Task.FromResult(new FileSystemResult());
        }

        public string MD5
        {
            get
            {
                byte[] hash = ((HashAlgorithm) CryptoConfig.CreateFromName("MD5")).ComputeHash(_data);
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            }
        }

        public string SHA1
        {
            get
            {
                byte[] hash = ((HashAlgorithm) CryptoConfig.CreateFromName("SHA")).ComputeHash(_data);
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            }
        }

        public string ContentType => _mime;
        public string Extension => Path.GetExtension(_name);
    }
}
