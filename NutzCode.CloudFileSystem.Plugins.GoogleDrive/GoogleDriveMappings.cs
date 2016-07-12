namespace NutzCode.CloudFileSystem.Plugins.GoogleDrive
{
    public class GoogleDriveMappings : Mappings
    {
        public static GoogleDriveMappings Maps = new GoogleDriveMappings();

        public GoogleDriveMappings()
        {
            Add("name","title");
            Add("size", "fileSize");
            Add("contentType", "mimeType");
            Add("extension", "fileExtension");
            Add("md5", "md5Checksum");
        }
    }
}
