using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using NutzCode.Libraries.Web;

namespace NutzCode.CloudFileSystem
{
    public abstract class BaseObject
    {

        private string _parentpath;

        public virtual ExpandoObject MetadataExpanded { get; private set; }
        public virtual string Metadata { get; private set; }
        public virtual string MetadataMime { get; private set; }


        public IDirectory Parent { get; internal set; } = null;

        internal virtual Mappings Mappings { get; }

        internal virtual List<IFile> Assets { get; } = new List<IFile>();

        public virtual List<IFile> GetAssets()
        {
            return Assets;
        }
        internal virtual string GetKey()
        {
            return Id;
        }
        public abstract long Size { get; }

        internal abstract SeekableWebParameters GetSeekableWebParameters(long position);


        internal virtual void SetData(string strdata, ExpandoObject deserializeddata, string strmime)
        {
            MetadataExpanded = deserializeddata;
            Metadata = strdata;
            MetadataMime = strmime;

        }
        internal virtual void SetData(BaseObject f)
        {
            Metadata = f.Metadata;
            MetadataMime = f.MetadataMime;
            MetadataExpanded = f.MetadataExpanded;

        }

        public virtual async Task<FileSystemResult<Stream>> OpenRead()
        {
            return await Task.FromResult(new FileSystemResult<Stream>(new SeekableWebStream(GetKey(), Size, CloudFileSystemPluginFactory.Instance.WebDataProvider, GetSeekableWebParameters)));
        }

        public BaseObject(string parentpath, Mappings m)
        {
            Mappings = m;
            _parentpath = parentpath;
        }
        public virtual bool PropertyExists(string prop)
        {
            List<string> props = Mappings.Get(prop) ?? new List<string> { prop };
            foreach (string s in props)
            {
                bool res = InternalPropertyExists(MetadataExpanded, s);
                if (res)
                    return true;
            }
            return false;
        }
        internal virtual bool InternalPropertyExists(ExpandoObject eobj, string prop)
        {
            if (MetadataExpanded != null)
            {
                IDictionary<string, object> exp = eobj;
                string[] childs = prop.Split('.');
                for (int x = 0; x < childs.Length; x++)
                {
                    if (exp.ContainsKey(childs[x]))
                    {
                        if (x == childs.Length - 1)
                            return true;
                        exp = (IDictionary <string, object>)exp[childs[x]];
                    }
                    else
                        return false;
                }
            }
            return false;
        }

        public virtual bool TryGetMetadataValue<T>(string name, out T value)
        {
            List<string> props = Mappings.Get(name) ?? new List<string> { name };
            foreach (string s in props)
            {
                bool res = InternalTryGetProperty(MetadataExpanded, s, out value);
                if (res)
                    return true;
            }
            value = default(T);
            return false;
        }
        internal virtual bool InternalTryGetProperty<T>(ExpandoObject eobj, string prop, out T value)
        {
            value = default(T);
            if (eobj != null)
            {
                IDictionary<string, object> exp = eobj;
                string[] childs = prop.Split('.');
                for (int x = 0; x < childs.Length; x++)
                {
                    if (exp.ContainsKey(childs[x]))
                    {
                        if (x == childs.Length - 1)
                        {
                            try
                            {
                                object obj = exp[childs[x]];
                                if (obj is T)
                                {
                                    value = (T) obj;
                                }
                                else
                                {
                                    value = (T)Convert.ChangeType(obj, typeof(T));
                                }
                                return true;
                            }
                            catch (Exception) // Serialization Type Problem
                            {
                                return false;
                            }
                        }
                        exp = (IDictionary<string, object>)exp[childs[x]];
                    }
                    else
                        return false;
                }
            }
            return false;
        }

        public virtual string Name
        {
            get
            {
                string ret;
                TryGetMetadataValue("name", out ret);
                return ret;
            }
        }

        public virtual string FullName
        {
            get
            {
                string name = Name;
                if (name != null)
                {
                    return _parentpath + "/" + name;
                }
                return _parentpath + "/[NULL]";
            }
        }
        public virtual string Id
        {
            get
            {
                string ret;
                TryGetMetadataValue("id", out ret);
                return ret;
            }
        }

        public virtual DateTime? ModifiedDate
        {
            get
            {
                string obj;
                if (TryGetMetadataValue("modifiedDate", out obj))
                {
                    DateTime dt;
                    if (DateTime.TryParse(obj, out dt))
                        return dt;
                }

                return null;
            }
        }
        public virtual DateTime? CreatedDate
        {
            get
            {
                string obj;
                if (TryGetMetadataValue("createdDate", out obj))
                {
                    DateTime dt;
                    if (DateTime.TryParse(obj, out dt))
                        return dt;
                }
                return null;
            }
        }
        public virtual DateTime? LastViewed
        {
            get
            {
                string obj;
                if (TryGetMetadataValue("lastViewed", out obj))
                {
                    DateTime dt;
                    if (DateTime.TryParse(obj, out dt))
                        return dt;
                }
                return null;
            }
        }

    }
}
