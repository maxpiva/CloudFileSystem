namespace NutzCode.CloudFileSystem.Plugins.OneDrive
{
    public class OneDriveMappings : Mappings
    {
        public static OneDriveMappings Maps = new OneDriveMappings();

        public OneDriveMappings()
        {
            Add("contentType", "file.mimeType");
            Add("sha1", "file.hashes.sha1Hash");
        }
    }
}
