using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using FileAttributes = System.IO.FileAttributes;
using System.Threading;

namespace NutzCode.CloudFileSystem.Plugins.LocalFileSystem
{
    public class LocalDirectory : DirectoryImplementation
    {
        private DirectoryInfo _directory;

        public override string Name => ((Parent == null) ? _directory?.FullName : _directory?.Name) ?? string.Empty;
        public override DateTime? ModifiedDate => _directory?.LastWriteTime;
        public override DateTime? CreatedDate => _directory?.CreationTime;
        public override DateTime? LastViewed => _directory?.LastAccessTime;

        public override ObjectAttributes Attributes
        {
            get
            {
                ObjectAttributes at = ObjectAttributes.Directory;
                if (_directory == null)
                    return at;
                if ((_directory.Attributes & FileAttributes.Hidden) > 0)
                    at |= ObjectAttributes.Hidden;
                return at;
            }
        }
        public override string FullName => _directory?.FullName;

        internal LocalDirectory(DirectoryInfo d, LocalFileSystem fs) : base(fs)
        {
            _directory = d;
        }

        public override void CreateDirectory(string name)
        {
            _directory?.CreateSubdirectory(name);
        }

        public override DirectoryInfo[] GetDirectories()
        {
            if (_directory==null)
                return new DirectoryInfo[0];
            return _directory.GetDirectories();
        }

        public override FileInfo[] GetFiles()
        {
            if (_directory == null)
                return new FileInfo[0];
            return Directory.GetFiles(FullName).Select(a => new FileInfo(a)).ToArray();
        }


        public override Task<FileSystemResult> MoveAsync(IDirectory destination, CancellationToken token = default(CancellationToken))
        {
            DirectoryImplementation to = destination as DirectoryImplementation;
            if (to == null)
                return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Destination should be a Local Directory"));
            if (to is LocalRoot)
                return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Root cannot be destination"));
            string destname = Path.Combine(to.FullName, Name);
            string oldFullname = FullName;
            Directory.Move(FullName, destname);
            FS.Refs.Remove(oldFullname);
            ((DirectoryImplementation)Parent).IntDirectories.Remove(this);
            to.IntDirectories.Add(this);
            Parent = destination;
            FS.Refs[FullName] = this;
            return Task.FromResult(new FileSystemResult());

        }
        public override Task<FileSystemResult> CopyAsync(IDirectory destination, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Directory copy is not supported"));
        }

        public override Task<FileSystemResult> RenameAsync(string newname, CancellationToken token = default(CancellationToken))
        {
            if (string.Equals(Name, newname))
                return Task.FromResult(new FileSystemResult(Status.ArgumentError, "Unable to rename, names are the same"));
            string oldFullname = FullName;
            string newfullname = Path.Combine(Parent.FullName, newname);
            Directory.Move(FullName, newfullname);
            FS.Refs.Remove(oldFullname);
            DirectoryInfo dinfo = new DirectoryInfo(newfullname);
            _directory = dinfo;
            FS.Refs[FullName] = this;
            return Task.FromResult(new FileSystemResult());
        }

        public override Task<FileSystemResult> TouchAsync(CancellationToken token = default(CancellationToken))
        {
            _directory.LastWriteTime = DateTime.Now;
            return Task.FromResult(new FileSystemResult());
        }

        public override Task<FileSystemResult> DeleteAsync(bool skipTrash, CancellationToken token = default(CancellationToken))
        {
            Directory.Delete(FullName,true);
            ((DirectoryImplementation)Parent).IntDirectories.Remove(this);
            return Task.FromResult(new FileSystemResult());
        }
    }
}
