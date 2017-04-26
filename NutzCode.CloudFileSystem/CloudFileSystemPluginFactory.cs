using System;
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

        //TODO Move to settings
        public const int MaximumNumberOfInactiveStreams = 40;
        public const int BlockSize = 64*1024;
        public const int MaxWaitBlockDistance = 2;
        public const int DirectoryTreeCacheSize = 1024;

        [ImportMany(typeof(ICloudPlugin))]
        public IEnumerable<ICloudPlugin> List { get; set; }

        public WebDataProvider WebDataProvider = new WebDataProvider(MaximumNumberOfInactiveStreams, BlockSize);

        public CloudFileSystemPluginFactory()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            string codebase = assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codebase);
            string dirname = Pri.LongPath.Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path).Replace("/",$"{System.IO.Path.DirectorySeparatorChar}"));
            if (dirname != null)
            {

                AggregateCatalog catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new AssemblyCatalog(assembly));
                catalog.Catalogs.Add(new DirectoryCatalog(dirname,"NutzCode.CloudFileSystem.Plugins.*.dll"));
                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
        }
    }
}
