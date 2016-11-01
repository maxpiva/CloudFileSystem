using System;
using System.Linq;
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
    public class LocalFileSystem : LocalRoot, IFileSystem
    {
        public string GetUserAuthorization()
        {
            return string.Empty;

        }
        public SupportedFlags Supports => SupportedFlags.Nothing;
        internal DirectoryCache.DirectoryCache Refs = new DirectoryCache.DirectoryCache(CloudFileSystemPluginFactory.DirectoryTreeCacheSize);


        public LocalFileSystem() : base(null)
        {
            FS = this;
        }
        public static async Task<FileSystemResult<IFileSystem>> Create(string name)
        {
            LocalFileSystem l=new LocalFileSystem();
            l.fname = name;
            FileSystemResult r=await l.PopulateAsync();
            if (!r.IsOk)
                return new FileSystemResult<IFileSystem>(r.Error);            
            return new FileSystemResult<IFileSystem>(l);
        }

        public async Task<FileSystemResult<FileSystemSizes>> QuotaAsync()
        {
            Sizes = new FileSystemSizes();
            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (LocalDrive ld in IntDirectories)
            {
                try
                {
                    Sizes.AvailableSize += ld.Drive.AvailableFreeSpace;
                    Sizes.UsedSize += ld.Drive.TotalSize - ld.Drive.AvailableFreeSpace;
                    Sizes.TotalSize += ld.Drive.TotalSize;
                }
                catch (Exception) //Cdrom and others
                {
                    //ignored
                }
            }
            return await Task.FromResult(new FileSystemResult<FileSystemSizes>(Sizes));
        }

        public async Task<FileSystemResult<IObject>> ResolveAsync(string path)
        {
            if (path.StartsWith("\\\\"))
            {
                int idx = path.IndexOf("\\", 2);
                if (idx >= 0)
                {
                    idx = path.IndexOf("\\", idx + 1);
                    if (idx < 0)
                        idx = path.Length;
                }
                else
                    idx = path.Length;
                string share = path.Substring(0, idx);
                if (!System.IO.Directory.Exists(share))
                    return new FileSystemResult<IObject>("Not found");
                if (!FS.Directories.Any(a => a.FullName == share))
                    FS.AddUncPath(share);
                path = path.Replace(share, share.Replace("\\", "*"));
            }


            return await Refs.ObjectFromPath(this, path);
        }

        public FileSystemSizes Sizes { get; private set; }


        public async Task<FileSystemResult<IDirectory>> GetRoot()
        {
            LocalRoot l=new LocalRoot(FS);
            await l.PopulateAsync();
            return new FileSystemResult<IDirectory>(l);
        }
    }
}
