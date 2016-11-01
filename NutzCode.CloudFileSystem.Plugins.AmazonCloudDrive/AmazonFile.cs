using System;
using System.Collections.Generic;
using Stream = System.IO.Stream;
using MemoryStream = System.IO.MemoryStream;
using System.Threading;
using System.Threading.Tasks;
using NutzCode.Libraries.Web;

namespace NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive
{
    public class AmazonFile : AmazonObject,IFile
    {
        public override long Size
        {
            get
            {
                long value;
                TryGetMetadataValue("size", out value);
                return value;
            }
            
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

        public async Task<FileSystemResult> OverwriteFileAsync(Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
#if DEBUG || EXPERIMENTAL
            string type;
            TryGetMetadataValue("kind", out type);
            FileSystemResult<IFile> f=await InternalCreateFile(Name, type,true,(AmazonObject)Parent,readstream,token,progress,properties);
            if (f.IsOk)
            {
                // ReSharper disable once PossibleInvalidCastException
                // ReSharper disable once SuspiciousTypeConversion.Global
                SetData((BaseObject) (object) f);
            }
            return f;
#else
            throw new NotSupportedException();
#endif
        }


        internal override SeekableWebParameters GetSeekableWebParameters(long position)
        {
            string url = AmazonNodeFile.FormatRest(FS.OAuth.EndPoint.ContentUrl, Id);
            SeekableWebParameters pars=FS.OAuth.CreateSeekableWebParameters(this, url, GetKey());
            if (position != 0)
            {
                pars.HasRange = true;
                pars.RangeStart = position;
                pars.RangeEnd = Size - 1;
            }
            return pars;
        }



        public AmazonFile(string parentpath, AmazonFileSystem fs) : base(parentpath, fs, AmazonMappings.Maps)
        {

        }

      
    }
}
