using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{
    public static class CloudPath
    {
        public static string Combine(params string[] strs)
        {
            StringBuilder bld=new StringBuilder();
            for (int x = 0; x < strs.Length - 1; x++)
            {
                string val=strs[x].Replace("\\","/");
                bld.Append(val);
                if (!strs[x].EndsWith("/"))
                    bld.Append("/");
            }
            return bld.ToString();
        }

        public static string GetFileName(string str)
        {
            str = str.Replace("\\", "/");
            int idx = str.LastIndexOf("/", StringComparison.InvariantCulture);
            if (idx >= 0)
                return str.Substring(idx + 1);
            return str;
        }

        public static string GetDirectoryName(string str)
        {
            str = str.Replace("\\", "/");
            int idx = str.LastIndexOf("/", StringComparison.InvariantCulture);
            if (idx >= 0)
                return str.Substring(0, idx - 1);
            return str;
        }

        public static string GetFileNameWithoutExtension(string str)
        {
            str = GetDirectoryName(str);
            int idx = str.LastIndexOf("/", StringComparison.InvariantCulture);
            if (idx>0)
                return str.Substring(0, idx - 1);
            return str;
        }

    }
}
