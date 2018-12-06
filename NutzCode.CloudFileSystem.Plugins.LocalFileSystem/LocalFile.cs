﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
 using MimeTypes;
using System.IO;
using Stream = System.IO.Stream;
using FileAttributes = System.IO.FileAttributes;


namespace NutzCode.CloudFileSystem.Plugins.LocalFileSystem
{
    public class LocalFile : LocalObject, IFile
    {
        // ReSharper disable once InconsistentNaming
        internal FileInfo file;

        public override string Name => file?.Name ?? string.Empty;
        public override DateTime? ModifiedDate => file?.LastWriteTime;
        public override DateTime? CreatedDate => file?.CreationTime;
        public override DateTime? LastViewed => file?.LastAccessTime;

        public override ObjectAttributes Attributes
        {
            get
            {
                ObjectAttributes at = 0;
                if (file == null)
                    return at;
                if ((file.Attributes & FileAttributes.Hidden) > 0)
                    at |= ObjectAttributes.Hidden;
                return at;
            }
        }

        public override string FullName => file?.FullName;


        internal LocalFile(FileInfo f, LocalFileSystem fs) : base(fs)
        {
            file = f;
        }

        public long Size => file?.Length ?? 0;

        public Task<FileSystemResult<Stream>> OpenReadAsync(CancellationToken token = default(CancellationToken))
        {
            try
            {
                if (file == null)
                    return Task.FromResult(new FileSystemResult<Stream>(Status.ArgumentError,"Empty File"));
                return Task.FromResult(new FileSystemResult<Stream>(file?.OpenRead()));
            }
            catch (Exception e)
            {
                return Task.FromResult(new FileSystemResult<Stream>(Status.SystemError, e.Message));
            }
        }

        public async Task<FileSystemResult> OverwriteFileAsync(Stream readstream, IProgress<FileProgress> progress, Dictionary<string, object> properties, CancellationToken token = default(CancellationToken))
        {
            try
            {
                await InternalCreateFileAsync((DirectoryImplementation) Parent, Name, readstream, token, progress, properties).ConfigureAwait(false);
                return new FileSystemResult();
            }
            catch (Exception e)
            {
                return new FileSystemResult(Status.ArgumentError, e.Message);
            }
        }

        public string MD5 => string.Empty;
        public string SHA1 => string.Empty;

        public string ContentType
        {
            get
            {
                return MimeTypeMap.GetMimeType(Extension);
            }
        }

        public string Extension => Path.GetExtension(Name)?.Substring(1) ?? string.Empty;


        public override Task<FileSystemResult> MoveAsync(IDirectory destination, CancellationToken token = default(CancellationToken))
        {
            try
            {
                DirectoryImplementation to = destination as DirectoryImplementation;
                if (to == null)
                    return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Destination should be a Local Directory"));
                if (to is LocalRoot)
                    return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Root cannot be destination"));
                string destname = Path.Combine(to.FullName, Name);
                File.Move(FullName, destname);
                ((DirectoryImplementation) Parent).IntFiles.Remove(this);
                to.IntFiles.Add(this);
                Parent = to;
                file = new FileInfo(destname);
                return Task.FromResult(new FileSystemResult());
            }
            catch (Exception e)
            {
                return Task.FromResult(new FileSystemResult(Status.SystemError, e.Message));
            }
        }

        public override Task<FileSystemResult> CopyAsync(IDirectory destination, CancellationToken token = default(CancellationToken))
        {
            try
            {
                DirectoryImplementation to = destination as DirectoryImplementation;
                if (to == null)
                    return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Destination should be a Local Directory"));
                if (to is LocalRoot)
                    return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Root cannot be destination"));
                string destname = Path.Combine(to.FullName, Name);

                File.Copy(FullName, destname);
                FileInfo finfo = new FileInfo(destname);
                LocalFile local = new LocalFile(finfo, FS);
                local.Parent = destination;
                return Task.FromResult(new FileSystemResult());
            }
            catch (Exception e)
            {
                return Task.FromResult(new FileSystemResult(Status.SystemError, e.Message));
            }
        }

        public override Task<FileSystemResult> RenameAsync(string newname, CancellationToken token = default(CancellationToken))
        {
            try
            {
                if (string.Equals(Name, newname))
                    return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Unable to rename, names are the same"));
                string newfullname = Path.Combine(Parent?.FullName ?? file.DirectoryName, newname);
                File.Move(FullName, newfullname);
                file = new FileInfo(newfullname);
                return Task.FromResult(new FileSystemResult());
            }
            catch (Exception e)
            {
                return Task.FromResult(new FileSystemResult(Status.SystemError, e.Message));
            }
        }

        public override Task<FileSystemResult> TouchAsync(CancellationToken token = default(CancellationToken))
        {
            try
            {
                file.LastWriteTime = DateTime.Now;
                return Task.FromResult(new FileSystemResult());
            }
            catch (Exception e)
            {
                return Task.FromResult(new FileSystemResult(Status.SystemError, e.Message));
            }
        }

        public override Task<FileSystemResult> DeleteAsync(bool skipTrash, CancellationToken token = default(CancellationToken))
        {
            try
            {
                file.Delete();
                return Task.FromResult(new FileSystemResult());
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException) return Task.FromResult(new FileSystemResult());
                return Task.FromResult(new FileSystemResult(Status.SystemError, e.Message));
            }
        }
    }
}
