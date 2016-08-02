using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{
    [InheritedExport]
    public interface IAppAuthorization
    {
        Dictionary<string, object> Get(string provider);
    }
}
