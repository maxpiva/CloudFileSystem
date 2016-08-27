using NutzCode.Libraries.Web;

namespace NutzCode.CloudFileSystem.Plugins.OneDrive
{
    public class OneDriveThumbAsset : OneDriveFile
    {
        private string _name;
        public override string Name => _name;
        private string _mime;
        public override string ContentType => _mime;
        private string _url;
        public int Width { get; private set; }
        public int Height { get; private set; }

        internal override SeekableWebParameters GetSeekableWebParameters(long position)
        {
            SeekableWebParameters pars = FS.OAuth.CreateSeekableWebParameters(this, _url, GetKey());
            if (position != 0)
            {
                pars.HasRange = true;
                pars.RangeStart = position;
                pars.RangeEnd = Size - 1;
            }
            return pars;
        }

        public OneDriveThumbAsset(string parentpath, OneDriveFileSystem fs, string name, string url, int width, int height, string mime) : base(parentpath, fs)
        {
            _name = name;
            _mime = mime;
            Width = width;
            Height = height;
            _url = url;
        }
    }
}
