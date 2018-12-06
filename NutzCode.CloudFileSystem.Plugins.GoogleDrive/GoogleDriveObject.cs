﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using Stream = System.IO.Stream;
using MemoryStream = System.IO.MemoryStream;
using System.IO;
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
        internal async Task<FileSystemResult<dynamic>> ListAsync(string url, CancellationToken token)
        {
            string baseurl = url;
            int count;
            List<dynamic> accum = new List<dynamic>();
            do
            {
                FileSystemResult<ExpandoObject> cl = await FS.OAuth.CreateMetadataStreamAsync<ExpandoObject>(url, token).ConfigureAwait(false);
                if (cl.Status!=Status.Ok)
                    return new FileSystemResult<dynamic>(cl.Status, cl.Error);
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

        public async Task<FileSystemResult> MoveAsync(IDirectory destination, CancellationToken token=default(CancellationToken))
        {
            if (Parent == null)
                return new FileSystemResult(Status.ArgumentError, "Unable to move root directory");
            if (!(destination is GoogleDriveDirectory))
                return new FileSystemResult(Status.ArgumentError, "Destination should be a Google Drive Directory");
            GoogleDriveDirectory dest = (GoogleDriveDirectory)destination;
            if (dest.Id == ((GoogleDriveDirectory)Parent).Id)
                return new FileSystemResult(Status.ArgumentError, "Source Directory and Destination Directory should be different");
            string addParents = dest.Id;
            string removeParents = ((GoogleDriveDirectory)Parent).Id;

            string url = GooglePatch.FormatRest(Id);
            url += "?addParents=" + addParents + "&removeParents=" + removeParents;
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, null, null,new HttpMethod("PATCH")).ConfigureAwait(false);
            if (ex.Status==Status.Ok)
            {
                string oldFullname = FullName;
                SetData(ex.Result);
                ChangeObjectDirectory<GoogleDriveDirectory,GoogleDriveFile>(oldFullname,FS.Refs,this,(GoogleDriveDirectory)Parent,dest);
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }

        public async Task<FileSystemResult> CopyAsync(IDirectory destination, CancellationToken token=default(CancellationToken))
        {
            if (Parent == null)
                return new FileSystemResult(Status.ArgumentError, "Unable to copy root directory");
            if (!(destination is GoogleDriveDirectory))
                return new FileSystemResult(Status.ArgumentError, "Destination should be a Google Drive Directory");
            GoogleDriveDirectory dest = (GoogleDriveDirectory)destination;
            if (dest.Id == ((GoogleDriveDirectory)Parent).Id)
                return new FileSystemResult(Status.ArgumentError, "Source Directory and Destination Directory should be different");
            string addParents = dest.Id;

            string url = GooglePatch.FormatRest(Id);
            url += "?addParents=" + addParents;
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, null, null, new HttpMethod("PATCH")).ConfigureAwait(false);
            if (ex.Status == Status.Ok)
            {
                if (this is GoogleDriveFile)
                {
                    GoogleDriveFile f = new GoogleDriveFile(destination.FullName, FS);
                    f.SetData(Metadata, MetadataExpanded, MetadataMime);
                    f.Parent = destination;
                    dest.Files.Add(f);
                }
                else if (this is GoogleDriveDirectory)
                {
                    GoogleDriveDirectory d = new GoogleDriveDirectory(destination.FullName, FS);
                    d.SetData(Metadata, MetadataExpanded, MetadataMime);
                    d.Parent = destination;
                    dest.Directories.Add(d);
                    FS.Refs[d.FullName] = d;
                }
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }

        public async Task<FileSystemResult> RenameAsync(string newname, CancellationToken token=default(CancellationToken))
        {
            string oldFullname = FullName;
            string url = GooglePatch.FormatRest(Id);
            File f=new File();
            f.Title = newname;
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(f)), "application/json", new HttpMethod("PATCH")).ConfigureAwait(false);
            if (ex.Status == Status.Ok)
            {
                SetData(ex.Result);
                if (this is GoogleDriveDirectory)
                {
                    FS.Refs.Remove(oldFullname);
                    FS.Refs[FullName] = (GoogleDriveDirectory)this;
                }
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
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
        public async Task<FileSystemResult> TouchAsync(CancellationToken token = default(CancellationToken))
        {
            string url = GoogleTouch.FormatRest(Id);
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, null, null, HttpMethod.Post).ConfigureAwait(false);
            if (ex.Status == Status.Ok)
            {
                SetData(ex.Result);
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }

        public async Task<FileSystemResult> DeleteAsync(bool skipTrash, CancellationToken token = default(CancellationToken))
        {
            FileSystemResult<string> ex;
            if (skipTrash)
            {
                string url = GooglePatch.FormatRest(Id);
                ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, null, null, HttpMethod.Delete).ConfigureAwait(false);
            }
            else
            {
                string url = GoogleTrash.FormatRest(Id);
                ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, null, null, HttpMethod.Post).ConfigureAwait(false);
            }
            if (ex.Status == Status.Ok)
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
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }

        public async Task<IFile> CreateAssetAsync(string name, Stream readstream, IProgress<FileProgress> progress, Dictionary<string, object> properties, CancellationToken token = default(CancellationToken))
        {
            string ext = Path.GetExtension(name);
            if ((ext != "png") && (ext != "jpg") && (ext != "jpeg") && (ext != "gif"))
                return new GoogleDriveFile("",FS) { Status=Status.ArgumentError, Error="Google Drive only supports 'thumbnail' asset, acceptable formats are, jpg, png and gif" };
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
            await readstream.CopyToAsync(ms, 16384, token).ConfigureAwait(false);
            MemoryFile file = new MemoryFile(FullName + ":thumbnail", "thumbnail", mime, ms.ToArray());
            File f = new File { Thumbnail = new File.ThumbnailData {MimeType = mime, Image = Convert.ToBase64String(ms.ToArray())} };
            string url = GooglePatch.FormatRest(Id);
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(f)), "application/json", new HttpMethod("PATCH")).ConfigureAwait(false);
            if (ex.Status != Status.Ok)
                return new GoogleDriveFile("", FS) {Status = ex.Status, Error = ex.Error};
            
            Assets.Clear();
            Assets.Add(file);
            return file;
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
        public async Task<FileSystemResult> WriteMetadataAsync(ExpandoObject metadata, CancellationToken token = default(CancellationToken))
        {
            string url = GooglePatch.FormatRest(Id);
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata)), "application/json", new HttpMethod("PATCH")).ConfigureAwait(false);
            if (ex.Status == Status.Ok)
            {
                SetData(ex.Result);
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }

        public async Task<FileSystemResult<List<Property>>> ReadPropertiesAsync(CancellationToken token = default(CancellationToken))
        {
            List<Property> props=new List<Property>();
            string url = GoogleProperties.FormatRest(Id);
            FileSystemResult<dynamic> fr = await ListAsync(url, token).ConfigureAwait(false);
            if (fr.Status != Status.Ok)
                return new FileSystemResult<List<Property>>(fr.Status, fr.Error);
            foreach (dynamic v in fr.Result)
            {
                props.Add(new Property {IsPublic = v.visibility == "PUBLIC", Key = v.key, Value = v.value});
            }
            return new FileSystemResult<List<Property>>(props);
        }

        public async Task<FileSystemResult> SavePropertyAsync(Property property, CancellationToken token = default(CancellationToken))
        {
            FileSystemResult<List<Property>> fex=await ReadPropertiesAsync(token).ConfigureAwait(false);
            if (fex.Status != Status.Ok)
                return new FileSystemResult(fex.Status, fex.Error);
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
                FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(prop)), "application/json", new HttpMethod("PATCH")).ConfigureAwait(false);
                if (ex.Status!=Status.Ok)
                    return new FileSystemResult<IFile>(ex.Status, ex.Error);
            }
            else
            {
                string url = GoogleProperties.FormatRest(Id);
                FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(prop)), "application/json",HttpMethod.Post).ConfigureAwait(false);
                if (ex.Status!=Status.Ok)
                    return new FileSystemResult<IFile>(ex.Status, ex.Error);
            }
            return new FileSystemResult();
        }
    }
}
