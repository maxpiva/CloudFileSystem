using System;

namespace NutzCode.CloudFileSystem
{
    public class AuthorizationRequirement
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsRequired { get; set; }
    }
}
