using System;
using System.Collections.Generic;
using System.Dynamic;
using Stream = System.IO.Stream;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NutzCode.Libraries.Web;

namespace NutzCode.CloudFileSystem.Plugins.GoogleDrive
{
    public class GoogleDriveDirectory : GoogleDriveObject, IDirectory, IOwnDirectory<GoogleDriveFile,GoogleDriveDirectory>
    {
        public const string GoogleList = "https://www.googleapis.com/drive/v2/files?q='{0}'+in+parents";
        public const string GoogleCreateDir = "https://www.googleapis.com/drive/v2/files";


        public List<GoogleDriveDirectory> IntDirectories { get; set; }=new List<GoogleDriveDirectory>();
        public List<GoogleDriveFile> IntFiles { get; set; }=new List<GoogleDriveFile>();


        public List<IDirectory> Directories => IntDirectories.Cast<IDirectory>().ToList();
        public List<IFile> Files => IntFiles.Cast<IFile>().ToList();


        public bool IsPopulated { get; private set; }
        public bool IsRoot { get; internal set; } = false;






        public Task<IFile> CreateFileAsync(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            throw new NotImplementedException();
        }

        public async Task<IDirectory> CreateDirectoryAsync(string name, Dictionary<string, object> properties)
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
            if (ex.Status==Status.Ok)
            {
                GoogleDriveDirectory dir = new GoogleDriveDirectory(FullName, FS) { Parent = this };
                dir.SetData(JsonConvert.SerializeObject(ex.Result));
                FS.Refs[dir.FullName]=dir;
                IntDirectories.Add(dir);
                return dir;
            }
            return new GoogleDriveDirectory(FullName, FS) { Parent = this, Status = ex.Status, Error=ex.Error };
        }

        public async Task<FileSystemResult> PopulateAsync()
        {
            FileSystemResult r = await FS.OAuth.MayRefreshToken();
            if (r.Status!=Status.Ok)
                return r;
            string url = GoogleList.FormatRest(Id);
            FileSystemResult<dynamic> fr = await List(url);
            if (fr.Status != Status.Ok)
                return new FileSystemResult(fr.Status, fr.Error);
            IntFiles = new List<GoogleDriveFile>();
            List<IDirectory> dirlist = new List<IDirectory>();
            foreach (dynamic v in fr.Result)
            {
                if (v.mimeType == "application/vnd.google-apps.folder")
                {
                    GoogleDriveDirectory dir = new GoogleDriveDirectory(FullName, FS) {Parent = this};
                    dir.SetData(JsonConvert.SerializeObject(v));
                    if ((dir.Attributes & ObjectAttributes.Trashed) != ObjectAttributes.Trashed)
                        dirlist.Add(dir);

                }
                else
                {
                    GoogleDriveFile file = new GoogleDriveFile(FullName, FS) {Parent = this};
                    file.SetData(JsonConvert.SerializeObject(v));
                    if ((file.Attributes & ObjectAttributes.Trashed) != ObjectAttributes.Trashed)
                        IntFiles.Add(file);

                }
            }
            FS.Refs.AddDirectories(dirlist,this);
            IntDirectories = dirlist.Cast<GoogleDriveDirectory>().ToList();
            IsPopulated = true;
            return new FileSystemResult();
        }

        public virtual Task<FileSystemSizes> QuotaAsync()
        {
            return FS.QuotaAsync();
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
