using System;
using System.Diagnostics.CodeAnalysis;

namespace NutzCode.CloudFileSystem
{
    [Flags]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum SupportedFlags
    {
        Nothing=0,
        Assets = 1,
        Properties = 2,
        MD5 = 8,
        SHA1 = 16
    }
}
