using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using NutzCode.Libraries.Web.StreamProvider;

namespace NutzCode.CloudFileSystem
{


    public class CloudFileSystemPluginFactory
    {
        private static CloudFileSystemPluginFactory _instance;
        public static CloudFileSystemPluginFactory Instance => _instance ?? (_instance = new CloudFileSystemPluginFactory());

        public const int MaximumNumberOfInactiveStreams = 40;
        public const int BlockSize = 64*1024;
        public const int MaxWaitBlockDistance = 2;


        [ImportMany(typeof(ICloudPlugin))]
        public IEnumerable<ICloudPlugin> List { get; set; }

        public WebDataProvider WebDataProvider = new WebDataProvider(MaximumNumberOfInactiveStreams, BlockSize);

        public CloudFileSystemPluginFactory()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string dirname = System.IO.Path.GetDirectoryName(assembly.GetName().CodeBase);
            if (dirname != null)
            {
                if (dirname.StartsWith(@"file:\"))
                    dirname = dirname.Substring(6);
                AggregateCatalog catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new AssemblyCatalog(assembly));
                catalog.Catalogs.Add(new DirectoryCatalog(dirname,"NutzCode.CloudFileSystem.Plugins.*.dll"));
                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
        }

    }
}
