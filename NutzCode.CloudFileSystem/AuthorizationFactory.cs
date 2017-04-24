using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NutzCode.Libraries.Web.StreamProvider;

namespace NutzCode.CloudFileSystem
{
    public class AuthorizationFactory
    {
        private static AuthorizationFactory _instance;
        public static AuthorizationFactory Instance => _instance ?? (_instance = new AuthorizationFactory());


        [Import(typeof(IAppAuthorization))]
        public IAppAuthorization AuthorizationProvider { get; set; }


        public AuthorizationFactory(string dll=null)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            string codebase = assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codebase);
            string dirname = Pri.LongPath.Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path)
                .Replace("/", $"{System.IO.Path.DirectorySeparatorChar}"));
            if (dirname != null)
            {
                AggregateCatalog catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new AssemblyCatalog(assembly));
                if (dll!=null)
                    catalog.Catalogs.Add(new DirectoryCatalog(dirname, dll));
                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
        }
    }
}
