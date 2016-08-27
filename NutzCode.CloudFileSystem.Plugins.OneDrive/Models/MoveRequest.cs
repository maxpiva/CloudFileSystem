using System.Runtime.Serialization;

namespace NutzCode.CloudFileSystem.Plugins.OneDrive.Models
{
    public class MoveRequest
    {
        [DataMember(Name = "parentReference", IsRequired = true)]
        public ItemReference ParentReference { get; set; }
 
    }
}
