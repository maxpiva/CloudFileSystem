namespace NutzCode.CloudFileSystem.OAuth2
{
    public class LocalUserSettingWithCode : LocalUserSettings
    {
        public string Code { get; set; }
        public string OriginalRedirectUri { get; set; }

    }
}