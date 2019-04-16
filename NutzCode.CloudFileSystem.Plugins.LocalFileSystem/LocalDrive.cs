﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public override bool IsEmpty => !Directory.EnumerateFileSystemEntries(FullName).Any();
        
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


        public override async Task<FileSystemResult> MoveAsync(IDirectory destination)
        {
            return await Task.FromResult(new FileSystemResult("Unable to move a root drive"));
        }

        public override async Task<FileSystemResult> CopyAsync(IDirectory destination)
        {
            return await Task.FromResult(new FileSystemResult("Unable to copy a root drive"));
        }

        public override async Task<FileSystemResult> RenameAsync(string newname)
        {
            return await Task.FromResult(new FileSystemResult("Unable to rename a drive"));
        }

        public override async Task<FileSystemResult> TouchAsync()
        {
            return await Task.FromResult(new FileSystemResult("Unable to touch a drive"));
        }

        public override async Task<FileSystemResult> DeleteAsync(bool skipTrash)
        {
            return await Task.FromResult(new FileSystemResult("Unable to delete a drive"));
        }
        public override async Task<FileSystemResult<FileSystemSizes>> QuotaAsync()
        {
            FileSystemSizes Sizes = new FileSystemSizes();
            Sizes.AvailableSize += Drive.AvailableFreeSpace;
            Sizes.UsedSize += Drive.TotalSize - Drive.AvailableFreeSpace;
            Sizes.TotalSize += Drive.TotalSize;
            return await Task.FromResult(new FileSystemResult<FileSystemSizes>(Sizes));
        }
    }
}
