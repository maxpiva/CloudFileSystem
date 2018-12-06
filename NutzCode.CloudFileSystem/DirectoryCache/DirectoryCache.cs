using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NutzCode.Libraries.Web.StreamProvider;

namespace NutzCode.CloudFileSystem.DirectoryCache
{
    public class DirectoryCache : LRUCache<string,IDirectory>
    {
        public DirectoryCache(int capacity) : base(capacity)
        {
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddDirectories(List<IDirectory> dirs, IDirectory parent)
        {
            List<string> oldirs = parent.Directories.Select(a => a.FullName).ToList();
            oldirs.Except(dirs.Select(a => a.FullName)).ToList().ForEach(Remove);
            dirs.ForEach(a=>this[a.FullName]=a);
        }

        private async Task<IObject> GetFromPathAsync(IDirectory d, string path, CancellationToken token)
        {
            try
            {
                bool populated = false;
                if (path.StartsWith($"{System.IO.Path.DirectorySeparatorChar}"))
                    path = path.Substring(1);
                if (!d.IsPopulated)
                {
                    FileSystemResult n = await d.PopulateAsync(token).ConfigureAwait(false);
                    if (n.Status != Status.Ok)
                        return new EmptyObject {Status = n.Status, Error = n.Error};
                    populated = true;
                }
                if (path.Contains($"{System.IO.Path.DirectorySeparatorChar}"))
                {
                    int idx = path.IndexOf($"{System.IO.Path.DirectorySeparatorChar}", StringComparison.InvariantCulture);
                    string dirname = path.Substring(0, idx);
                    path = path.Substring(idx + 1);
                    IDirectory fnd = null;
                    while (true)
                    {
                        foreach (IDirectory dn in d.Directories)
                        {
                            if (dn.Name.Equals(dirname.Replace("*", $"{System.IO.Path.DirectorySeparatorChar}"), StringComparison.InvariantCultureIgnoreCase))
                            {
                                fnd = dn;
                                break;
                            }
                        }
                        if (fnd != null)
                            break;
                        if (populated)
                            break;
                        FileSystemResult n = await d.PopulateAsync(token).ConfigureAwait(false);
                        if (n.Status != Status.Ok)
                            return new EmptyObject { Status = n.Status, Error = n.Error };
                        populated = true;
                    }
                    if (fnd == null)
                        return new EmptyObject { Status = Status.NotFound, Error = "File Not Found" };
                    return await GetFromPathAsync(fnd, path, token).ConfigureAwait(false);
                }
                while (true)
                {
                    foreach (IFile dn in d.Files)
                    {
                        if (dn.Name.Equals(path.Replace("*", $"{System.IO.Path.DirectorySeparatorChar}"), StringComparison.InvariantCultureIgnoreCase))
                            return dn;
                    }
                    foreach (IDirectory dn in d.Directories)
                    {
                        if (dn.Name.Equals(path.Replace("*", $"{System.IO.Path.DirectorySeparatorChar}"), StringComparison.InvariantCultureIgnoreCase))
                            return dn;
                    }
                    if (populated)
                        break;
                    FileSystemResult n = await d.PopulateAsync(token).ConfigureAwait(false);
                    if (n.Status!=Status.Ok)
                        return new EmptyObject { Status = n.Status, Error = n.Error };
                    populated = true;
                }
                return new EmptyObject { Status = Status.NotFound, Error = "File Not Found" };
            }
            catch (Exception)
            {
                //FS Errors=NOT FOUND
                return new EmptyObject { Status = Status.NotFound, Error = "File Not Found" };
            }
        
        }

        public Task<IObject> ObjectFromPathAsync(IFileSystem fs, string fullpath, CancellationToken token)
        {
            // take both and convert
            fullpath = fullpath.Replace("/", $"{System.IO.Path.DirectorySeparatorChar}");
            fullpath = fullpath.Replace("\\", $"{System.IO.Path.DirectorySeparatorChar}");
            if (fullpath.EndsWith($"{System.IO.Path.DirectorySeparatorChar}"))
                fullpath = fullpath.Substring(0, fullpath.Length - 1);
            string originalPath = fullpath;

            if (fullpath.Equals($"{System.IO.Path.DirectorySeparatorChar}") || fullpath == string.Empty ||
                fullpath.Equals(fs.FullName, StringComparison.InvariantCultureIgnoreCase))
                return Task.FromResult<IObject>(fs);

            IDirectory d = this[fullpath.Replace("*", $"{System.IO.Path.DirectorySeparatorChar}")];
            string lastpart = string.Empty;
            if (d != null)
                return Task.FromResult<IObject>(d);
            while (fullpath != string.Empty)
            {
                int idx = fullpath.LastIndexOf($"{System.IO.Path.DirectorySeparatorChar}", StringComparison.InvariantCulture);
                if (idx < 0)
                {
                    lastpart = fullpath + lastpart;
                    fullpath = string.Empty;
                }
                else
                {
                    lastpart = fullpath.Substring(idx) + lastpart;
                    fullpath = fullpath.Substring(0, idx);
                }
                d = this[fullpath.Replace("*", $"{System.IO.Path.DirectorySeparatorChar}")];
                if (d != null)
                    break;
            }
            if (d == null)
                d = fs;
            if (Extensions.IsLinux)
            {
                foreach (IDirectory directory in fs.Directories)
                {
                    if (!originalPath.StartsWith(directory.Name, StringComparison.InvariantCulture)) continue;

                    if (originalPath.Equals(directory.Name, StringComparison.InvariantCulture))
                        return Task.FromResult((IObject)directory);

                    lastpart = originalPath.Substring(directory.Name.Length);
                    d = directory;
                }
            }
            return GetFromPathAsync(d, lastpart, token);
        }
    }
}
