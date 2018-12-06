using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using MemoryStream = System.IO.MemoryStream;
using Stream = System.IO.Stream;




namespace NutzCode.CloudFileSystem
{
    public class MemoryFile : IFile
    {
        private byte[] _data;
        readonly string _fullname;
        readonly string _mime;

        public string Name { get; private set; }
        public string FullName => _fullname;
        public DateTime? ModifiedDate => DateTime.Now;
        public DateTime? CreatedDate => DateTime.Now;
        public DateTime? LastViewed => DateTime.Now;
        public ObjectAttributes Attributes => 0;
        public IDirectory Parent => null;
        public IFileSystem FileSystem => null;
        public ExpandoObject MetadataExpanded => null;
        public string Metadata => null;
        public string MetadataMime => null;


        public MemoryFile(string fullname, string name, string mime, byte[] data)
        {
            if (data == null || data.Length == 0 || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(fullname))
                throw new ArgumentException();
            _fullname = fullname;
            Name = name;
            _data = data;
            _mime = mime;
        }


        public Task<FileSystemResult> MoveAsync(IDirectory destination, CancellationToken token = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult> CopyAsync(IDirectory destination, CancellationToken token = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult> RenameAsync(string newname, CancellationToken token = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult> TouchAsync(CancellationToken token = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult> DeleteAsync(bool skipTrash, CancellationToken token = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public List<IFile> GetAssets()
        {
            throw new NotSupportedException();
        }

        public Task<IFile> CreateAssetAsync(string name, Stream readstream, IProgress<FileProgress> progress, Dictionary<string, object> properties, CancellationToken token = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult<ExpandoObject>> ReadMetadataAsync(CancellationToken token = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult> WriteMetadataAsync(ExpandoObject metadata, CancellationToken token = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult<List<Property>>> ReadPropertiesAsync(CancellationToken token = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult> SavePropertyAsync(Property property, CancellationToken token = default(CancellationToken))
        {
            throw new NotSupportedException();
        }


        public bool TryGetMetadataValue<T>(string name, out T value)
        {
            value = default(T);
            return false;
        }

        public long Size => _data.Length;
        public Task<FileSystemResult<Stream>> OpenReadAsync(CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult<Stream>(new MemoryStream(_data)));
        }

        public Task<FileSystemResult> OverwriteFileAsync(Stream readstream, IProgress<FileProgress> progress, Dictionary<string, object> properties, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult());
        }

        public string MD5
        {
            get
            {
                byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(_data);
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            }
        }

        public string SHA1
        {
            get
            {
                byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("SHA")).ComputeHash(_data);
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            }
        }

        public string ContentType => _mime;
        public string Extension => Path.GetExtension(Name);
        public Status Status { get; set; }
        public string Error { get; set; }
    }
}
