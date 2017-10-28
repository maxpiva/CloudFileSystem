using System.Collections.Generic;

namespace NutzCode.CloudFileSystem
{

    public interface IAppAuthorization
    {
        Dictionary<string, object> Get(string provider);
    }
}
