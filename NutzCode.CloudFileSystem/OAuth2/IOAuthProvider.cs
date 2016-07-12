using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem.OAuth2
{
    [InheritedExport]
    public interface IOAuthProvider
    {
        string Name { get; }
        Task<AuthResult> Login(AuthRequest request);
    }

    public class AuthResult
    {
        public string Code { get; set; }
        public List<string> Scopes { get; set; }
        public bool HasError { get; set; }
        public string ErrorString { get; set; }
    }

    public class AuthRequest
    {
        public string Name { get; set; }
        public string LoginUrl { get; set; }
        public string ClientId { get; set; }
        public List<string> Scopes { get; set; }
        public string RedirectUri { get; set; }
    }

    public class OAuthProviderFactory
    {
        [ImportMany(typeof(IOAuthProvider))]
        public IEnumerable<IOAuthProvider> List { get; set; }


        public OAuthProviderFactory()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string dirname = System.IO.Path.GetDirectoryName(assembly.GetName().CodeBase);
            if (dirname != null)
            {
                if (dirname.StartsWith(@"file:\"))
                    dirname = dirname.Substring(6);
                AggregateCatalog catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new AssemblyCatalog(assembly));
                catalog.Catalogs.Add(new DirectoryCatalog(dirname, "NutzCode.CloudFileSystem.OAuth.*.dll"));
                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
        }
    }
}
