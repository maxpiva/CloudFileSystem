using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NutzCode.CloudFileSystem.Plugins.OneDrive.Models;
using Stream = System.IO.Stream;


namespace NutzCode.CloudFileSystem.Plugins.OneDrive
{
    public abstract class OneDriveObject : BaseObject, IObject
    {

        public const string Item = "{0}/drive/items/{1}";
        public const string Copy = "{0}/drive/items/{1}/action.copy";
        // ReSharper disable once InconsistentNaming
        internal OneDriveFileSystem FS;


        public OneDriveObject(string parentname, OneDriveFileSystem fs, Mappings maps) : base(parentname, maps)
        {
            FS = fs;
        }
        internal async Task<FileSystemResult<dynamic>> ListAsync(string url, CancellationToken token)
        {
            int count;
            List<dynamic> accum = new List<dynamic>();
            do
            {
                FileSystemResult<ExpandoObject> cl = await FS.OAuth.CreateMetadataStreamAsync<ExpandoObject>(url,token).ConfigureAwait(false);
                if (cl.Status!=Status.Ok)
                    return new FileSystemResult<dynamic>(cl.Status, cl.Error);
                dynamic obj = cl.Result;
                count = obj.children.Count;
                if (count > 0)
                {
                    accum.AddRange(obj.children);
                    if (!((IDictionary<string, object>) obj).ContainsKey("@odata.nextLink"))
                        count = 0;
                    else
                        url = (string)((IDictionary<string, object>) obj)["@odata.nextLink"];
                }

            } while (count > 0);
            return new FileSystemResult<dynamic>(accum);
        }

        public ObjectAttributes Attributes
        {
            get
            {
                ObjectAttributes obj = 0;
                object de;
                if (this is OneDriveDirectory)
                    obj |= ObjectAttributes.Directory;
                if (TryGetMetadataValue("deleted", out de))
                {
                    if (de!=null)
                        obj |= ObjectAttributes.Trashed;
                }
                if (TryGetMetadataValue("shared", out de))
                {
                    if (de!=null)
                        obj |= ObjectAttributes.Shared;
                }
                return obj;
            }
        }


