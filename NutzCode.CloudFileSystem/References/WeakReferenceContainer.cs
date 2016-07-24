using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("NutzCode.CloudFileSystem.AmazonCloudDrive"), InternalsVisibleTo("NutzCode.CloudFileSystem.GoogleDrive")]
namespace NutzCode.CloudFileSystem.References
{
    public class WeakReferenceContainer
    {
        private List<WeakReference<IDirectory>> _directories = new List<WeakReference<IDirectory>>();
       
        internal void AddWeakReferenceDirectory(IDirectory dir)
        {
            foreach (WeakReference<IDirectory> a in _directories.ToList())
            {
                IDirectory d;
                if (!a.TryGetTarget(out d))
                    _directories.Remove(a);
            }
            _directories.Add(new WeakReference<IDirectory>(dir));
        }
        internal void RemoveWeakReferenceDirectory(IDirectory dir)
        {
            foreach (WeakReference<IDirectory> a in _directories.ToList())
            {
                IDirectory d;
                if (a.TryGetTarget(out d))
                {
                    if (d == dir)
                    {
                        _directories.Remove(a);
                        return;
                    }   

                }
            }
        }
        public virtual IObject ObjectFromPath(string fullpath)
        {
            fullpath = fullpath.Replace("/", "\\");
            if (!fullpath.StartsWith("\\"))
                fullpath = "\\" + fullpath;
            foreach (WeakReference<IDirectory> a in _directories.ToList())
            {
                IDirectory d;
                if (a.TryGetTarget(out d))
                {
                    if (d.FullName.Equals(fullpath, StringComparison.InvariantCultureIgnoreCase))
                        return d;
                    if (d.IsPopulated)
                    {
                        foreach (IFile f in d.Files)
                        {
                            if (f.FullName.Equals(fullpath, StringComparison.InvariantCultureIgnoreCase))
                            {
                                return f;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public async Task<FileSystemResult<IObject>> FromPath(IDirectory root, string path)
        {
            IObject ret = ObjectFromPath(path);
            if (ret == null)
            {
                ret = await root.ObjectFromPath(path);
                return new FileSystemResult<IObject>(ret);

            }
            return new FileSystemResult<IObject>(ret);
        }

    }
}
