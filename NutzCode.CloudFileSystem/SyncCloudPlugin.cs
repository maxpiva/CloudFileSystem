using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;

namespace NutzCode.CloudFileSystem
{
    public abstract class SyncCloudPlugin
    {
        public abstract Task<FileSystemResult<IFileSystem>> InitAsync(string filesystemname, IOAuthProvider oAuthProvider, Dictionary<string, object> settings, string userauthorization = null);


        public FileSystemResult<IFileSystem> Init(string filesystemname, IOAuthProvider oAuthProvider, Dictionary<string, object> settings, string userauthorization = null)
        {
            return Task.Run(async () => await InitAsync(filesystemname, oAuthProvider, settings, userauthorization)).Result;
        }
    }
}
