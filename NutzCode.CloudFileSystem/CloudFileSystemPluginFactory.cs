using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NutzCode.Libraries.Web.StreamProvider;

namespace NutzCode.CloudFileSystem
{
    public class CloudFileSystemPluginFactory
    {
        private static CloudFileSystemPluginFactory _instance;
        public static CloudFileSystemPluginFactory Instance => _instance ?? (_instance = new CloudFileSystemPluginFactory());

        //TODO Move to settings
        public const int MaximumNumberOfInactiveStreams = 40;
        public const int BlockSize = 64*1024;
        public const int MaxWaitBlockDistance = 2;
        public const int DirectoryTreeCacheSize = 1024;


        public IEnumerable<ICloudPlugin> List { get; set; }

        public WebDataProvider WebDataProvider = new WebDataProvider(MaximumNumberOfInactiveStreams, BlockSize);

        public CloudFileSystemPluginFactory()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            string executableLocation = assembly.Location;
            string dirname = Path.GetDirectoryName(executableLocation);
            List<ICloudPlugin> ls = new List<ICloudPlugin>();
            if (dirname != null)
            {
                List<Assembly> assemblies = Directory.GetFiles(dirname, "*.dll", SearchOption.AllDirectories).Select(s => {
                    try 
                    {
                        return Assembly.LoadFrom(s);
                    }
                    catch (System.BadImageFormatException)
                    {
                        return null;
                    }
                }).Where(s => s != null).ToList();
                assemblies.Add(assembly);
                foreach (Assembly a in assemblies)
                {
                    foreach (Type t in a.GetTypes())
                    {
                        if (typeof(ICloudPlugin).IsAssignableFrom(t) && !t.IsInterface)
                        {
                            ls.Add((ICloudPlugin)Activator.CreateInstance(t));
                        }
                    }
                }
            }

            List = ls;
        }
    }
}
