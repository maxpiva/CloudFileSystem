using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NutzCode.CloudFileSystem.Plugins.GoogleDrive
{
    public class File
    {
        [JsonProperty("alternateLink")]
        public virtual string AlternateLink { get; set; }

        [JsonProperty("appDataContents")]
        public virtual bool? AppDataContents { get; set; }

        [JsonProperty("canComment")]
        public virtual bool? CanComment { get; set; }

        [JsonProperty("copyable")]
        public virtual bool? Copyable { get; set; }

        [JsonProperty("createdDate")]
        public virtual DateTime? CreatedDate { get; set; }

        [JsonProperty("defaultOpenWithLink")]
        public virtual string DefaultOpenWithLink { get; set; }

        [JsonProperty("description")]
        public virtual string Description { get; set; }

        [JsonProperty("downloadUrl")]
        public virtual string DownloadUrl { get; set; }

        [JsonProperty("editable")]
        public virtual bool? Editable { get; set; }

        [JsonProperty("embedLink")]
        public virtual string EmbedLink { get; set; }

        [JsonProperty("etag")]
        public virtual string ETag { get; set; }

        [JsonProperty("explicitlyTrashed")]
        public virtual bool? ExplicitlyTrashed { get; set; }

        [JsonProperty("exportLinks")]
        public virtual IDictionary<string, string> ExportLinks { get; set; }

        [JsonProperty("fileExtension")]
        public virtual string FileExtension { get; set; }

        [JsonProperty("fileSize")]
        public virtual long? FileSize { get; set; }

        [JsonProperty("folderColorRgb")]
        public virtual string FolderColorRgb { get; set; }

        [JsonProperty("fullFileExtension")]
        public virtual string FullFileExtension { get; set; }

        [JsonProperty("headRevisionId")]
        public virtual string HeadRevisionId { get; set; }

        [JsonProperty("iconLink")]
        public virtual string IconLink { get; set; }

        [JsonProperty("id")]
        public virtual string Id { get; set; }

        [JsonProperty("imageMediaMetadata")]
        public virtual ImageMediaMetadataData ImageMediaMetadata { get; set; }

        [JsonProperty("indexableText")]
        public virtual IndexableTextData IndexableText { get; set; }

        [JsonProperty("kind")]
        public virtual string Kind { get; set; }

        [JsonProperty("labels")]
        public virtual LabelsData Labels { get; set; }

        [JsonProperty("lastModifyingUser")]
        public virtual User LastModifyingUser { get; set; }

        [JsonProperty("lastModifyingUserName")]
        public virtual string LastModifyingUserName { get; set; }



        [JsonProperty("lastViewedByMeDate")]
        public virtual DateTime? LastViewedByMeDate { get; set; }



        [JsonProperty("markedViewedByMeDate")]
        public virtual DateTime? MarkedViewedByMe { get; set; }

        [JsonProperty("md5Checksum")]
        public virtual string Md5Checksum { get; set; }

        [JsonProperty("mimeType")]
        public virtual string MimeType { get; set; }



        [JsonProperty("modifiedByMeDate")]
        public virtual DateTime? ModifiedByMeDate { get; set; }

        [JsonProperty("modifiedDate")]
        public virtual DateTime? ModifiedDate { get; set; }

        [JsonProperty("openWithLinks")]
        public virtual IDictionary<string, string> OpenWithLinks { get; set; }

        [JsonProperty("originalFilename")]
        public virtual string OriginalFilename { get; set; }

        [JsonProperty("ownedByMe")]
        public virtual bool? OwnedByMe { get; set; }

        [JsonProperty("ownerNames")]
        public virtual IList<string> OwnerNames { get; set; }

        [JsonProperty("owners")]
        public virtual IList<User> Owners { get; set; }

        [JsonProperty("parents")]
        public virtual IList<ParentReference> Parents { get; set; }

        [JsonProperty("permissions")]
        public virtual IList<Permission> Permissions { get; set; }

        [JsonProperty("properties")]
        public virtual IList<Property> Properties { get; set; }

        [JsonProperty("quotaBytesUsed")]
        public virtual long? QuotaBytesUsed { get; set; }

        [JsonProperty("selfLink")]
        public virtual string SelfLink { get; set; }

        [JsonProperty("shareable")]
        public virtual bool? Shareable { get; set; }

        [JsonProperty("shared")]
        public virtual bool? Shared { get; set; }

        [JsonProperty("sharedWithMeDate")]
        public virtual string SharedWithMeDateRaw { get; set; }

        [JsonProperty("sharingUser")]
        public virtual User SharingUser { get; set; }

        [JsonProperty("spaces")]
        public virtual IList<string> Spaces { get; set; }

        [JsonProperty("thumbnail")]
        public virtual ThumbnailData Thumbnail { get; set; }

        [JsonProperty("thumbnailLink")]
        public virtual string ThumbnailLink { get; set; }

        [JsonProperty("title")]
        public virtual string Title { get; set; }

        [JsonProperty("userPermission")]
        public virtual Permission UserPermission { get; set; }

        [JsonProperty("version")]
        public virtual long? Version { get; set; }

        [JsonProperty("videoMediaMetadata")]
        public virtual VideoMediaMetadataData VideoMediaMetadata { get; set; }

        [JsonProperty("webContentLink")]
        public virtual string WebContentLink { get; set; }

        [JsonProperty("webViewLink")]
        public virtual string WebViewLink { get; set; }

        [JsonProperty("writersCanShare")]
        public virtual bool? WritersCanShare { get; set; }

        public class ImageMediaMetadataData
        {
            [JsonProperty("aperture")]
            public virtual float? Aperture { get; set; }

            [JsonProperty("cameraMake")]
            public virtual string CameraMake { get; set; }

            [JsonProperty("cameraModel")]
            public virtual string CameraModel { get; set; }

            [JsonProperty("colorSpace")]
            public virtual string ColorSpace { get; set; }

            [JsonProperty("date")]
            public virtual string Date { get; set; }

            [JsonProperty("exposureBias")]
            public virtual float? ExposureBias { get; set; }

            [JsonProperty("exposureMode")]
            public virtual string ExposureMode { get; set; }

            [JsonProperty("exposureTime")]
            public virtual float? ExposureTime { get; set; }

            [JsonProperty("flashUsed")]
            public virtual bool? FlashUsed { get; set; }

            [JsonProperty("focalLength")]
            public virtual float? FocalLength { get; set; }

            [JsonProperty("height")]
            public virtual int? Height { get; set; }

            [JsonProperty("isoSpeed")]
            public virtual int? IsoSpeed { get; set; }

            [JsonProperty("lens")]
            public virtual string Lens { get; set; }

            [JsonProperty("location")]
            public virtual LocationData Location { get; set; }

            [JsonProperty("maxApertureValue")]
            public virtual float? MaxApertureValue { get; set; }

            [JsonProperty("meteringMode")]
            public virtual string MeteringMode { get; set; }

            [JsonProperty("rotation")]
            public virtual int? Rotation { get; set; }

            [JsonProperty("sensor")]
            public virtual string Sensor { get; set; }

            [JsonProperty("subjectDistance")]
            public virtual int? SubjectDistance { get; set; }

            [JsonProperty("whiteBalance")]
            public virtual string WhiteBalance { get; set; }

            [JsonProperty("width")]
            public virtual int? Width { get; set; }

            public class LocationData
            {
                [JsonProperty("altitude")]
                public virtual double? Altitude { get; set; }

                [JsonProperty("latitude")]
                public virtual double? Latitude { get; set; }

                [JsonProperty("longitude")]
                public virtual double? Longitude { get; set; }
            }
        }

        public class IndexableTextData
        {
            [JsonProperty("text")]
            public virtual string Text { get; set; }
        }

        public class LabelsData
        {
            [JsonProperty("hidden")]
            public virtual bool? Hidden { get; set; }

            [JsonProperty("restricted")]
            public virtual bool? Restricted { get; set; }

            [JsonProperty("starred")]
            public virtual bool? Starred { get; set; }

            [JsonProperty("trashed")]
            public virtual bool? Trashed { get; set; }

            [JsonProperty("viewed")]
            public virtual bool? Viewed { get; set; }
        }

        public class ThumbnailData
        {
            [JsonProperty("image")]
            public virtual string Image { get; set; }

            [JsonProperty("mimeType")]
            public virtual string MimeType { get; set; }
        }

        public class VideoMediaMetadataData
        {
            [JsonProperty("durationMillis")]
            public virtual long? DurationMillis { get; set; }

            [JsonProperty("height")]
            public virtual int? Height { get; set; }

            [JsonProperty("width")]
            public virtual int? Width { get; set; }
        }

        public class Permission
        {
            [JsonProperty("additionalRoles")]
            public virtual IList<string> AdditionalRoles { get; set; }

            [JsonProperty("authKey")]
            public virtual string AuthKey { get; set; }

            [JsonProperty("domain")]
            public virtual string Domain { get; set; }

            [JsonProperty("emailAddress")]
            public virtual string EmailAddress { get; set; }

            [JsonProperty("etag")]
            public virtual string ETag { get; set; }

            [JsonProperty("id")]
            public virtual string Id { get; set; }

            [JsonProperty("kind")]
            public virtual string Kind { get; set; }

            [JsonProperty("name")]
            public virtual string Name { get; set; }

            [JsonProperty("photoLink")]
            public virtual string PhotoLink { get; set; }

            [JsonProperty("role")]
            public virtual string Role { get; set; }

            [JsonProperty("selfLink")]
            public virtual string SelfLink { get; set; }

            [JsonProperty("type")]
            public virtual string Type { get; set; }

            [JsonProperty("value")]
            public virtual string Value { get; set; }

            [JsonProperty("withLink")]
            public virtual bool? WithLink { get; set; }
        }

        public class User
        {
            [JsonProperty("displayName")]
            public virtual string DisplayName { get; set; }

            [JsonProperty("emailAddress")]
            public virtual string EmailAddress { get; set; }

            public virtual string ETag { get; set; }

            [JsonProperty("isAuthenticatedUser")]
            public virtual bool? IsAuthenticatedUser { get; set; }

            [JsonProperty("kind")]
            public virtual string Kind { get; set; }

            [JsonProperty("permissionId")]
            public virtual string PermissionId { get; set; }

            [JsonProperty("picture")]
            public virtual PictureData Picture { get; set; }

            public class PictureData
            {
                [JsonProperty("url")]
                public virtual string Url { get; set; }
            }
        }

        public class Property
        {
            [JsonProperty("etag")]
            public virtual string ETag { get; set; }

            [JsonProperty("key")]
            public virtual string Key { get; set; }

            [JsonProperty("kind")]
            public virtual string Kind { get; set; }

            [JsonProperty("selfLink")]
            public virtual string SelfLink { get; set; }

            [JsonProperty("value")]
            public virtual string Value { get; set; }

            [JsonProperty("visibility")]
            public virtual string Visibility { get; set; }
        }

        public class ParentReference
        {
            public virtual string ETag { get; set; }

            [JsonProperty("id")]
            public virtual string Id { get; set; }

            [JsonProperty("isRoot")]
            public virtual bool? IsRoot { get; set; }

            [JsonProperty("kind")]
            public virtual string Kind { get; set; }

            [JsonProperty("parentLink")]
            public virtual string ParentLink { get; set; }

            [JsonProperty("selfLink")]
            public virtual string SelfLink { get; set; }
        }
    }
}
