﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using Stream = System.IO.Stream;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;



namespace NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive
{
    public abstract class AmazonObject : BaseObject, IObject
    {

        internal const string AmazonMove = "{0}/nodes/{1}/children";
        internal const string AmazonCopy = "{0}/nodes/{1}/children/{2}";
        internal const string AmazonPatch = "{0}/nodes/{1}";
        internal const string AmazonTrash = "{0}/trash/{1}";
        internal const string AmazonNodeFile = "{0}/nodes/{1}/content";
        internal const string AmazonUpload = "{0}/nodes{1}";
        internal const string AmazonProperty = "{0}/nodes/{1}/properties/{2}/{3}";
        // ReSharper disable once InconsistentNaming
        internal AmazonFileSystem FS;

        public AmazonObject(string parentname, AmazonFileSystem fs, Mappings maps) : base(parentname, maps)
        {
            FS = fs;
        }

        // ReSharper disable once CyclomaticComplexity
        internal async Task<IFile> InternalCreateFileAsync(string name, string type, bool overwrite, AmazonObject parent, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            if (readstream.Length == 0)
                throw new ArgumentException("input stream must have length");
            string url = AmazonUpload.FormatRest(FS.OAuth.EndPoint.ContentUrl, "?suppress=deduplication");
            Json.Metadata j = null;
            if (!overwrite)
            {
                if (properties == null)
                    properties = new Dictionary<string, object>();
                j = new Json.Metadata();
                j.version = 0;
                j.contentProperties = new Json.ContentProperties();
                j.contentProperties.size = readstream.Length;
                j.contentProperties.version = 0;
                j.contentProperties.extension = Path.GetExtension(name);
                string n = Extensions.ContentFromExtension(j.contentProperties.extension);
                j.contentProperties.contentType = !string.IsNullOrEmpty(n) ? n : "application/octet-stream";
                if (properties.Any(a => a.Key.Equals("ModifiedDate", StringComparison.InvariantCultureIgnoreCase)))
                    j.modifiedDate =
                        (DateTime)
                            properties.First(
                                a => a.Key.Equals("ModifiedDate", StringComparison.InvariantCultureIgnoreCase)).Value;
                if (properties.Any(a => a.Key.Equals("CreatedDate", StringComparison.InvariantCultureIgnoreCase)))
                    j.createdDate =
                        (DateTime)
                            properties.First(
                                a => a.Key.Equals("CreatedDate", StringComparison.InvariantCultureIgnoreCase)).Value;
                if (properties.Any(a => a.Key.Equals("Application", StringComparison.InvariantCultureIgnoreCase)))
                    j.createdBy =
                        (string)
                            properties.First(
                                a => a.Key.Equals("Application", StringComparison.InvariantCultureIgnoreCase)).Value;
                else
                    j.createdBy = "CloudFileSystem";
                if (properties.Any(a => a.Key.Equals("MD5", StringComparison.InvariantCultureIgnoreCase)))
                    j.contentProperties.md5 =
                        (string)
                            properties.First(a => a.Key.Equals("MD5", StringComparison.InvariantCultureIgnoreCase))
                                .Value;
                j.description = j.name = name;
                j.isShared = false;
                j.kind = type;
                j.parents = new List<string>();
                j.parents.Add(parent.Id);
            }
            HttpRequestMessage msg = new HttpRequestMessage(overwrite ? HttpMethod.Put : HttpMethod.Post, url);
            string boundary = "--" + Guid.NewGuid().ToString();
            msg.Headers.UserAgent.ParseAdd(FS.OAuth.UserSettings.UserAgent);
            MultipartFormDataContent ct = new MultipartFormDataContent(boundary);
            if (!overwrite)
            {
                string meta = JsonConvert.SerializeObject(j);
                StringContent sc = new StringContent(meta, Encoding.UTF8);
                ct.Add(sc, "metadata");
            }
            StreamContent ssc = new StreamContent(readstream);
            ct.Add(ssc, "file", name);
            msg.Content = ct;
            HttpClientHandler handler = new HttpClientHandler();
            ProgressMessageHandler progresshandler = new ProgressMessageHandler(handler);
            progresshandler.HttpSendProgress += (a, b) =>
            {
                FileProgress u = new FileProgress();
                u.Percentage = b.ProgressPercentage;
                u.TotalSize = b.TotalBytes ?? 0;
                u.TransferSize = b.BytesTransferred;
                progress.Report(u);
            };
            handler.AllowAutoRedirect = true;
            handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            await FS.CheckExpirationsAsync(token).ConfigureAwait(false);
            HttpClient cl = new HttpClient(progresshandler);
            HttpResponseMessage response = null;
            try
            {
                response = await cl.SendAsync(msg, token).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    string dd = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    string parentpath = string.Empty;
                    AmazonDirectory dir=parent as AmazonDirectory;
                    if (dir==null || !dir.IsRoot)
                        parentpath = FullName;
                    AmazonFile file = new AmazonFile(parentpath, FS) {Parent = dir };
                    file.SetData(dd);
                    progress.Report(new FileProgress
                    {
                        Percentage = 100,
                        TotalSize = file.Size,
                        TransferSize = file.Size
                    });
                    return file;
                }
                return new AmazonFile("",FS) { Status = Status.HttpError, Error="Http Error : " + response.StatusCode};
            }
            catch (Exception e)
            {
                return new AmazonFile("", FS) { Status = Status.SystemError, Error = "Http Error : " + "Exception Error : " + e.Message };
            }
            finally
            {
                response?.Dispose();
                cl.Dispose();
                handler.Dispose();
                progresshandler.Dispose();
                msg.Dispose();
            }
        }

