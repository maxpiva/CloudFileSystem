using System;
using System.Threading.Tasks;

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
