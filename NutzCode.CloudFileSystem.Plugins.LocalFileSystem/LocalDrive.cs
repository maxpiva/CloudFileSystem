using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using DriveInfo = System.IO.DriveInfo;
using System.Threading;

namespace NutzCode.CloudFileSystem.Plugins.LocalFileSystem
{
    public class LocalDrive : DirectoryImplementation
    {
        internal DriveInfo Drive;
        
        public override string Name => Drive?.Name.Replace("\\",string.Empty) ?? string.Empty;
        public override DateTime? ModifiedDate => DateTime.Now;
        public override DateTime? CreatedDate => DateTime.Now;
        public override DateTime? LastViewed => DateTime.Now;

        public override ObjectAttributes Attributes => ObjectAttributes.Directory;

        public override string FullName => Name;
        
        internal LocalDrive(DriveInfo d, LocalFileSystem fs) : base(fs)
        {
            Drive = d;
        }

        public override void CreateDirectory(string name)
        {
            Directory.CreateDirectory(Path.Combine(Name, name));
        }

        public override DirectoryInfo[] GetDirectories()
        {
            if (Drive == null)
                return new DirectoryInfo[0];
            String path = Name;
            if (!Name.EndsWith("" + Path.DirectorySeparatorChar))
                path += Path.DirectorySeparatorChar;
            return Directory.GetDirectories(path).Select(a=>new DirectoryInfo(a)).ToArray();
        }

        public override FileInfo[] GetFiles()
        {
            if (Drive == null)
                return new FileInfo[0];
            String path = Name;
            if (!Name.EndsWith("" + Path.DirectorySeparatorChar))
                path += Path.DirectorySeparatorChar;
            return Directory.GetFiles(path).Select(a => new FileInfo(a)).ToArray();
        }


        public override Task<FileSystemResult> MoveAsync(IDirectory destination, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult(Status.ArgumentError,"Unable to move a root drive"));
        }

        public override Task<FileSystemResult> CopyAsync(IDirectory destination, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Unable to copy a root drive"));
        }

        public override Task<FileSystemResult> RenameAsync(string newname, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Unable to rename a drive"));
        }

        public override Task<FileSystemResult> TouchAsync(CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Unable to touch a drive"));
        }

        public override Task<FileSystemResult> DeleteAsync(bool skipTrash, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Unable to delete a drive"));
        }
        public override Task<FileSystemSizes> QuotaAsync(CancellationToken token = default(CancellationToken))
        {
            FileSystemSizes Sizes = new FileSystemSizes();
            Sizes.AvailableSize += Drive.AvailableFreeSpace;
            Sizes.UsedSize += Drive.TotalSize - Drive.AvailableFreeSpace;
            Sizes.TotalSize += Drive.TotalSize;
            return Task.FromResult(Sizes);
        }
    }
}
