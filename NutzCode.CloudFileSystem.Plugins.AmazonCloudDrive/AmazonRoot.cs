namespace NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive
{
    public class AmazonRoot : AmazonDirectory
    {
        internal string FsName = string.Empty;

        public AmazonRoot(AmazonFileSystem fs) : base(string.Empty, fs)
        {
            IsRoot = true;
        }

        public override string Name => FsName;
    }
}
