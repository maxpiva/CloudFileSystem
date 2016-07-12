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


        public LocalFileSystem() : base(null)
        {
            FS = this;
        }
        public static async Task<FileSystemResult<IFileSystem>> Create(string name)
        {
            LocalFileSystem l=new LocalFileSystem();
            l.fname = name;
            FileSystemResult r=await l.Populate();
            if (r.IsOk)
                return new FileSystemResult<IFileSystem>(r.Error);
            l.Sizes=new FileSystemSizes();
            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (LocalDrive ld in l.directories)
            {
                l.Sizes.AvailableSize += ld.Drive.AvailableFreeSpace;
                l.Sizes.UsedSize += ld.Drive.TotalSize - ld.Drive.AvailableFreeSpace;
                l.Sizes.TotalSize += ld.Drive.TotalSize;
            }
            return new FileSystemResult<IFileSystem>(l);

        }
        public async Task<FileSystemResult<IObject>> FromPath(string path)
        {

            IObject ret = await this.ObjectFromPath(path);
            return new FileSystemResult<IObject>(ret);
        }

        public FileSystemSizes Sizes { get; private set; }


        public async Task<FileSystemResult<IDirectory>> GetRoot()
        {
            LocalRoot l=new LocalRoot(FS);
            await l.Populate();
            return new FileSystemResult<IDirectory>(l);
        }
    }
}
