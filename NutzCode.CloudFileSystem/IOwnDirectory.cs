using System.Collections.Generic;

namespace NutzCode.CloudFileSystem
{
    public interface IOwnDirectory<T,S> where T : IFile where S : IDirectory
    {
        List<S> IntDirectories { get; set; }
        List<T> IntFiles { get; set; }

    }
}
