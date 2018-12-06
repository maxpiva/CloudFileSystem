using System;
using System.Collections.Generic;
using System.Dynamic;
using Stream = System.IO.Stream;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nito.AsyncEx;
using NutzCode.Libraries.Web;

namespace NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive
{
    public class AmazonDirectory : AmazonObject, IDirectory, IOwnDirectory<AmazonFile,AmazonDirectory>
    {
        internal const string AmazonRoot = "{0}/nodes?filters=isRoot:true AND status:AVAILABLE&asset=ALL";
        internal const string AmazonList = "{0}/nodes?filters=status:AVAILABLE AND parents:{1}&asset=ALL";

        internal const string AmazonCreate = "{0}/nodes";
        internal const string AmazonCreateDirectory = "{0}/nodes?localId={1}";


        public List<AmazonDirectory> IntDirectories { get; set; }=new List<AmazonDirectory>();
        public List<AmazonFile> IntFiles { get; set; }=new List<AmazonFile>();

        public List<IDirectory> Directories => IntDirectories.Cast<IDirectory>().ToList();
        public List<IFile> Files => IntFiles.Cast<IFile>().ToList();

        public bool IsEmpty => !(Directories.Any() || Files.Any());


        public bool IsPopulated { get; private set; }
        public bool IsRoot { get; internal set; } = false;


        internal async Task<FileSystemResult<dynamic>> ListAsync(string url, CancellationToken token)
        {
            string baseurl = url;
            int count ;
            List<dynamic> accum = new List<dynamic>();
            do
            {
                FileSystemResult<ExpandoObject> cl = await FS.OAuth.CreateMetadataStreamAsync<ExpandoObject>(url,token).ConfigureAwait(false);
                if (cl.Status!=Status.Ok)
                    return new FileSystemResult<dynamic>(cl.Status, cl.Error);
                dynamic obj = cl.Result;
                count = obj.data.Count;
                if (count > 0)
                {
                    accum.AddRange(obj.data);
                    if (!((IDictionary<string, object>)obj).ContainsKey("nextToken"))
                        count = 0;
                    else
                        url = baseurl + "&startToken=" + obj.nextToken;
                }

            } while (count > 0);
            return new FileSystemResult<dynamic>(accum);
        }


       
        private readonly AsyncLock _populateLock=new AsyncLock();
        public async Task<FileSystemResult> PopulateAsync(CancellationToken token=default(CancellationToken))
        {
            using (await _populateLock.LockAsync(token).ConfigureAwait(false))
            {
                FileSystemResult r = await FS.CheckExpirationsAsync(token).ConfigureAwait(false);
                if (r.Status!=Status.Ok)
                    return r;
                string url = AmazonList.FormatRest(FS.OAuth.EndPoint.MetadataUrl, Id);
                FileSystemResult<dynamic> fr = await ListAsync(url, token).ConfigureAwait(false);
                if (r.Status != Status.Ok)
                    return r;
                IntFiles = new List<AmazonFile>();
                List<IDirectory> dirlist = new List<IDirectory>();
                foreach (dynamic v in fr.Result)
                {
                    if (v.kind == "FOLDER") 
                    {
                        AmazonDirectory dir = new AmazonDirectory(FullName, FS) { Parent = this };
                        dir.SetData(JsonConvert.SerializeObject(v));
                        if ((dir.Attributes & ObjectAttributes.Trashed) != ObjectAttributes.Trashed)
                            dirlist.Add(dir);

                    }
                    else if (v.kind == "FILE")
                    {
                        AmazonFile f = new AmazonFile(FullName, FS) { Parent = this};
                        f.SetData(JsonConvert.SerializeObject(v));
                        if ((f.Attributes & ObjectAttributes.Trashed) != ObjectAttributes.Trashed)
                            IntFiles.Add(f);

                    }
                }
                FS.Refs.AddDirectories(dirlist, this);
                IntDirectories = dirlist.Cast<AmazonDirectory>().ToList();
                IsPopulated = true;
                return new FileSystemResult();
            }           
        }

        public virtual Task<FileSystemSizes> QuotaAsync(CancellationToken token=default(CancellationToken))
        {
            return FS.QuotaAsync(token);
        }


        public AmazonDirectory(string parentpath, AmazonFileSystem fs) : base(parentpath, fs, AmazonMappings.Maps)
        {
            IsPopulated = false;
        }

        public async Task<IFile> CreateFileAsync(string name, Stream readstream, IProgress<FileProgress> progress, Dictionary<string, object> properties, CancellationToken token=default(CancellationToken))
        {
#if DEBUG || EXPERIMENTAL
            IFile f=await InternalCreateFileAsync(name,"FILE",false, this,readstream,token,progress, properties).ConfigureAwait(false);
            if (f.Status!=Status.Ok)
                IntFiles.Add((AmazonFile)f);
            return f;
#else
            throw new NotSupportedException();
#endif
        }

        public async Task<IDirectory> CreateDirectoryAsync(string name, Dictionary<string, object> properties, CancellationToken token=default(CancellationToken))
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
            FileSystemResult<string> ex=await FS.OAuth.CreateMetadataStreamAsync<string>(url, token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(j,Formatting.None,new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })), "application/json").ConfigureAwait(false);
            if (ex.Status==Status.Ok)
            {
                AmazonDirectory dir = new AmazonDirectory(FullName, FS) { Parent = this };
                dir.SetData(ex.Result);
                FS.Refs[dir.FullName] = dir;
                IntDirectories.Add(dir);
                return dir;
            }
            return new AmazonDirectory(FullName, FS) { Parent = this, Status=ex.Status, Error = ex.Error};
        }

        public override long Size { get; } = 0;
        internal override SeekableWebParameters GetSeekableWebParameters(long position)
        {
            throw new NotSupportedException();
        }

    }
}
