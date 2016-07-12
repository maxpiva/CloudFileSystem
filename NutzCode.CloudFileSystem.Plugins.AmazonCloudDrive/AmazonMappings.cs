namespace NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive
{
    public class AmazonMappings : Mappings
    {
        public static AmazonMappings Maps = new AmazonMappings();

        public AmazonMappings()
        {
            Add("size", "contentProperties.size");
            Add("contentType", "contentProperties.contentType");
            Add("extension", "contentProperties.extension");
            Add("md5", "contentProperties.md5");
            Add("application", "createdBy");
        }
    }
}
