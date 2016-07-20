using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NutzCode.Libraries.Web;

namespace NutzCode.CloudFileSystem.Plugins.GoogleDrive
{
    public class GoogleDriveDirectory : GoogleDriveObject, IDirectory
    {
        public const string GoogleList = "https://www.googleapis.com/drive/v2/files?q='{0}'+in+parents";
        public const string GoogleCreateDir = "https://www.googleapis.com/drive/v2/files";



        private List<GoogleDriveDirectory> _directories = new List<GoogleDriveDirectory>();
        private List<GoogleDriveFile> _files = new List<GoogleDriveFile>();

        public List<IDirectory> Directories => _directories.Cast<IDirectory>().ToList();
        public List<IFile> Files => _files.Cast<IFile>().ToList();


        public bool IsPopulated { get; private set; }
        public bool IsRoot { get; internal set; } = false;






        public Task<FileSystemResult<IFile>> CreateFileAsync(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            throw new NotImplementedException();
        }

        public async Task<FileSystemResult<IDirectory>> CreateDirectoryAsync(string name, Dictionary<string, object> properties)
        {

            if (properties == null)
                properties = new Dictionary<string, object>();
            File f=new File();
            f.MimeType = "application/vnd.google-apps.folder";
            f.Title = f.Description = name;
            if (Id != "ROOT")
            {
                f.Parents=new List<File.ParentReference>();
                f.Parents.Add(new File.ParentReference {Id = Id});
            }
            if (properties.Any(a => a.Key.Equals("ModifiedDate", StringComparison.InvariantCultureIgnoreCase)))
                f.ModifiedDate = (DateTime)properties.First(a => a.Key.Equals("ModifiedDate", StringComparison.InvariantCultureIgnoreCase)).Value;
            if (properties.Any(a => a.Key.Equals("CreatedDate", StringComparison.InvariantCultureIgnoreCase)))
                f.CreatedDate = (DateTime)properties.First(a => a.Key.Equals("CreatedDate", StringComparison.InvariantCultureIgnoreCase)).Value;
            FileSystemResult<ExpandoObject> ex = await FS.OAuth.CreateMetadataStream<ExpandoObject>(GoogleCreateDir, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(f)), "application/json");
            if (ex.IsOk)
            {
                string parentpath = string.Empty;
                if (!IsRoot)
                    parentpath = FullName;
                GoogleDriveDirectory dir = new GoogleDriveDirectory(parentpath, FS) { Parent = this };
                dir.SetData(JsonConvert.SerializeObject(ex.Result));
                FS.Refs.AddWeakReferenceDirectory(dir);
                _directories.Add(dir);
                return new FileSystemResult<IDirectory>(dir);
            }
            return new FileSystemResult<IDirectory>(ex.Error);
        }

        public async Task<FileSystemResult> PopulateAsync()
        {
            return await InternalPopulate(false);
        }
        public async Task<FileSystemResult> RefreshAsync()
        {
            return await InternalPopulate(true);
        }
        private async Task<FileSystemResult> InternalPopulate(bool force)
        {
            if (IsPopulated && !force)
                return new FileSystemResult();
            FileSystemResult r = await FS.OAuth.MayRefreshToken();
            if (!r.IsOk)
                return r;
            string url = GoogleList.FormatRest(Id);
            FileSystemResult<dynamic> fr = await List(url);
            if (!fr.IsOk)
                return new FileSystemResult(fr.Error);
            if (_directories != null)
            {
                foreach (IDirectory d in _directories)
                    FS.Refs.RemoveWeakReferenceDirectory(d);
            }
            _directories = new List<GoogleDriveDirectory>();
            _files = new List<GoogleDriveFile>();
            string parentpath = string.Empty;
            if (!IsRoot)
                parentpath = FullName;
            foreach (dynamic v in fr.Result)
            {
                if (v.mimeType == "application/vnd.google-apps.folder")
                {
                    GoogleDriveDirectory dir = new GoogleDriveDirectory(parentpath, FS) {Parent = this};
                    dir.SetData(JsonConvert.SerializeObject(v));
                    FS.Refs.AddWeakReferenceDirectory(dir);
                    _directories.Add(dir);

                }
                else
                {
                    GoogleDriveFile file = new GoogleDriveFile(parentpath, FS) {Parent = this};
                    file.SetData(JsonConvert.SerializeObject(v));
                    _files.Add(file);

                }
            }
                IsPopulated = true;
            return new FileSystemResult();
        }




        public GoogleDriveDirectory(string parentpath, GoogleDriveFileSystem fs) : base(parentpath, fs, GoogleDriveMappings.Maps)
        {
            IsPopulated = false;
        }

        public override long Size { get; } = 0;
        internal override SeekableWebParameters GetSeekableWebParameters(long position)
        {
            throw new NotImplementedException();
        }
    }
}
