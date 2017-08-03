using System;
using System.Collections.Generic;
using System.Dynamic;
using Stream = System.IO.Stream;
using MemoryStream = System.IO.MemoryStream;
using Path = Pri.LongPath.Path;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NutzCode.CloudFileSystem.Plugins.GoogleDrive
{
    public abstract class GoogleDriveObject : BaseObject, IObject
    {
        // ReSharper disable once InconsistentNaming
        internal GoogleDriveFileSystem FS;

        public string GooglePatch = "https://www.googleapis.com/drive/v2/files/{0}";
        public string GoogleTouch = "https://www.googleapis.com/drive/v2/files/{0}/touch";
        public string GoogleTrash = "https://www.googleapis.com/drive/v2/files/{0}/trash";
        public string GoogleProperties = "https://www.googleapis.com/drive/v2/files/{0}/properties";
        public string GoogleProperty = "https://www.googleapis.com/drive/v2/files/{0}/properties/{1}";

        public GoogleDriveObject(string parentname, GoogleDriveFileSystem fs, Mappings maps) : base(parentname, maps)
        {
            FS = fs;
        }
        internal async Task<FileSystemResult<dynamic>> List(string url)
        {
            string baseurl = url;
            int count;
            List<dynamic> accum = new List<dynamic>();
            do
            {
                FileSystemResult<ExpandoObject> cl = await FS.OAuth.CreateMetadataStream<ExpandoObject>(url);
                if (!cl.IsOk)
                    return new FileSystemResult<dynamic>(cl.Error);
                dynamic obj = cl.Result;
                count = obj.items.Count;
                if (count > 0)
                {
                    accum.AddRange(obj.items);
                    if (!((IDictionary<string, object>)obj).ContainsKey("nextPageToken"))
                        count = 0;
                    else
                        url = baseurl + "&pageToken=" + obj.nextPageToken;
                }

            } while (count > 0);
            return new FileSystemResult<dynamic>(accum);
        }
        public ObjectAttributes Attributes
        {
            get
            {
                ObjectAttributes obj = 0;
                if (this is GoogleDriveDirectory)
                    obj |= ObjectAttributes.Directory;
                bool dta;
                if (TryGetMetadataValue("labels.starred", out dta))
                {
                    if (dta)
                        obj|=ObjectAttributes.Starred;
                }
                if (TryGetMetadataValue("labels.hidden", out dta))
                {
                    if (dta)
                        obj |= ObjectAttributes.Hidden;
                }
                if (TryGetMetadataValue("labels.trashed", out dta))
                {
                    if (dta)
                        obj |= ObjectAttributes.Trashed;
                }
                if (TryGetMetadataValue("labels.restricted", out dta))
                {
                    if (dta)
                        obj |= ObjectAttributes.Restricted;
                }
                if (TryGetMetadataValue("labels.viewed", out dta))
                {
                    if (dta)
                        obj |= ObjectAttributes.Viewed;
                }
                if (TryGetMetadataValue("editable", out dta))
                {
                    if (dta)
                        obj |= ObjectAttributes.Editable;
                }
                if (TryGetMetadataValue("shareable", out dta))
                {
                    if (dta)
                        obj |= ObjectAttributes.Shareable;
                }
                if (TryGetMetadataValue("shared", out dta))
                {
                    if (dta)
                        obj |= ObjectAttributes.Shared;
                }
                return obj;
            }
        }


        public IFileSystem FileSystem => FS;

        public async Task<FileSystemResult> MoveAsync(IDirectory destination)
        {
            if (Parent == null)
                return new FileSystemResult("Unable to move root directory");
            if (!(destination is GoogleDriveDirectory))
                return new FileSystemResult("Destination should be a Google Drive Directory");
            GoogleDriveDirectory dest = (GoogleDriveDirectory)destination;
            if (dest.Id == ((GoogleDriveDirectory)Parent).Id)
                return new FileSystemResult("Source Directory and Destination Directory should be different");
            string addParents = dest.Id;
            string removeParents = ((GoogleDriveDirectory)Parent).Id;

            string url = GooglePatch.FormatRest(Id);
            url += "?addParents=" + addParents + "&removeParents=" + removeParents;
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStream<string>(url, null, null,new HttpMethod("PATCH"));
            if (ex.IsOk)
            {
                string oldFullname = this.FullName;
                this.SetData(ex.Result);
                ChangeObjectDirectory<GoogleDriveDirectory,GoogleDriveFile>(oldFullname,FS.Refs,this,(GoogleDriveDirectory)Parent,dest);
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Error);
        }

        public async Task<FileSystemResult> CopyAsync(IDirectory destination)
        {
            if (Parent == null)
                return new FileSystemResult("Unable to copy root directory");
            if (!(destination is GoogleDriveDirectory))
                return new FileSystemResult("Destination should be a Google Drive Directory");
            GoogleDriveDirectory dest = (GoogleDriveDirectory)destination;
            if (dest.Id == ((GoogleDriveDirectory)Parent).Id)
                return new FileSystemResult("Source Directory and Destination Directory should be different");
            string addParents = dest.Id;

            string url = GooglePatch.FormatRest(Id);
            url += "?addParents=" + addParents;
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStream<string>(url, null, null, new HttpMethod("PATCH"));
            if (ex.IsOk)
            {
                if (this is GoogleDriveFile)
                {
                    GoogleDriveFile f = new GoogleDriveFile(destination.FullName, this.FS);
                    f.SetData(this.Metadata, this.MetadataExpanded, this.MetadataMime);
                    f.Parent = destination;
                    dest.Files.Add(f);
                }
                else if (this is GoogleDriveDirectory)
                {
                    GoogleDriveDirectory d = new GoogleDriveDirectory(destination.FullName, this.FS);
                    d.SetData(this.Metadata, this.MetadataExpanded, this.MetadataMime);
                    d.Parent = destination;
                    dest.Directories.Add(d);
                    FS.Refs[d.FullName] = d;
                }
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Error);
        }

        public async Task<FileSystemResult> RenameAsync(string newname)
        {
            string oldFullname = this.FullName;
            string url = GooglePatch.FormatRest(Id);
            File f=new File();
            f.Title = newname;
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStream<string>(url, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(f)), "application/json", new HttpMethod("PATCH"));
            if (ex.IsOk)
            {
                SetData(ex.Result);
                if (this is GoogleDriveDirectory)
                {
                    FS.Refs.Remove(oldFullname);
                    FS.Refs[this.FullName] = (GoogleDriveDirectory)this;
                }
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Error);
        }

        public void SetData(string data)
        {
            SetData(data, JsonConvert.DeserializeObject<ExpandoObject>(data), "application/json");
        }
        internal override void SetData(string data, ExpandoObject dataexp, string datamime)
        {
            base.SetData(data,dataexp,datamime);
            SetAssets();
        }
        public async Task<FileSystemResult> TouchAsync()
        {
            string url = GoogleTouch.FormatRest(Id);
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStream<string>(url, null, null, HttpMethod.Post);
            if (ex.IsOk)
            {
                SetData(ex.Result);
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Error);
        }

        public async Task<FileSystemResult> DeleteAsync(bool skipTrash)
        {
            FileSystemResult<string> ex;
            if (skipTrash)
            {
                string url = GooglePatch.FormatRest(Id);
                ex = await FS.OAuth.CreateMetadataStream<string>(url, null, null, HttpMethod.Delete);
            }
            else
            {
                string url = GoogleTrash.FormatRest(Id);
                ex = await FS.OAuth.CreateMetadataStream<string>(url, null, null, HttpMethod.Post);
            }
            if (ex.IsOk)
            {
                if (this is GoogleDriveFile)
                {
                    Parent.Files.Remove((IFile) this);
                }
                else if (this is GoogleDriveDirectory)
                {
                    Parent.Directories.Remove((IDirectory) this);
                }
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Error);
        }

        public async Task<FileSystemResult<IFile>> CreateAssetAsync(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            string ext = Path.GetExtension(name);
            if ((ext != "png") && (ext != "jpg") && (ext != "jpeg") && (ext != "gif"))
                return new FileSystemResult<IFile>("Google Drive only supports 'thumbnail' asset, acceptable formats are, jpg, png and gif");
            string mime;
            switch (ext)
            {
                case "png":
                    mime = "image/png";
                    break;
                case "jpeg":
                case "jpg":
                    mime = "image/jpeg";
                    break;
                default:
                    mime = "image/gif";
                    break;
            }
            MemoryStream ms = new MemoryStream();
            await readstream.CopyToAsync(ms, 16384, token);
            MemoryFile file = new MemoryFile(FullName + ":thumbnail", "thumbnail", mime, ms.ToArray());
            File f = new File { Thumbnail = new File.ThumbnailData {MimeType = mime, Image = Convert.ToBase64String(ms.ToArray())} };
            string url = GooglePatch.FormatRest(Id);
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStream<string>(url, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(f)), "application/json", new HttpMethod("PATCH"));
            if (!ex.IsOk)
                return new FileSystemResult<IFile>(ex.Error);
            Assets.Clear();
            Assets.Add(file);
            return new FileSystemResult<IFile>(file);
        }
        private void SetAssets()
        {
            Assets.Clear();
            ExpandoObject ex;
            if (TryGetMetadataValue("thumbnail", out ex))
            {
                IDictionary<string, object> dic = ex;
                if (dic.ContainsKey("image"))
                {
                    byte[] image = Convert.FromBase64String((string) dic["image"]);
                    string mime = string.Empty;
                    if (dic.ContainsKey("mimeType"))
                        mime = (string) dic["mimeType"];
                    MemoryFile f=new MemoryFile(FullName+":thumbnail","thumbnail",mime,image);
                    Assets.Add(f);
                }
            }
        }
        public async Task<FileSystemResult> WriteMetadataAsync(ExpandoObject metadata)
        {
            string url = GooglePatch.FormatRest(Id);
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStream<string>(url, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata)), "application/json", new HttpMethod("PATCH"));
            if (ex.IsOk)
            {
                SetData(ex.Result);
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Error);
        }

        public async Task<FileSystemResult<List<Property>>> ReadPropertiesAsync()
        {
            List<Property> props=new List<Property>();
            string url = GoogleProperties.FormatRest(Id);
            FileSystemResult<dynamic> fr = await List(url);
            if (!fr.IsOk)
                return new FileSystemResult<List<Property>>(fr.Error);
            foreach (dynamic v in fr.Result)
            {
                props.Add(new Property {IsPublic = v.visibility == "PUBLIC", Key = v.key, Value = v.value});
            }
            return new FileSystemResult<List<Property>>(props);
        }

        public async Task<FileSystemResult> SavePropertyAsync(Property property)
        {
            FileSystemResult<List<Property>> fex=await ReadPropertiesAsync();
            if (!fex.IsOk)
                return new FileSystemResult(fex.Error);
            Property p = fex.Result.FirstOrDefault(a => a.Key == property.Key);
            File.Property prop = new File.Property
            {
                Key = property.Key,
                Kind = "drive#property",
                Value = property.Value,
                Visibility = property.IsPublic ? "PUBLIC" : "PRIVATE"
            };

            if (p != null)
            {
                string url = GoogleProperty.FormatRest(Id, property.Key);
                FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStream<string>(url, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(prop)), "application/json", new HttpMethod("PATCH"));
                if (!ex.IsOk)
                    return new FileSystemResult<IFile>(ex.Error);
            }
            else
            {
                string url = GoogleProperties.FormatRest(Id);
                FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStream<string>(url, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(prop)), "application/json",HttpMethod.Post);
                if (!ex.IsOk)
                    return new FileSystemResult<IFile>(ex.Error);
            }
            return new FileSystemResult();
        }
    }
}