        public async Task<FileSystemResult> MoveAsync(IDirectory destination, CancellationToken token = default(CancellationToken))
        {
            if (Parent == null)
                return new FileSystemResult(Status.ArgumentError, "Unable to move root directory");
            if (!(destination is OneDriveDirectory))
                return new FileSystemResult(Status.ArgumentError, "Destination should be a Google Drive Directory");
            OneDriveDirectory dest = (OneDriveDirectory)destination;
            if (dest.Id == ((OneDriveDirectory)Parent).Id)
                return new FileSystemResult(Status.ArgumentError, "Source Directory and Destination Directory should be different");
            string url = Item.FormatRest(Id);
            MoveRequest req=new MoveRequest();
            req.ParentReference=new ItemReference();
            req.ParentReference.Id = dest.Id;

            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)), "application/json", new HttpMethod("PATCH")).ConfigureAwait(false);
            if (ex.Status==Status.Ok)
            {
                string oldFullname = FullName;
                SetData(ex.Result);
                if (this is OneDriveFile)
                {
                    ((OneDriveDirectory)Parent)._files.Remove((OneDriveFile)this);
                    dest._files.Add((OneDriveFile)this);
                }
                else if (this is OneDriveDirectory)
                {
                    FS.Refs.Remove(oldFullname);
                    ((OneDriveDirectory)Parent)._directories.Remove((OneDriveDirectory)this);
                    dest._directories.Add((OneDriveDirectory)this);
                    FS.Refs[FullName] = (OneDriveDirectory)this;

                }
                Parent = ((OneDriveDirectory)destination);
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }

        public async Task<FileSystemResult> CopyAsync(IDirectory destination, CancellationToken token = default(CancellationToken))
        {
            if (Parent == null)
                return new FileSystemResult(Status.ArgumentError, "Unable to move root directory");
            if (!(destination is OneDriveDirectory))
                return new FileSystemResult(Status.ArgumentError, "Destination should be a Google Drive Directory");
            OneDriveDirectory dest = (OneDriveDirectory)destination;
            if (dest.Id == ((OneDriveDirectory)Parent).Id)
                return new FileSystemResult(Status.ArgumentError, "Source Directory and Destination Directory should be different");
            string url = Copy.FormatRest(Id);
            MoveRequest req = new MoveRequest();
            req.ParentReference = new ItemReference();
            req.ParentReference.Id = dest.Id;

            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)), "application/json", HttpMethod.Post).ConfigureAwait(false);

            //TODO Add job monitor
            if (ex.Status == Status.Ok)
            {
                string oldFullname = FullName;
                if (this is OneDriveFile)
                {
                    ((OneDriveDirectory)Parent)._files.Remove((OneDriveFile)this);
                    dest._files.Add((OneDriveFile)this);
                }
                else if (this is OneDriveDirectory)
                {
                    FS.Refs.Remove(oldFullname);
                    ((OneDriveDirectory)Parent)._directories.Remove((OneDriveDirectory)this);
                    dest._directories.Add((OneDriveDirectory)this);
                    FS.Refs[FullName] = (OneDriveDirectory)this;

                }
                Parent = ((OneDriveDirectory)destination);
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }

        public async Task<FileSystemResult> RenameAsync(string newname, CancellationToken token = default(CancellationToken))
        {
            string oldFullname = FullName;
            IDictionary<string, object> dic = MetadataExpanded;
            dic["name"] = newname;
            FileSystemResult r=await WriteMetadataAsync(MetadataExpanded, token).ConfigureAwait(false);
            if (r.Status == Status.Ok)
            {
                if (this is OneDriveDirectory)
                {
                    FS.Refs.Remove(oldFullname);
                    FS.Refs[FullName] = (OneDriveDirectory)this;
                }

            }
            return r;
        }


        public IFileSystem FileSystem => FS;
        public void SetData(string data)
        {
            SetData(data, JsonConvert.DeserializeObject<ExpandoObject>(data), "application/json");
        }
        internal override void SetData(string data, ExpandoObject dataexp, string datamime)
        {
            base.SetData(data, dataexp, datamime);
            SetAssets();
        }

        public Task<FileSystemResult> TouchAsync(CancellationToken token = default(CancellationToken))
        {
            IDictionary<string, object> dic = MetadataExpanded;
            if (dic.ContainsKey("dateTimeLastModified"))
                dic["dateTimeLastModified"] = JsonConvert.SerializeObject(DateTime.UtcNow);
            else
                dic.Add("dateTimeLastModified", JsonConvert.SerializeObject(DateTime.UtcNow));
            return WriteMetadataAsync(MetadataExpanded,token);
        }

        public async Task<FileSystemResult> DeleteAsync(bool skipTrash, CancellationToken token = default(CancellationToken))
        {
            FileSystemResult<string> ex;
            string url = Item.FormatRest(Id);
            ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, null, null, HttpMethod.Delete).ConfigureAwait(false);
            if (ex.Status == Status.Ok)
            {
                if (this is OneDriveFile)
                {
                    Parent.Files.Remove((IFile)this);
                }
                else if (this is OneDriveDirectory)
                {
                    Parent.Directories.Remove((IDirectory)this);
                }
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }

        public Task<IFile> CreateAssetAsync(string name, Stream readstream, IProgress<FileProgress> progress, Dictionary<string, object> properties, CancellationToken token = default(CancellationToken))
        {
            throw new NotSupportedException();
        }
        private OneDriveThumbAsset FromThumbnail(string name, object o)
        {
            IDictionary<string, object> dic = (IDictionary <string, object>)o;
            string url=string.Empty;
            int width =0 , height=0;
            if (dic.ContainsKey("url"))
                url = (string)dic["url"];
            if (dic.ContainsKey("width"))
                width = int.Parse((string)dic["width"]);
            if (dic.ContainsKey("height"))
                height = int.Parse((string) dic["height"]);
            string mime = "image/jpeg";
            if (url.EndsWith("gif",StringComparison.InvariantCultureIgnoreCase))
                mime = "image/gif";
            if (url.EndsWith("png", StringComparison.InvariantCultureIgnoreCase))
                mime = "image/png";
            return new OneDriveThumbAsset(FullName, FS, name, url, width, height, mime);
        }       
        private void SetAssets()
        {
            Assets.Clear();
            ExpandoObject ex;
            if (TryGetMetadataValue("thumbnails", out ex))
            {
                if (ex != null)
                {
                    foreach (object o in ex)
                    {
                        string[] str1 = {"source", "small", "medium", "large"};
                        string[] str2 = {":thumbnail{0}", ":thumbnail{0}small", ":thumbnail{0}medium", ":thumbnail{0}large"};
                        // ReSharper disable once ExpressionIsAlwaysNull
                        IList<object> lms = o as IList<object>;
                        int cnt = 0;
                        // ReSharper disable once PossibleNullReferenceException
                        foreach (object thumb in lms)
                        {
                            // ReSharper disable once PossibleInvalidCastException
                            IDictionary<string, object> dic = (IDictionary<string, object>) o;
                            for (int x = 0; x < str1.Length; x++)
                            {
                                if (dic.ContainsKey(str1[x]))
                                {
                                    string rname = string.Format(str2[x], cnt == 0 ? string.Empty : cnt.ToString());
                                    Assets.Add(FromThumbnail(rname, thumb));
                                }
                            }
                        }

                    }
                }
            }
        }
        public async Task<FileSystemResult> WriteMetadataAsync(ExpandoObject metadata, CancellationToken token = default(CancellationToken))
        {
            string url = Item.FormatRest(Id);
            FileSystemResult<string> ex = await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata)), "application/json", new HttpMethod("PATCH")).ConfigureAwait(false);
            if (ex.Status == Status.Ok)
            {
                SetData(ex.Result);
                return new FileSystemResult();
            }
            return new FileSystemResult<IDirectory>(ex.Status, ex.Error);
        }
        public Task<FileSystemResult<List<Property>>> ReadPropertiesAsync(CancellationToken token = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public Task<FileSystemResult> SavePropertyAsync(Property property, CancellationToken token = default(CancellationToken))
        {
            throw new NotSupportedException();
        }
    }
}
