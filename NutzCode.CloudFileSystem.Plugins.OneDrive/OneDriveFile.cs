using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NutzCode.Libraries.Web;

namespace NutzCode.CloudFileSystem.Plugins.OneDrive
{
    public class OneDriveFile : OneDriveObject, IFile
    {
        public const string FileDownload = "{0}/drive/items/{1}/content";

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

        public string MD5 => string.Empty;
       
        public string SHA1
        {
            get
            {
                string value;
                TryGetMetadataValue("sha1", out value);
                return value;
            }
        }

        public virtual string ContentType
        {
            get
            {
                string value;
                TryGetMetadataValue("contentType", out value);
                return value;
            }
        }

        public virtual string Extension
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                    return Path.GetExtension(Name).Replace(".", string.Empty);
                return string.Empty;
            }
        }


        internal override SeekableWebParameters GetSeekableWebParameters(long position)
        {
            string url = FileDownload.FormatRest(Id);
            SeekableWebParameters pars = FS.OAuth.CreateSeekableWebParameters(this, url, GetKey());
            if (position != 0)
            {
                pars.HasRange = true;
                pars.RangeStart = position;
                pars.RangeEnd = Size - 1;
            }
            return pars;
        }


        public OneDriveFile(string parentpath, OneDriveFileSystem fs) : base(parentpath, fs, OneDriveMappings.Maps)
        {
        }
    }
}
