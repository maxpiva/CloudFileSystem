

namespace NutzCode.CloudFileSystem.Plugins.GoogleDrive
{
    public class GoogleDriveRoot : GoogleDriveDirectory
    {
        public override string Id => "ROOT";
        internal string FsName=string.Empty;

        public GoogleDriveRoot(GoogleDriveFileSystem fs) : base(string.Empty, fs)
        {
            IsRoot = true;
        }

        public override string Name => FsName;

    }
}
