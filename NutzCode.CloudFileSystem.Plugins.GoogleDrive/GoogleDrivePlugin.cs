﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;
using NutzCode.CloudFileSystem.Plugins.GoogleDrive.Properties;

namespace NutzCode.CloudFileSystem.Plugins.GoogleDrive
{
    public class GoogleDrivePlugin : ICloudPlugin
    {

        public string Name => "Google Drive";
        public byte[] Icon {
            get {
                using (MemoryStream ms = new MemoryStream())
                {
                    this.GetType().Assembly.GetManifestResourceStream($"{this.GetType().Namespace}.Resources.Image48x48png").CopyTo(ms);
                    return ms.GetBuffer();                
                }
            }
        }

        public PluginAuthData PluginAuthData => new PluginAuthData { LoginUri = GoogleDriveFileSystem.GoogleOAuthLogin, RequiredScopes = GoogleDriveFileSystem.GoogleScopes, ScopesCommaSeparated = false };


        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettings settings, string userauthorization, CancellationToken token=default(CancellationToken))
        {
            return GoogleDriveFileSystem.CreateAsync(filesystemname, settings, Name, userauthorization, token);
        }

        public Task<IFileSystem> InitAsync(string filesystemname, ProxyUserSettings settings, string userauthorization, CancellationToken token = default(CancellationToken))
        {
            return GoogleDriveFileSystem.CreateAsync(filesystemname, settings, Name, userauthorization, token);
        }
        public Task<IFileSystem> InitAsync(string filesystemname, LocalUserSettingWithCode settings, CancellationToken token = default(CancellationToken))
        {
            return GoogleDriveFileSystem.CreateAsync(filesystemname, settings, Name, null, token);
        }

       
    }
}
