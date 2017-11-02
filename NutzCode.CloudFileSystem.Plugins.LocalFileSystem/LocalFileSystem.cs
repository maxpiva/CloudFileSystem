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

        //TODO locking?
        public override async Task<FileSystemResult<FileSystemSizes>> QuotaAsync()
        {
            Sizes = new FileSystemSizes();
            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (DirectoryImplementation ld in IntDirectories)
            {
                try
                {
                    FileSystemResult<FileSystemSizes> z = await ld.QuotaAsync();
                    if (z.IsOk)
                    {
                        Sizes.AvailableSize += z.Result.AvailableSize;
                        Sizes.UsedSize += z.Result.UsedSize;
                        Sizes.TotalSize += z.Result.TotalSize;
                    }
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
            if (File.Exists(path) || Directory.Exists(path))
            {
                FileAttributes attr = File.GetAttributes(path);

                // It's a directory
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    return new FileSystemResult<IObject>(new LocalDirectory(new DirectoryInfo(path), FS));

                // It's a file
                return new FileSystemResult<IObject>(new LocalFile(new FileInfo(path), FS));
            }

            return new FileSystemResult<IObject>("Not found");

            return await Refs.ObjectFromPath(this, path) ?? new FileSystemResult<IObject>("Not found");
        }

        public FileSystemSizes Sizes { get; private set; }


    }
}
