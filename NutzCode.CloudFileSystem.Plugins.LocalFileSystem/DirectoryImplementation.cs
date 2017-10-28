using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
#if PRILONGPATH
using Pri.LongPath;
using DirectoryInfo=System.IO.DirectoryInfo;
using FileInfo=System.IO.FileInfo;
#else
using System.IO;
#endif
using Stream = System.IO.Stream;

namespace NutzCode.CloudFileSystem.Plugins.LocalFileSystem
{
    public abstract class  DirectoryImplementation : LocalObject, IDirectory, IOwnDirectory<LocalFile,DirectoryImplementation>
    {
        
        public List<DirectoryImplementation> IntDirectories { get; set; }=new List<DirectoryImplementation>();
        public List<LocalFile> IntFiles { get; set; }=new List<LocalFile>();


        public List<IDirectory> Directories => IntDirectories.Cast<IDirectory>().ToList();
        public List<IFile> Files => IntFiles.Cast<IFile>().ToList();

        public abstract void CreateDirectory(string name);
        public abstract DirectoryInfo[] GetDirectories();
        public abstract FileInfo[] GetFiles();


        public DirectoryImplementation(LocalFileSystem fs) : base(fs)
        {
            
        }

        public virtual async Task<IFile> CreateFileAsync(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            return await InternalCreateFile(this, name, readstream, token, progress, properties);
        }

        public virtual async Task<IDirectory> CreateDirectoryAsync(string name, Dictionary<string, object> properties)
        {
            try
            {
                if (properties == null)
                    properties = new Dictionary<string, object>();
                CreateDirectory(name);
                DirectoryInfo dinfo = new DirectoryInfo(Path.Combine(FullName, name));
                if (properties.Any(a => a.Key.Equals("ModifiedDate", StringComparison.InvariantCultureIgnoreCase)))
                    dinfo.LastWriteTime = (DateTime)properties.First(a => a.Key.Equals("ModifiedDate", StringComparison.InvariantCultureIgnoreCase)).Value;
                if (properties.Any(a => a.Key.Equals("CreatedDate", StringComparison.InvariantCultureIgnoreCase)))
                    dinfo.CreationTime = (DateTime)properties.First(a => a.Key.Equals("CreatedDate", StringComparison.InvariantCultureIgnoreCase)).Value;
                LocalDirectory f = new LocalDirectory(dinfo,FS);
                f.Parent = this;
                FS.Refs[f.FullName] = f;
                IntDirectories.Add(f);
                return await Task.FromResult(f);

            }
            catch (Exception e)
            {
                return new LocalDirectory(null, FS) { Status=Status.SystemError, Error="Error : " + e.Message};
            }
        }

        public virtual bool IsPopulated { get; internal set; }
        public bool IsRoot { get; internal set; } = false;

        public virtual async Task<FileSystemResult> PopulateAsync()
        {
            IntDirectories = GetDirectories().Select(a => new LocalDirectory(a,FS) {Parent = this}).Cast<DirectoryImplementation>().ToList();
            IntDirectories.ForEach(a=>FS.Refs[a.FullName]=a);
            IntFiles = GetFiles().Select(a => new LocalFile(a,FS) { Parent=this }).ToList();
            IsPopulated = true;
            return await Task.FromResult(new FileSystemResult());
        }


        //Windows Pinvoke
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

        //MONO Pinvoke
        [DllImport("libc", SetLastError = true)]
        public static extern int statvfs(string path, out Statvfs buf);
        //MONO Struct
        public struct Statvfs
        {
            public ulong f_bsize;      // file system block size
            public ulong f_frsize;   // fragment size
            public ulong f_blocks;   // size of fs in f_frsize units
            public ulong f_bfree;    // # free blocks
            public ulong f_bavail;   // # free blocks for non-root
            public ulong f_files;    // # inodes
            public ulong f_ffree;    // # free inodes
            public ulong f_favail;   // # free inodes for non-root
            public ulong f_fsid;     // file system id
            public ulong f_flag;     // mount flags
            public ulong f_namemax;  // maximum filename length
        }
       


        public virtual async Task<FileSystemSizes> QuotaAsync()
        {
            FileSystemSizes Sizes = new FileSystemSizes();
            if (Extensions.IsLinux)
            {
                Statvfs vfs;
                statvfs(FullName.Replace("\\\\", "/"), out vfs);
                Sizes.TotalSize = (long) (vfs.f_blocks * vfs.f_frsize);
                Sizes.AvailableSize = (long) (vfs.f_bavail * vfs.f_frsize);
                Sizes.UsedSize = Sizes.TotalSize - Sizes.AvailableSize;
            }
            else
            {
                ulong freebytes;
                ulong totalnumberoffreebytes;
                if (GetDiskFreeSpaceEx(FullName, out freebytes, out _, out totalnumberoffreebytes))
                {
                    Sizes.TotalSize = (long)totalnumberoffreebytes;
                    Sizes.AvailableSize = (long)freebytes;
                    Sizes.UsedSize = Sizes.TotalSize - Sizes.AvailableSize;
                }
            }
            return await Task.FromResult(Sizes);
        }


    }
}
