using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem
{
    public interface IOwnDirectory<T,S> where T : IFile where S : IDirectory
    {
        List<S> IntDirectories { get; set; }
        List<T> IntFiles { get; set; }

    }
}