        public ObjectAttributes Attributes
        {
            get
            {
                ObjectAttributes obj = 0;
                if (this is AmazonDirectory)
                    obj |= ObjectAttributes.Directory;
                string status;
                if (TryGetMetadataValue("status", out status))
                {
                    if (status == "TRASH")
                        obj |= ObjectAttributes.Trashed;
                }
                if (TryGetMetadataValue("kind", out status))
                {
                    if (status=="ASSET")
                        obj|=ObjectAttributes.Asset;
                }
                bool dta;
                if (TryGetMetadataValue("restricted", out dta))
                {
                    if (dta)
                        obj |= ObjectAttributes.Restricted;
                }
                if (TryGetMetadataValue("isShared", out dta))
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
            if (Parent==null)
                return new FileSystemResult(Status.ArgumentError, "Unable to move root directory");
            if (!(destination is AmazonDirectory))
                return new FileSystemResult(Status.ArgumentError,"Destination should be an Amazon Cloud Drive Directory");
            AmazonDirectory dest = (AmazonDirectory) destination;
            if (dest.Id== ((AmazonDirectory)Parent).Id)
                return new FileSystemResult(Status.ArgumentError, "Source Directory and Destination Directory should be different");
            Json.MoveData j=new Json.MoveData();
            j.childId = Id;
            j.fromParent = ((AmazonDirectory)Parent).Id;

            string url = AmazonMove.FormatRest(FS.OAuth.EndPoint.MetadataUrl, dest.Id);
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(j)), "application/json").ConfigureAwait(false);
            //TODO Some kind of locking
            if (ex.Status==Status.Ok)
            {
                string oldFullname = FullName;
                SetData(ex.Result);
                ChangeObjectDirectory<AmazonDirectory,AmazonFile>(oldFullname,FS.Refs,this,(AmazonDirectory)Parent,dest);
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }

