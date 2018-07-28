using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NutzCode.CloudFileSystem.Plugins.OneDrive.Models;
using NutzCode.Libraries.Web;
using Stream=System.IO.Stream;

namespace NutzCode.CloudFileSystem.Plugins.OneDrive
{
    public class OneDriveDirectory : OneDriveObject, IDirectory
    {

        public const string ListChildrens= "{0}/drive/items/{1}?expand=children";
        public const string CreateDir = "{0}/drive/items/{1}/children";



        internal List<OneDriveDirectory> _directories = new List<OneDriveDirectory>();
        internal List<OneDriveFile> _files = new List<OneDriveFile>();

        public List<IDirectory> Directories => _directories.Cast<IDirectory>().ToList();
        public List<IFile> Files => _files.Cast<IFile>().ToList();


        public bool IsPopulated { get; private set; }
        public bool IsRoot { get; internal set; } = false;


        public bool IsEmpty => !(Directories.Any() || Files.Any());




        public Task<IFile> CreateFileAsync(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            throw new NotImplementedException();
        }

        public async Task<IDirectory> CreateDirectoryAsync(string name, Dictionary<string, object> properties)
        {

            //if (properties == null)
            //    properties = new Dictionary<string, object>();
            CreateDirectoryRequest req = new CreateDirectoryRequest();
            req.Name = name;
            req.Folder = new Folder();
            string requesturl = CreateDir.FormatRest(this is OneDriveRoot ? "root" : Id);
            FileSystemResult<ExpandoObject> ex = await FS.OAuth.CreateMetadataStream<ExpandoObject>(requesturl, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)), "application/json");
            if (ex.Status==Status.Ok)
            {
                string id;
                if (InternalTryGetProperty(ex.Result, "id", out id))
                {
                    //FileSystemResult<ExpandoObject> ex2 =
                        await FS.OAuth.CreateMetadataStream<ExpandoObject>(Item.FormatRest(FS.OneDriveUrl, id));
                    OneDriveDirectory dir = new OneDriveDirectory(FullName, FS) {Parent = this};
                    dir.SetData(JsonConvert.SerializeObject(ex.Result));
                    FS.Refs[dir.FullName] = dir;
                    _directories.Add(dir);
                    return dir;
                }
                return new OneDriveDirectory(null, FS) {  Status = Status.SystemError, Error="Unable to get id from the created directory"};
            }
            return new OneDriveDirectory(null, FS) { Status = ex.Status, Error = ex.Error };
        }

        public async Task<FileSystemResult> PopulateAsync()
        {
            FileSystemResult r = await FS.OAuth.MayRefreshToken();
            if (r.Status != Status.Ok)
                return r;
            string url = ListChildrens.FormatRest(this is OneDriveRoot ? "root" : Id);
            FileSystemResult<dynamic> fr = await List(url);
            if (fr.Status!=Status.Ok)
                return new FileSystemResult(fr.Status, fr.Error);
            _files = new List<OneDriveFile>();
            List<IDirectory> dirlist = new List<IDirectory>();
            foreach (dynamic v in fr.Result)
            {
                if (((IDictionary<string, object>)v).ContainsKey("folder"))
                {
                    OneDriveDirectory dir = new OneDriveDirectory(FullName, FS) { Parent = this };
                    dir.SetData(JsonConvert.SerializeObject(v));
                    if ((dir.Attributes & ObjectAttributes.Trashed) != ObjectAttributes.Trashed)
                        dirlist.Add(dir);

                }
                else
                {
                    OneDriveFile file = new OneDriveFile(FullName, FS) { Parent = this };
                    file.SetData(JsonConvert.SerializeObject(v));
                    _files.Add(file);

                }
            }
            FS.Refs.AddDirectories(dirlist, this);
            _directories = dirlist.Cast<OneDriveDirectory>().ToList();
            IsPopulated = true;
            return new FileSystemResult();
        }

        public virtual Task<FileSystemSizes> QuotaAsync()
        {
            return FS.QuotaAsync();
        }


        public OneDriveDirectory(string parentpath, OneDriveFileSystem fs) : base(parentpath, fs, OneDriveMappings.Maps)
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
