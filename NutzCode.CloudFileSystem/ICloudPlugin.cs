using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;
namespace NutzCode.CloudFileSystem
{
    [InheritedExport]
    public interface ICloudPlugin
    {
        string Name { get; }
        Bitmap Icon { get; }
        List<AuthorizationRequirement> AuthorizationRequirements { get; }
        Task<FileSystemResult<IFileSystem>> Init(string filesystemname, IOAuthProvider oAuthProvider, Dictionary<string, object> settings, string userauthorization = null);
    }
}
