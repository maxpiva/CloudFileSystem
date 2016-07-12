using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NutzCode.CloudFileSystem
{
    public static class Extensions
    {
        public static string ContentFromExtension(string extension)
        {
            if (extension.StartsWith("."))
                extension = extension.Substring(1);
            return MimeTypes.MimeTypeMap.GetMimeType(extension);
        }




        public static string ToString(this ICloudPlugin plugin)
        {
            return plugin.Name;
        }

        public static string Application(this IFile file)
        {
            string v = null;
            if (file.TryGetMetadataValue("application", out v))
                return v;
            return null;
        }
        /*
        public static string FullPath(this IObject obj)
        {
            string path = string.Empty;
            IDirectory d;
            if (obj is IFile)
            {
                path = ((IFile) obj).Name;
                d = ((IFile) obj).Parent;
            }
            else
                d = (IDirectory) obj;
            while (!d.IsRoot)
            {
                if (d.Name.EndsWith(""))
                    path = d.Name + path;
                else
                    path = d.Name + "" + path;
                d = d.Parent;
            }
            return path;
        }
        */

        public static string FormatRest(this string template, params object[] objs)
        {
            return string.Format(template, objs.Select(o => o.ToString()).Select(n => !n.StartsWith("?") ? HttpUtility.UrlEncode(n) : n).Cast<object>().ToArray());
        }

        public static async Task<IObject> ObjectFromPath(this IDirectory dir, string fullname)
        {
            while (!dir.IsRoot)
            {
                dir = dir.Parent;
            }
            string[] parts = fullname.Split('\\');
            int start = 0;
            bool repeat;
            do
            {
                repeat = false;
                if (!dir.IsPopulated)
                    await dir.Populate();
                foreach (IDirectory d in dir.Directories)
                {
                    if (d.Name.Equals(parts[start], StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (start == parts.Length - 1)
                            return d;
                        repeat = true;
                        start++;
                        dir = d;
                        break;
                    }
                }
                if ((!repeat) && (start == parts.Length-1))
                {
                    foreach (IFile d in dir.Files)
                    {
                        if (d.Name.Equals(parts[start], StringComparison.InvariantCultureIgnoreCase))
                        {
                            return d;
                        }
                    }
                }
            } while (repeat);
            return null;
        }
        
        public static string HashFromExtendedFile(string file, string type="md5")
        {
            if (File.Exists(file + "."+type))
            {
                string[] lines = File.ReadAllLines(file + "."+type);
                foreach (string s in lines)
                {
                    if ((s.Length >= 32) && (s[0] != '#'))
                    {
                        return s.Substring(0, 32);
                    }
                }
            }
            FileInfo f = new FileInfo(file);
            string dir = string.Empty;
            if (f.Directory != null)
                dir = f.Directory.Name;
            string bas = Path.Combine(Path.GetDirectoryName(file) ?? string.Empty, dir);
            if (File.Exists(bas + ".md5"))
            {
                string[] lines = File.ReadAllLines(bas + "."+type);
                foreach (string s in lines)
                {
                    if ((s.Length >= 35) && (s[0] != '#'))
                    {
                        string hash = s.Substring(0, 32);
                        string fname = s.Substring(32).Replace("*", string.Empty).Trim();
                        if (string.Equals(f.Name, fname, StringComparison.InvariantCultureIgnoreCase))
                            return hash;
                    }
                }

            }
            if (File.Exists(bas + ""))
            {
                string[] lines = File.ReadAllLines(bas + "");
                bool hash = false;
                for (int x = 0; x < lines.Length; x++)
                {
                    string s = lines[x];
                    if ((s.Length > 5) && (s.StartsWith("#"+type+"#")))
                        hash = true;
                    else if ((s.Length >= 35) && (s[0] != '#') && hash)
                    {
                        string md = s.Substring(0, 32);
                        string fname = s.Substring(32).Replace("*", string.Empty).Trim();
                        if (string.Equals(f.Name, fname, StringComparison.InvariantCultureIgnoreCase))
                            return md;
                        hash = false;
                    }
                    else
                        hash = false;
                }

            }
            return string.Empty;
        }
    }
}
