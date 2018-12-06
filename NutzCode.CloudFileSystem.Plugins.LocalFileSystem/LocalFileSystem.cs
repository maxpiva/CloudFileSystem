﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace NutzCode.CloudFileSystem.Plugins.LocalFileSystem
{
    public class LocalFileSystem : LocalRoot, IFileSystem
    {
        public string GetUserAuthorization()
        {
            return string.Empty;

        }
        public SupportedFlags Supports => SupportedFlags.Nothing;

        internal DirectoryCache.DirectoryCache Refs =
            new DirectoryCache.DirectoryCache(CloudFileSystemPluginFactory.DirectoryTreeCacheSize);



        public LocalFileSystem() : base(null)
        {
            FS = this;
        }

        public static async Task<IFileSystem> CreateAsync(string name, CancellationToken token = default(CancellationToken))
        {
            LocalFileSystem l = new LocalFileSystem();
            l.fname = name;
            FileSystemResult r = await l.PopulateAsync(token).ConfigureAwait(false);
            if (r.Status != Status.Ok)
                r.CopyErrorTo(l);
            return l;
        }

        //TODO locking?
        public override async Task<FileSystemSizes> QuotaAsync(CancellationToken token = default(CancellationToken))
        {
            Sizes = new FileSystemSizes();
            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (DirectoryImplementation ld in IntDirectories)
            {
                try
                {
                    FileSystemSizes z = await ld.QuotaAsync(token).ConfigureAwait(false);
                    if (z.Status==Status.Ok)
                    {
                        Sizes.AvailableSize += z.AvailableSize;
                        Sizes.UsedSize += z.UsedSize;
                        Sizes.TotalSize += z.TotalSize;
                    }
                }
                catch (Exception) //Cdrom and others
                {
                    //ignored
                }
            }
            return await Task.FromResult(Sizes).ConfigureAwait(false);
        }

        public Task<IObject> ResolveAsync(string path, CancellationToken token = default(CancellationToken))
        {
            try
            {
                // Allow either and convert to OS desired later
                if (path.StartsWith("\\\\") || path.StartsWith("//"))
                {
                    int idx = path.IndexOf(System.IO.Path.DirectorySeparatorChar, 2);
                    if (idx >= 0)
                    {
                        idx = path.IndexOf(System.IO.Path.DirectorySeparatorChar, idx + 1);
                        if (idx < 0)
                            idx = path.Length;
                    }
                    else
                        idx = path.Length;
                    string share = path.Substring(0, idx);
                    if (!Directory.Exists(share))
                        return Task.FromResult<IObject>(new EmptyObject { Status = Status.NotFound, Error = "Not found" });
                    if (!FS.Directories.Any(a => a.FullName.Equals(share, StringComparison.InvariantCultureIgnoreCase)))
                        FS.AddUncPath(share);
                }
            }
            catch (Exception e)
            {
                // Last ditch effort to catch errors, this needs to always succeed.
                return Task.FromResult<IObject>(new EmptyObject { Status = Status.SystemError, Error = e.Message });
            }
            if (File.Exists(path) || Directory.Exists(path))
            {
                FileAttributes attr = File.GetAttributes(path);

                // It's a directory
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    return Task.FromResult<IObject>(new LocalDirectory(new DirectoryInfo(path), FS));

                // It's a file
                return Task.FromResult<IObject>(new LocalFile(new FileInfo(path), FS));
            }
            return Task.FromResult<IObject>(new EmptyObject { Status = Status.NotFound, Error = "Not found" });
           
        }



        public FileSystemSizes Sizes { get; private set; }


    }
}