        public async Task<FileSystemResult> CopyAsync(IDirectory destination, CancellationToken token = default(CancellationToken))
        {
            if (Parent == null)
                return new FileSystemResult(Status.ArgumentError, "Unable to Copy root directory");
            if (!(destination is AmazonDirectory))
                return new FileSystemResult(Status.ArgumentError, "Destination should be an Amazon Cloud Drive Directory");
            AmazonDirectory dest = (AmazonDirectory)destination;
            if (dest.Id == ((AmazonDirectory)Parent).Id)
                return new FileSystemResult(Status.ArgumentError, "Source Directory and Destination Directory should be different");
            string url = AmazonCopy.FormatRest(FS.OAuth.EndPoint.MetadataUrl, dest.Id, Id);

            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, null, null,HttpMethod.Put).ConfigureAwait(false);
            if (ex.Status == Status.Ok)
            {
                if (this is AmazonFile)
                {
                    AmazonFile f=new AmazonFile(destination.FullName,FS);
                    f.SetData(Metadata,MetadataExpanded,MetadataMime);
                    f.Parent = destination;
                    dest.IntFiles.Add(f);
                }
                else if (this is AmazonDirectory)
                {

                    AmazonDirectory d=new AmazonDirectory(destination.FullName,FS);
                    d.SetData(Metadata, MetadataExpanded, MetadataMime);
                    d.Parent = destination;
                    dest.IntDirectories.Add(d);
                    FS.Refs[d.FullName] = d;
                }
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }

