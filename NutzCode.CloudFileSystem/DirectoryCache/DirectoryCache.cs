using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        private async Task<FileSystemResult<IObject>> GetFromPath(IDirectory d, string path)
        {
            try
            {
                bool populated = false;
                if (path.StartsWith($"{System.IO.Path.DirectorySeparatorChar}"))
                    path = path.Substring(1);
                if (!d.IsPopulated)
                {
                    FileSystemResult n = await d.PopulateAsync();
                    if (!n.IsOk)
                        return new FileSystemResult<IObject>(n.Error);
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
                        FileSystemResult n = await d.PopulateAsync();
                        if (!n.IsOk)
                            return new FileSystemResult<IObject>(n.Error);
                        populated = true;
                    }
                    if (fnd == null)
                        return new FileSystemResult<IObject>("File Not Found");
                    return await GetFromPath(fnd, path);
                }
                while (true)
                {
                    foreach (IFile dn in d.Files)
                    {
                        if (dn.Name.Equals(path.Replace("*", $"{System.IO.Path.DirectorySeparatorChar}"), StringComparison.InvariantCultureIgnoreCase))
                            return new FileSystemResult<IObject>(dn);
                    }
                    foreach (IDirectory dn in d.Directories)
                    {
                        if (dn.Name.Equals(path.Replace("*", $"{System.IO.Path.DirectorySeparatorChar}"), StringComparison.InvariantCultureIgnoreCase))
                            return new FileSystemResult<IObject>(dn);
                    }
                    if (populated)
                        break;
                    FileSystemResult n = await d.PopulateAsync();
                    if (!n.IsOk)
                        return new FileSystemResult<IObject>(n.Error);
                    populated = true;
                }
                return new FileSystemResult<IObject>("File Not Found");
            }
            catch (Exception e)
            {
                //FS Errors=NOT FOUND
                return new FileSystemResult<IObject>("File Not Found");
            }
        
        }

        public async Task<FileSystemResult<IObject>> ObjectFromPath(IFileSystem fs, string fullpath)
        {
            // take both and convert
            fullpath = fullpath.Replace("/", $"{System.IO.Path.DirectorySeparatorChar}");
            fullpath = fullpath.Replace("\\", $"{System.IO.Path.DirectorySeparatorChar}");
            if (fullpath.EndsWith($"{System.IO.Path.DirectorySeparatorChar}"))
                fullpath = fullpath.Substring(0, fullpath.Length - 1);
            string originalPath = fullpath;

            if (fullpath.Equals($"{System.IO.Path.DirectorySeparatorChar}") || fullpath == string.Empty ||
                fullpath.Equals(fs.FullName, StringComparison.InvariantCultureIgnoreCase))
                return new FileSystemResult<IObject>(fs);

            IDirectory d = this[fullpath.Replace("*", $"{System.IO.Path.DirectorySeparatorChar}")];
            string lastpart = string.Empty;
            if (d != null)
                return new FileSystemResult<IObject>(d);
            while (fullpath != "")
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
            if (Type.GetType("Mono.Runtime") != null)
                foreach (IDirectory directory in fs.Directories)
                {
                    if (!originalPath.StartsWith(directory.Name, StringComparison.InvariantCulture)) continue;

                    if (originalPath.Equals(directory.Name, StringComparison.InvariantCulture))
                        return new FileSystemResult<IObject>(directory);

                    lastpart = originalPath.Substring(directory.Name.Length);
                    d = directory;
                }

            return await GetFromPath(d, lastpart);
        }
    }
}
