using System;

namespace NutzCode.CloudFileSystem
{
    [Flags]
    public enum ObjectAttributes
    {
        Directory = 1,
        Hidden = 2,
        Restricted = 4,
        Starred = 8,
        Viewed = 16,
        Trashed = 32,
        Editable = 64,
        Shareable = 128,
        Shared = 256,
        Asset = 1024
    }
}
