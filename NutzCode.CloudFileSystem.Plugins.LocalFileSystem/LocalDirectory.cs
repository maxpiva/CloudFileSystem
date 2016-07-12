using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem.Plugins.LocalFileSystem
{
    public class LocalDirectory : DirectoryImplementation
    {
        private DirectoryInfo _directory;

        public override string Name => _directory?.Name ?? string.Empty;
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


        public override async Task<FileSystemResult> Move(IDirectory destination)
        {
            DirectoryImplementation to = destination as DirectoryImplementation;
            if (to == null)
                return new FileSystemResult("Destination should be a Local Directory");
            if (to is LocalRoot)
                return new FileSystemResult("Root cannot be destination");
            string destname = Path.Combine(to.FullName, Name);
            Directory.Move(FullName, destname);
            ((DirectoryImplementation)Parent).directories.Remove(this);
            to.directories.Add(this);
            Parent = destination;
            return await Task.FromResult(new FileSystemResult());
        }

        public override async Task<FileSystemResult> Copy(IDirectory destination)
        {
            return await Task.FromResult(new FileSystemResult("Directory copy is not supported"));
        }

        public override async Task<FileSystemResult> Rename(string newname)
        {
            if (string.Equals(Name, newname))
                return new FileSystemResult("Unable to rename, names are the same");
            string newfullname = Path.Combine(Parent.Name, newname);
            Directory.Move(FullName, newfullname);
            DirectoryInfo dinfo = new DirectoryInfo(newfullname);
            _directory = dinfo;
            return await Task.FromResult(new FileSystemResult());
        }

        public override async Task<FileSystemResult> Touch()
        {
            _directory.LastWriteTime = DateTime.Now;
            return await Task.FromResult(new FileSystemResult());
        }

        public override async Task<FileSystemResult> Delete(bool skipTrash)
        {
            Directory.Delete(FullName,true);
            ((DirectoryImplementation)Parent).directories.Remove(this);
            return await Task.FromResult(new FileSystemResult());
        }
    }
}
