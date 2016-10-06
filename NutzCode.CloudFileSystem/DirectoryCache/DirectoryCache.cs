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
            bool populated = false;
            if (path.StartsWith("\\"))
                path = path.Substring(1);
            if (!d.IsPopulated)
            {
                FileSystemResult n = await d.PopulateAsync();
                if (!n.IsOk)
                    return new FileSystemResult<IObject>(n.Error);
                populated = true;
            }
            if (path.Contains("\\"))
            {
                int idx = path.IndexOf("\\", StringComparison.InvariantCulture);
                string dirname = path.Substring(0, idx);
                path = path.Substring(idx + 1);
                IDirectory fnd = null;
                while(true)
                {
                    foreach (IDirectory dn in d.Directories)
                    {
                        if (dn.Name.Equals(dirname.Replace("*","\\"), StringComparison.InvariantCultureIgnoreCase))
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
                    if (dn.Name.Equals(path.Replace("*","\\"), StringComparison.InvariantCultureIgnoreCase))
                        return new FileSystemResult<IObject>(dn);
                }
                foreach (IDirectory dn in d.Directories)
                {
                    if (dn.Name.Equals(path.Replace("*", "\\"), StringComparison.InvariantCultureIgnoreCase))
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

        public async Task<FileSystemResult<IObject>> ObjectFromPath(IFileSystem fs, string fullpath)
        {

            fullpath = fullpath.Replace("/", "\\");
            if (fullpath.EndsWith("\\"))
                fullpath = fullpath.Substring(0, fullpath.Length - 1);
            if (fullpath=="/" || fullpath==string.Empty || fullpath==fs.FullName)
                return new FileSystemResult<IObject>(fs);

            IDirectory d = this[fullpath.Replace("*", "\\")];
            string lastpart = string.Empty;
            if (d != null)
                return new FileSystemResult<IObject>(d);
            while (fullpath != "")
            {
                int idx = fullpath.LastIndexOf("\\",StringComparison.InvariantCulture);
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
                d = this[fullpath.Replace("*","\\")];
                if (d != null)
                    break;
            }
            if (d == null)
                d = fs;
            return await GetFromPath(d, lastpart);
        }
    }
}
