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
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem.OAuth2
{

    public interface IOAuthProvider
    {
        string Name { get; }
        Task<AuthResult> Login(AuthRequest request);
    }

}
