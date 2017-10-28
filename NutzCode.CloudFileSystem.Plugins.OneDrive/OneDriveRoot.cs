namespace NutzCode.CloudFileSystem.Plugins.OneDrive
{
    public class OneDriveRoot : OneDriveDirectory
    {
        public override string Id => "";
        internal string FsName=string.Empty;
        

        public OneDriveRoot(OneDriveFileSystem fs) : base(string.Empty, fs)
        {
            IsRoot = true;
        }

        public override string Name => FsName;

    }
}