        public async Task<FileSystemResult> RenameAsync(string newname, CancellationToken token = default(CancellationToken))
        {
            string oldFullname = FullName;
            string url = AmazonPatch.FormatRest(FS.OAuth.EndPoint.ContentUrl, Id);
            Json.ChangeName name=new Json.ChangeName();
            name.name = newname;
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(name)), "application/json", new HttpMethod("PATCH")).ConfigureAwait(false);
            if (ex.Status == Status.Ok)
            {
                SetData(ex.Result);               
                if (this is AmazonDirectory)
                {
                    FS.Refs.Remove(oldFullname);
                    FS.Refs[FullName] = (AmazonDirectory)this;
                }
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }

        public Task<FileSystemResult> TouchAsync(CancellationToken token = default(CancellationToken))
        {
            //Touch is not supported yet by Amazon
            throw new NotSupportedException();
            /*
            string url = string.Format(AmazonPatch, FS.OAuth.EndPoint.ContentUrl, Id);
            Json.ChangeModifiedDate date=new Json.ChangeModifiedDate();
            date.modifiedDate=DateTime.UtcNow;
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStream<string>(url, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(date)), "application/json", new HttpMethod("PATCH"));
            if (ex.IsOk)
            {
                SetData(ex.Result);
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Error);*/
        }

        public async Task<FileSystemResult> DeleteAsync(bool skipTrash, CancellationToken token = default(CancellationToken))
        {
            if (Parent == null)
                return new FileSystemResult(Status.ArgumentError, "Unable to delete root directory");
            string url = AmazonTrash.FormatRest(FS.OAuth.EndPoint.MetadataUrl,Id);

            FileSystemResult<ExpandoObject> ex = await FS.OAuth.CreateMetadataStreamAsync<ExpandoObject>(url, token, null, null, HttpMethod.Put).ConfigureAwait(false);
            if (ex.Status == Status.Ok)
            {
                if (this is AmazonFile)
                {
                    Parent.Files.Remove((IFile)this);
                }
                else if (this is AmazonDirectory)
                {
                    Parent.Directories.Remove((IDirectory)this);
                }
                Parent = null;
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }

        public async Task<FileSystemResult> WriteMetadataAsync(ExpandoObject metadata, CancellationToken token = default(CancellationToken))
        {
            //Only Name, Description and Labels supported
            Json.MetaPatch patch=new Json.MetaPatch();
            string v;
            if (InternalTryGetProperty(metadata, "name", out v))
                patch.name = v;
            if (InternalTryGetProperty(metadata, "description", out v))
                patch.description = v;
            List<string> labels;
            if (InternalTryGetProperty(metadata, "labels", out labels))
                patch.labels = labels;
            string url = AmazonPatch.FormatRest(FS.OAuth.EndPoint.MetadataUrl, Id);
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(patch)), "application/json", new HttpMethod("PATCH")).ConfigureAwait(false);
            if (ex.Status == Status.Ok)
            {
                SetData(ex.Result);
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }

        public void SetData(string data)
        {
            SetData(data, JsonConvert.DeserializeObject<ExpandoObject>(data), "application/json");
        }
        internal override void SetData(string data, ExpandoObject dataexp, string mime)
        {
            base.SetData(data,dataexp,mime);
            SetAssets();
        }
        private List<Property> InternalReadProperties()
        {
            //Properties can only be read/write/change by the Application Friendly Name Owner
            ExpandoObject o;
            List<Property> props=new List<Property>();
            if (TryGetMetadataValue("properties", out o))
            {
                foreach (string s in ((IDictionary<string, object>)o).Keys)
                {
                    if (s.Equals(FS.AppFriendlyName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        IDictionary<string, object> l = (IDictionary<string, object>) ((IDictionary<string, object>) o)[s];
                        foreach (string n in l.Keys)
                        {
                            props.Add(new Property {IsPublic = false, Key = n, Owner = s, Value = (string)l[n]});
                        }
                    }
                }
            }
            return props;
        }


        public Task<FileSystemResult<List<Property>>> ReadPropertiesAsync(CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(new FileSystemResult<List<Property>>(InternalReadProperties()));
        }





        public async Task<FileSystemResult> SavePropertyAsync(Property property, CancellationToken token = default(CancellationToken))
        {
            //Properties can only be read/write/change by the Application Friendly Name Owner
            List<Property> org=InternalReadProperties();
            property.Owner = FS.AppFriendlyName;
            Property p = org.FirstOrDefault(a => a.Owner == property.Owner && a.Key == property.Key);
            List<string> errors = new List<string>();
            if (p != null)
            {
                string url2 = AmazonProperty.FormatRest(FS.OAuth.EndPoint.MetadataUrl, Id, FS.AppFriendlyName, property.Key);
                FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url2, token, null, null, HttpMethod.Delete).ConfigureAwait(false);
                if (ex.Status != Status.Ok)
                    errors.Add(ex.Error);
            }
            string url = AmazonProperty.FormatRest(FS.OAuth.EndPoint.MetadataUrl, Id, FS.AppFriendlyName, property.Key);
            Json.AddProperty addp = new Json.AddProperty();
            addp.value = property.Value;
            FileSystemResult<string> ex2 = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(addp)), "application/json", HttpMethod.Put).ConfigureAwait(false);
            if (ex2.Status != Status.Ok)
                errors.Add(ex2.Error);
            if (errors.Count > 0)
                return new FileSystemResult(Status.HttpError, "Save With Errors: " + string.Join("\r\n", errors));
            return new FileSystemResult();
        }
        private void SetAssets()
        {
            List<object> ex;
            Assets.Clear();
            if (TryGetMetadataValue("assets", out ex))
            {
                foreach (ExpandoObject ob in ex)
                {
                    AmazonFile f = new AmazonFile(string.Empty, FS) { Parent = null };
                    f.SetData(JsonConvert.SerializeObject(ob));
                    Assets.Add(f);
                }
            }
        }


        public async Task<IFile> CreateAssetAsync(string name, Stream readstream, IProgress<FileProgress> progress, Dictionary<string, object> properties, CancellationToken token = default(CancellationToken))
        {
#if DEBUG || EXPERIMENTAL
            IFile f = await InternalCreateFileAsync(Name, "ASSET", false, this, readstream, token, progress, properties).ConfigureAwait(false);
            if (f.Status==Status.Ok)
                Assets.Add(f);
            return f;
#else
           throw new NotSupportedException();
#endif
        }

    }
}
