﻿using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Stream = System.IO.Stream;
using DriveInfo = System.IO.DriveInfo;

namespace NutzCode.CloudFileSystem.Plugins.LocalFileSystem
{
    public class LocalRoot : DirectoryImplementation
    {
        // ReSharper disable once InconsistentNaming
        internal string fname;

        public override string Name => fname;
        public override DateTime? ModifiedDate => DateTime.Now;
        public override DateTime? CreatedDate => DateTime.Now;
        public override DateTime? LastViewed => DateTime.Now;

        public override ObjectAttributes Attributes => ObjectAttributes.Directory;
        public override string FullName => fname;

        public override void CreateDirectory(string name)
        {
        }

        private List<string> UncPaths=new List<string>();

        public LocalRoot(LocalFileSystem fs) : base(fs)
        {
            IsRoot = true;
        }
        public override DirectoryInfo[] GetDirectories()
        {
            return new DirectoryInfo[0];
        }

        public override FileInfo[] GetFiles()
        {
            return new FileInfo[0];
        }

        public override Task<IFile> CreateFileAsync(string name, Stream readstream, IProgress<FileProgress> progress, Dictionary<string, object> properties, CancellationToken token=default(CancellationToken))
        {
            return Task.FromResult((IFile)new LocalFile(null, FS) { Status=Status.ArgumentError, Error = "Unable to write to root"});
        }

        public override Task<IDirectory> CreateDirectoryAsync(string name, Dictionary<string, object> properties, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult((IDirectory)new LocalDirectory(null, FS) { Status=Status.ArgumentError, Error="Unable to create a directory in the root"});
        }

        public override Task<FileSystemResult> PopulateAsync(CancellationToken token = default(CancellationToken))
        {
            IntDirectories = DriveInfo.GetDrives().Select(a => new LocalDrive(a,FS) {Parent=this }).Cast<DirectoryImplementation>().ToList();
            IntDirectories.AddRange(UncPaths.Select(a=>new LocalDirectory(new DirectoryInfo(a),FS)));
            IsPopulated = true;
            return Task.FromResult(new FileSystemResult());
        }

        internal void AddUncPath(string path)
        {
            UncPaths.Add(path);
            IntDirectories.Add(new LocalDirectory(new DirectoryInfo(path),FS));
        }
        public override Task<FileSystemResult> MoveAsync(IDirectory destination, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Unable to move a root drive"));
        }

        public override Task<FileSystemResult> CopyAsync(IDirectory destination, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Unable to copy a root drive"));
        }

        public override Task<FileSystemResult> RenameAsync(string newname, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Unable to rename the root"));
        }

        public override Task<FileSystemResult> TouchAsync(CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Unable to touch the root"));
        }

        public override Task<FileSystemResult> DeleteAsync(bool skipTrash, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Unable to delete the root"));
        }
    }
}
