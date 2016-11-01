using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Path = Pri.LongPath.Path;
using Directory = Pri.LongPath.Directory;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;
using File = Pri.LongPath.File;
using FileSystemInfo = Pri.LongPath.FileSystemInfo;
using FileInfo = Pri.LongPath.FileInfo;
using Stream = System.IO.Stream;
using FileAttributes = System.IO.FileAttributes;

namespace NutzCode.CloudFileSystem.Plugins.LocalFileSystem
{
    public abstract class DirectoryImplementation : LocalObject, IDirectory, IOwnDirectory<LocalFile,DirectoryImplementation>
    {
        public List<DirectoryImplementation> IntDirectories { get; set; }=new List<DirectoryImplementation>();
        public List<LocalFile> IntFiles { get; set; }=new List<LocalFile>();


        public List<IDirectory> Directories => IntDirectories.Cast<IDirectory>().ToList();
        public List<IFile> Files => IntFiles.Cast<IFile>().ToList();

        public abstract void CreateDirectory(string name);
        public abstract DirectoryInfo[] GetDirectories();
        public abstract FileInfo[] GetFiles();


        public DirectoryImplementation(LocalFileSystem fs) : base(fs)
        {
            
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
                IntDirectories.Add(f);
                return await Task.FromResult(new FileSystemResult<IDirectory>(f));

            }
            catch (Exception e)
            {
                return new FileSystemResult<IDirectory>("Error : " + e.Message);
            }
        }

        public virtual bool IsPopulated { get; internal set; }
        public bool IsRoot { get; internal set; } = false;

        public virtual async Task<FileSystemResult> PopulateAsync()
        {
            IntDirectories = GetDirectories().Select(a => new LocalDirectory(a,FS) {Parent = this}).Cast<DirectoryImplementation>().ToList();
            IntDirectories.ForEach(a=>FS.Refs[a.FullName]=a);
            IntFiles = GetFiles().Select(a => new LocalFile(a,FS) { Parent=this }).ToList();
            IsPopulated = true;
            return await Task.FromResult(new FileSystemResult());
        }


    }
}
