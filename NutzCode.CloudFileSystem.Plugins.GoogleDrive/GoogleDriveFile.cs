using System;
using System.Collections.Generic;
using Stream = System.IO.Stream;
using System.Threading;
using System.Threading.Tasks;
using NutzCode.Libraries.Web;

namespace NutzCode.CloudFileSystem.Plugins.GoogleDrive
{
    public class GoogleDriveFile : GoogleDriveObject, IFile
    {

        public const string GoogleFileDownload = "https://www.googleapis.com/drive/v2/files/{0}?alt=media";

        public override long Size
        {
            get
            {
                long value;
                TryGetMetadataValue("size", out value);
                return value;
            }

        }

        public Task<FileSystemResult> OverwriteFileAsync(Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            throw new NotImplementedException();
        }

        public string MD5
        {
            get
            {
                string value;
                TryGetMetadataValue("md5", out value);
                return value;
            }
        }

        public string SHA1 => string.Empty;

        public string ContentType
        {
            get
            {
                string value;
                TryGetMetadataValue("contentType", out value);
                return value;
            }
        }

        public string Extension
        {
            get
            {
                string value;
                TryGetMetadataValue("extension", out value);
                return value;
            }
        }


        internal override SeekableWebParameters GetSeekableWebParameters(long position)
        {
            string url = GoogleFileDownload.FormatRest(Id);
            if (FS.AckAbuse)
                url += "&acknowledgeAbuse=true";
            SeekableWebParameters pars = FS.OAuth.CreateSeekableWebParameters(this, url, GetKey());
            if (position != 0)
            {
                pars.HasRange = true;
                pars.RangeStart = position;
                pars.RangeEnd = Size - 1;
            }
            return pars;
        }


        public GoogleDriveFile(string parentpath, GoogleDriveFileSystem fs) : base(parentpath, fs, GoogleDriveMappings.Maps)
        {
        }
    }
}
