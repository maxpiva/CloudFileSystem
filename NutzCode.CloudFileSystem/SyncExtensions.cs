using System;
using System.Collections.Generic;
using System.Dynamic;
using Stream = System.IO.Stream;
using System.Threading;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;

namespace NutzCode.CloudFileSystem
{
    public static class SyncExtensions
    {
        public static IFileSystem Init(this ICloudPlugin plugin, string filesystemname, LocalUserSettings settings, string userauthorization)
        {
            return Task.Run(async () => await plugin.InitAsync(filesystemname, settings, userauthorization)).Result;
        }
        public static IFileSystem Init(this ICloudPlugin plugin, string filesystemname, ProxyUserSettings settings, string userauthorization)
        {
            return Task.Run(async () => await plugin.InitAsync(filesystemname, settings, userauthorization)).Result;
        }
        public static IFileSystem Init(this ICloudPlugin plugin, string filesystemname, LocalUserSettingWithCode settings)
        {
            return Task.Run(async () => await plugin.InitAsync(filesystemname, settings)).Result;
        }
        public static FileSystemSizes Quota(this IDirectory filesys)
        {
            return Task.Run(async () => await filesys.QuotaAsync()).Result;
        }

        public static IObject Resolve(this IFileSystem filesys, string path)
        {
            return Task.Run(async () => await filesys.ResolveAsync(path)).Result;
        }

        public static FileSystemResult Move(this IObject obj, IDirectory destination)
        {
            return Task.Run(async () => await obj.MoveAsync(destination)).Result;
        }
        public static FileSystemResult Copy(this IObject obj, IDirectory destination)
        {
            return Task.Run(async () => await obj.CopyAsync(destination)).Result;
        }
        public static FileSystemResult Rename(this IObject obj, string newname)
        {
            return Task.Run(async () => await obj.RenameAsync(newname)).Result;
        }
        public static FileSystemResult Touch(this IObject obj)
        {
            return Task.Run(async () => await obj.TouchAsync()).Result;
        }
        public static FileSystemResult Delete(this IObject obj, bool skipTrash)
        {
            return Task.Run(async () => await obj.DeleteAsync(skipTrash)).Result;
        }
        public static IFile CreateAsset(this IObject obj, string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            return Task.Run(async () => await obj.CreateAssetAsync(name,readstream,token,progress,properties), token).Result;
        }
        public static FileSystemResult WriteMetadata(this IObject obj, ExpandoObject metadata)
        {
            return Task.Run(async () => await obj.WriteMetadataAsync(metadata)).Result;
        }

        public static FileSystemResult<List<Property>> ReadProperties(this IObject obj)
        {
            return Task.Run(async () => await obj.ReadPropertiesAsync()).Result;
        }
        public static FileSystemResult SaveProperty(this IObject obj, Property property)
        {
            return Task.Run(async () => await obj.SavePropertyAsync(property)).Result;
        }

        public static IFile CreateFile(this IDirectory directory, string name, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            return Task.Run(async () => await directory.CreateFileAsync(name,readstream,token, progress, properties), token).Result;
        }

        public static IDirectory CreateDirectory(this IDirectory directory, string name, Dictionary<string, object> properties)
        {
            return Task.Run(async () => await directory.CreateDirectoryAsync(name,properties)).Result;
        }

        public static FileSystemResult Populate(this IDirectory directory)
        {
            return Task.Run(async () => await directory.PopulateAsync()).Result;
        }



        public static FileSystemResult<Stream> OpenRead(this IFile file)
        {
            return Task.Run(async () => await file.OpenReadAsync()).Result;
        }

        public static FileSystemResult OverwriteFile(this IFile file, Stream readstream, CancellationToken token, IProgress<FileProgress> progress, Dictionary<string, object> properties)
        {
            return Task.Run(async () => await file.OverwriteFileAsync(readstream,token, progress, properties), token).Result;
        }


    }
}
