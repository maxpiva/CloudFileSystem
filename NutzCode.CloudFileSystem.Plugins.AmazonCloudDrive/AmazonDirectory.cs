using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nito.AsyncEx;
using NutzCode.Libraries.Web;

namespace NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive
{
    public class AmazonDirectory : AmazonObject, IDirectory
    {
        internal const string AmazonRoot = "{0}/nodes?filters=isRoot:true AND status:AVAILABLE&asset=ALL";
        internal const string AmazonList = "{0}/nodes?filters=status:AVAILABLE AND parents:{1}&asset=ALL";

        internal const string AmazonCreate = "{0}/nodes";
        internal const string AmazonCreateDirectory = "{0}/nodes?localId={1}";

        internal List<AmazonDirectory> _directories=new List<AmazonDirectory>();
        internal List<AmazonFile> _files = new List<AmazonFile>();

        public List<IDirectory> Directories => _directories.Cast<IDirectory>().ToList();
        public List<IFile> Files => _files.Cast<IFile>().ToList();


        public bool IsPopulated { get; private set; }
        public bool IsRoot { get; internal set; } = false;


        internal async Task<FileSystemResult<dynamic>> List(string url)
        {
            string baseurl = url;
            int count ;
            List<dynamic> accum = new List<dynamic>();
            do
            {
                FileSystemResult<ExpandoObject> cl = await FS.OAuth.CreateMetadataStream<ExpandoObject>(url);
                if (!cl.IsOk)
                    return new FileSystemResult<dynamic>(cl.Error);
                dynamic obj = cl.Result;
                count = obj.data.Count;
                if (count > 0)
                {
                    accum.AddRange(obj.data);
                    if (!((IDictionary<String, object>)obj).ContainsKey("nextToken"))
                        count = 0;
                    else
                        url = baseurl + "&startToken=" + obj.nextToken;
                }

            } while (count > 0);
            return new FileSystemResult<dynamic>(accum);
        }


       
        private readonly AsyncLock _populateLock=new AsyncLock();
        public async Task<FileSystemResult> PopulateAsync()
        {
            using (await _populateLock.LockAsync())
            {
                FileSystemResult r = await FS.CheckExpirations();
                if (!r.IsOk)
                    return r;
                string url = AmazonList.FormatRest(FS.OAuth.EndPoint.MetadataUrl, Id);
                FileSystemResult<dynamic> fr = await List(url);
                if (!fr.IsOk)
                    return new FileSystemResult(fr.Error);
                _files = new List<AmazonFile>();
                List<IDirectory> dirlist = new List<IDirectory>();
                foreach (dynamic v in fr.Result)
                {
                    if (v.kind == "FOLDER") 
                    {
                        AmazonDirectory dir = new AmazonDirectory(FullName, FS) { Parent = this };
                        dir.SetData(JsonConvert.SerializeObject(v));
                        dirlist.Add(dir);

                    }
                    else if (v.kind == "FILE")
                    {
                        AmazonFile f = new AmazonFile(FullName, FS) { Parent = this};
                        f.SetData(JsonConvert.SerializeObject(v));
                        _files.Add(f);

                    }
                }
                FS.Refs.AddDirectories(dirlist, this);
                _directories = dirlist.Cast<AmazonDirectory>().ToList();
                IsPopulated = true;
                return new FileSystemResult();
            }           
        }


        public AmazonDirectory(string parentpath, AmazonFileSystem fs) : base(parentpath, fs, AmazonMappings.Maps)
        {
            IsPopulated = false;
        }

        public async Task<FileSystemResult<IFile>> CreateFileAsync(string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
#if DEBUG || EXPERIMENTAL
            FileSystemResult<IFile> f=await InternalCreateFile(name,"FILE",false, this,readstream,token,progress, properties);
            if (f.IsOk)
                _files.Add((AmazonFile)f.Result);
            return f;
#else
            throw new NotSupportedException();
#endif
        }

        public async Task<FileSystemResult<IDirectory>> CreateDirectoryAsync(string name, Dictionary<string, object> properties)
        {
            if (properties == null)
                properties = new Dictionary<string, object>();
            Json.Metadata j = new Json.Metadata();
            if (properties.Any(a => a.Key.Equals("ModifiedDate", StringComparison.InvariantCultureIgnoreCase)))
                j.modifiedDate = (DateTime)properties.First(a => a.Key.Equals("ModifiedDate", StringComparison.InvariantCultureIgnoreCase)).Value;
            if (properties.Any(a => a.Key.Equals("CreatedDate", StringComparison.InvariantCultureIgnoreCase)))
                j.createdDate = (DateTime)properties.First(a => a.Key.Equals("CreatedDate", StringComparison.InvariantCultureIgnoreCase)).Value;
            if (properties.Any(a => a.Key.Equals("Application", StringComparison.InvariantCultureIgnoreCase)))
                j.createdBy = (string)properties.First(a => a.Key.Equals("Application", StringComparison.InvariantCultureIgnoreCase)).Value;
            else
                j.createdBy = "CloudFileSystem";
            j.description = j.name = name;
            j.isShared = false;
            j.kind = "FOLDER";
            j.parents = new List<string> { Id };
            string url = AmazonCreateDirectory.FormatRest(FS.OAuth.EndPoint.MetadataUrl,Guid.NewGuid().ToString().Replace("-",string.Empty));
            FileSystemResult<string> ex=await FS.OAuth.CreateMetadataStream<string>(url,Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(j,Newtonsoft.Json.Formatting.None,new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })), "application/json");
            if (ex.IsOk)
            {
                AmazonDirectory dir = new AmazonDirectory(this.FullName, FS) { Parent = this };
                dir.SetData(ex.Result);
                FS.Refs[dir.FullName] = dir;
                _directories.Add(dir);
                return new FileSystemResult<IDirectory>(dir);
            }
            return new FileSystemResult<IDirectory>(ex.Error);
        }

        public override long Size { get; } = 0;
        internal override SeekableWebParameters GetSeekableWebParameters(long position)
        {
            throw new NotSupportedException();
        }
    }
}
