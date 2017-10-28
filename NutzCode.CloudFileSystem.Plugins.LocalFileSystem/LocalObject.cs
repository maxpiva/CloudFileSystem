using System;
using System.Collections.Generic;
using System.Dynamic;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if PRILONGPATH
using Pri.LongPath;
using DirectoryInfo=System.IO.DirectoryInfo;
using FileInfo=System.IO.FileInfo;
#else
using System.IO;
#endif
using Stream = System.IO.Stream;
using FileMode = System.IO.FileMode;
using FileAccess = System.IO.FileAccess;

namespace NutzCode.CloudFileSystem.Plugins.LocalFileSystem
{
    public abstract class LocalObject : IObject
    {
        
        public abstract string Name { get;}
        public abstract DateTime? ModifiedDate { get;  }
        public abstract DateTime? CreatedDate { get;  }
        public abstract DateTime? LastViewed { get; }
        public abstract ObjectAttributes Attributes { get; }
        public abstract string FullName { get;  }

        public IDirectory Parent { get; internal set; }
        // ReSharper disable once InconsistentNaming
        internal LocalFileSystem FS;

        public string MetadataMime { get; } = null;
        public abstract Task<FileSystemResult> MoveAsync(IDirectory destination);
        public abstract Task<FileSystemResult> CopyAsync(IDirectory destination);
        public abstract Task<FileSystemResult> RenameAsync(string newname);
        public abstract Task<FileSystemResult> TouchAsync();
        public abstract Task<FileSystemResult> DeleteAsync(bool skipTrash);

        public List<IFile> GetAssets()
        {
            throw new NotSupportedException();
        }

        public Task<IFile> CreateAssetAsync(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
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

        public IFileSystem FileSystem => FS;
        public ExpandoObject MetadataExpanded { get; } = null;
        public string Metadata { get; } = null;

        protected LocalObject(LocalFileSystem fs)
        {
            FS = fs;
        }

        internal async Task<IFile> InternalCreateFile(DirectoryImplementation dir, string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            if (properties == null)
                properties = new Dictionary<string, object>();
            string path = name;
            if (dir.Parent != null)
                path = Path.Combine(dir.FullName, path);
            Stream s = File.Open(path, FileMode.Create, FileAccess.Write);
            byte[] block = new byte[1024 * 128];
            long left = readstream.Length;
            do
            {
                int size = (int)Math.Min(left, block.Length);
                int rsize = await readstream.ReadAsync(block, 0, size, token);
                await s.WriteAsync(block, 0, rsize, token);
                left -= rsize;
                FileProgress p = new FileProgress
                {
                    Percentage = ((float)(readstream.Length - left) * 100) / readstream.Length,
                    TotalSize = readstream.Length,
                    TransferSize = readstream.Length - left
                };
                progress.Report(p);
            } while (left > 0 && !token.IsCancellationRequested);
            s.Close();
            if (token.IsCancellationRequested)
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                    // ignored
                }
                return new LocalFile(null, FS) { Status = Status.Canceled,Error="Transfer canceled"};
            }
            FileInfo finfo = new FileInfo(path);
            if (properties.Any(a => a.Key.Equals("ModifiedDate", StringComparison.InvariantCultureIgnoreCase)))
                finfo.LastWriteTime = (DateTime)properties.First(a => a.Key.Equals("ModifiedDate", StringComparison.InvariantCultureIgnoreCase)).Value;
            if (properties.Any(a => a.Key.Equals("CreatedDate", StringComparison.InvariantCultureIgnoreCase)))
                finfo.CreationTime = (DateTime)properties.First(a => a.Key.Equals("CreatedDate", StringComparison.InvariantCultureIgnoreCase)).Value;
            return new LocalFile(finfo, FS);
        }

        public bool TryGetMetadataValue<T>(string prop, out T value)
        {
            value = default(T);
            Type tt = typeof (T);
            if ((this is LocalFile) && (tt.Name.ToLowerInvariant()=="string") && ((prop.Equals("md5", StringComparison.InvariantCultureIgnoreCase) || prop.Equals("sha1", StringComparison.InvariantCultureIgnoreCase))))
            {
                string res= Extensions.HashFromExtendedFile(((LocalFile)this).file.FullName,prop);
                if (string.IsNullOrEmpty(res))
                    return false;
                value = (T) (object) res;
                return true;
            }
            return false;
        }

        public Status Status { get; set; }
        public string Error { get; set; }
    }
}
