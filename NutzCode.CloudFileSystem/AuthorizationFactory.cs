using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using System.Runtime.Loader;

namespace NutzCode.CloudFileSystem
{
    public class AuthorizationFactory
    {
        private static AuthorizationFactory _instance;
        public static AuthorizationFactory Instance => _instance ?? (_instance = new AuthorizationFactory());


        [Import]
        public IAppAuthorization AuthorizationProvider { get; set; }

        public AuthorizationFactory(string dll = null)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            List<Assembly> assemblies = new List<Assembly> {assembly};
            if (dll!=null)
                assemblies.Add(AssemblyLoadContext.Default.LoadFromAssemblyPath(dll));
            ContainerConfiguration configuration = new ContainerConfiguration().WithAssemblies(assemblies);
            using (CompositionHost container = configuration.CreateContainer())
            {
                AuthorizationProvider = container.GetExport<IAppAuthorization>();
            }
        }        
    }
}
