﻿﻿using System;
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

        public async Task<FileSystemResult<Stream>> OpenReadAsync()
        {
            try
            {
                if (file == null)
                    return new FileSystemResult<Stream>(Status.ArgumentError,"Empty File");
                return await Task.FromResult(new FileSystemResult<Stream>(file?.OpenRead()));
            }
            catch (Exception e)
            {
                return await Task.FromResult(new FileSystemResult<Stream>(Status.SystemError, e.Message));
            }
        }

        public async Task<FileSystemResult> OverwriteFileAsync(Stream readstream, CancellationToken token,
            IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            try
            {
                await InternalCreateFile((DirectoryImplementation) Parent, Name, readstream, token, progress, properties);
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


        public override async Task<FileSystemResult> MoveAsync(IDirectory destination)
        {
            try
            {
                DirectoryImplementation to = destination as DirectoryImplementation;
                if (to == null)
                    return new FileSystemResult(Status.ArgumentError, "Destination should be a Local Directory");
                if (to is LocalRoot)
                    return new FileSystemResult(Status.ArgumentError, "Root cannot be destination");
                string destname = Path.Combine(to.FullName, Name);
                File.Move(FullName, destname);
                ((DirectoryImplementation) Parent).IntFiles.Remove(this);
                to.IntFiles.Add(this);
                Parent = to;
                file = new FileInfo(destname);
                return await Task.FromResult(new FileSystemResult());
            }
            catch (Exception e)
            {
                return new FileSystemResult(Status.SystemError, e.Message);
            }
        }

        public override async Task<FileSystemResult> CopyAsync(IDirectory destination)
        {
            try
            {
                DirectoryImplementation to = destination as DirectoryImplementation;
                if (to == null)
                    return new FileSystemResult(Status.ArgumentError, "Destination should be a Local Directory");
                if (to is LocalRoot)
                    return new FileSystemResult(Status.ArgumentError, "Root cannot be destination");
                string destname = Path.Combine(to.FullName, Name);

                File.Copy(FullName, destname);
                FileInfo finfo = new FileInfo(destname);
                LocalFile local = new LocalFile(finfo, FS);
                local.Parent = destination;
                return await Task.FromResult(new FileSystemResult());
            }
            catch (Exception e)
            {
                return new FileSystemResult(Status.SystemError, e.Message);
            }
        }

        public override async Task<FileSystemResult> RenameAsync(string newname)
        {
            try
            {
                if (string.Equals(Name, newname))
                    return new FileSystemResult(Status.ArgumentError, "Unable to rename, names are the same");
                string newfullname = Path.Combine(Parent?.FullName ?? file.DirectoryName, newname);
                File.Move(FullName, newfullname);
                file = new FileInfo(newfullname);
                return await Task.FromResult(new FileSystemResult());
            }
            catch (Exception e)
            {
                return new FileSystemResult(Status.SystemError, e.Message);
            }
        }

        public override async Task<FileSystemResult> TouchAsync()
        {
            try
            {
                file.LastWriteTime = DateTime.Now;
                return await Task.FromResult(new FileSystemResult());
            }
            catch (Exception e)
            {
                return new FileSystemResult(Status.SystemError, e.Message);
            }
        }

        public override async Task<FileSystemResult> DeleteAsync(bool skipTrash)
        {
            try
            {
                file.Delete();
                return await Task.FromResult(new FileSystemResult());
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException) return await Task.FromResult(new FileSystemResult());
                return new FileSystemResult(Status.SystemError, e.Message);
            }
        }
    }
}
