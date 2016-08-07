using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem.Plugins.LocalFileSystem
{
    public abstract class DirectoryImplementation : LocalObject, IDirectory
    {
        // ReSharper disable once InconsistentNaming
        internal List<DirectoryImplementation> directories = new List<DirectoryImplementation>();
        // ReSharper disable once InconsistentNaming
        internal List<LocalFile> files = new List<LocalFile>();

        public List<IDirectory> Directories => directories.Cast<IDirectory>().ToList();
        public List<IFile> Files => files.Cast<IFile>().ToList();

        public abstract void CreateDirectory(string name);
        public abstract DirectoryInfo[] GetDirectories();
        public abstract FileInfo[] GetFiles();


        public DirectoryImplementation(LocalFileSystem fs) : base(fs)
        {
            
        }

        public async Task<FileSystemResult> PopulateAsync()
        {
            return await InternalPopulate(false);
        }
        public async Task<FileSystemResult> RefreshAsync()
        {
            return await InternalPopulate(true);
        }

        public virtual async Task<FileSystemResult<IFile>> CreateFileAsync(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            return await InternalCreateFile(this, name, readstream, token, progress, properties);
        }

        public virtual async Task<FileSystemResult<IDirectory>> CreateDirectoryAsync(string name, Dictionary<string, object> properties)
        {
            try
            {
                if (properties == null)
                    properties = new Dictionary<string, object>();
                CreateDirectory(name);
                DirectoryInfo dinfo = new DirectoryInfo(Path.Combine(FullName, name));
                if (properties.Any(a => a.Key.Equals("ModifiedDate", StringComparison.InvariantCultureIgnoreCase)))
                    dinfo.LastWriteTime = (DateTime)properties.First(a => a.Key.Equals("ModifiedDate", StringComparison.InvariantCultureIgnoreCase)).Value;
                if (properties.Any(a => a.Key.Equals("CreatedDate", StringComparison.InvariantCultureIgnoreCase)))
                    dinfo.CreationTime = (DateTime)properties.First(a => a.Key.Equals("CreatedDate", StringComparison.InvariantCultureIgnoreCase)).Value;
                LocalDirectory f = new LocalDirectory(dinfo,FS);
                f.Parent = this;
                FS.Refs[f.FullName] = f;
                directories.Add(f);
                return await Task.FromResult(new FileSystemResult<IDirectory>(f));

            }
            catch (Exception e)
            {
                return new FileSystemResult<IDirectory>("Error : " + e.Message);
            }
        }

        public virtual bool IsPopulated { get; internal set; }
        public bool IsRoot { get; internal set; } = false;

        public virtual async Task<FileSystemResult> InternalPopulate(bool force)
        {
            if (IsPopulated && !force)
                return new FileSystemResult();
            directories = GetDirectories().Select(a => new LocalDirectory(a,FS) {Parent = this}).Cast<DirectoryImplementation>().ToList();
            directories.ForEach(a=>FS.Refs[a.FullName]=a);
            files = GetFiles().Select(a => new LocalFile(a,FS)).ToList();
            IsPopulated = true;
            return await Task.FromResult(new FileSystemResult());
        }
    }
}
