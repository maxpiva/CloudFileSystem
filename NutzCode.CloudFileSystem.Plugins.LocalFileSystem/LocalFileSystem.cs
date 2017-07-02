﻿using System;
 using System.Collections.Generic;
 using System.Linq;
using System.Threading.Tasks;
using Path = Pri.LongPath.Path;
using Directory = Pri.LongPath.Directory;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;
using File = Pri.LongPath.File;
using FileSystemInfo = Pri.LongPath.FileSystemInfo;
using FileInfo = Pri.LongPath.FileInfo;
using Stream = System.IO.Stream;
using FileAttributes = System.IO.FileAttributes;

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

        public static async Task<FileSystemResult<IFileSystem>> Create(string name)
        {
            LocalFileSystem l = new LocalFileSystem();
            l.fname = name;
            FileSystemResult r = await l.PopulateAsync();
            if (!r.IsOk)
                return new FileSystemResult<IFileSystem>(r.Error);
            return new FileSystemResult<IFileSystem>(l);
        }

        public async Task<FileSystemResult<FileSystemSizes>> QuotaAsync()
        {
            Sizes = new FileSystemSizes();
            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (LocalDrive ld in IntDirectories)
            {
                try
                {
                    Sizes.AvailableSize += ld.Drive.AvailableFreeSpace;
                    Sizes.UsedSize += ld.Drive.TotalSize - ld.Drive.AvailableFreeSpace;
                    Sizes.TotalSize += ld.Drive.TotalSize;
                }
                catch (Exception) //Cdrom and others
                {
                    //ignored
                }
            }
            return await Task.FromResult(new FileSystemResult<FileSystemSizes>(Sizes));
        }

        public async Task<FileSystemResult<IObject>> ResolveAsync(string path)
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
                        return new FileSystemResult<IObject>("Not found");
                    if (FS.Directories.All(a => !a.FullName.Equals(share, StringComparison.InvariantCultureIgnoreCase)))
                        FS.AddUncPath(share);
                    path = path.Replace(share, share.Replace('\\', '*'));
                    path = path.Replace(share, share.Replace('/', '*'));
                }
            }
            catch (Exception e)
            {
                // Last ditch effort to catch errors, this needs to always succeed.
                return new FileSystemResult<IObject>(e.Message);
            }

            return await Refs.ObjectFromPath(this, path);
        }

        public FileSystemSizes Sizes { get; private set; }


        public async Task<FileSystemResult<List<IDirectory>>> GetRootsAsync()
        {
            try
            {
                LocalRoot l = new LocalRoot(FS);
                await l.PopulateAsync();
                return new FileSystemResult<List<IDirectory>>(l.IntDirectories.Cast<IDirectory>().ToList());
            }
            catch (Exception e)
            {
                // Last ditch effort to catch errors, this needs to always succeed.
                return new FileSystemResult<List<IDirectory>>(e.Message);
            }
        }

        public FileSystemResult<List<IDirectory>> GetRoots()
        {
            return Task.Run(async () => await GetRootsAsync()).Result;
        }
    }
}
