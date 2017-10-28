using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
#if PRILONGPATH
using Pri.LongPath;
using SearchOption = System.IO.SearchOption;
#else
using System.IO;
#endif
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
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

        [ImportMany]
        public IEnumerable<ICloudPlugin> List { get; set; }

        public WebDataProvider WebDataProvider = new WebDataProvider(MaximumNumberOfInactiveStreams, BlockSize);

        public CloudFileSystemPluginFactory()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            string executableLocation = assembly.Location;
            string dirname = Path.GetDirectoryName(executableLocation);
            if (dirname != null)
            {
                List<Assembly> assemblies = Directory.GetFiles(dirname, "*.dll", SearchOption.AllDirectories)
                    .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath).ToList();
                assemblies.Add(assembly);
                ContainerConfiguration configuration = new ContainerConfiguration().WithAssemblies(assemblies);
                using (CompositionHost container = configuration.CreateContainer())
                {
                    List = container.GetExports<ICloudPlugin>();
                }
            }
        }
    }
}
