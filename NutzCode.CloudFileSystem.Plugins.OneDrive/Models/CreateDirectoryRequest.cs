using System.Runtime.Serialization;

namespace NutzCode.CloudFileSystem.Plugins.OneDrive.Models
{
    public class CreateDirectoryRequest
    {
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }
        [DataMember(Name = "folder", IsRequired = true)]
        public Folder Folder { get; set; }
    }
}
