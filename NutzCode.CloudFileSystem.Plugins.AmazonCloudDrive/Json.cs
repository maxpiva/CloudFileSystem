using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NutzCode.CloudFileSystem.Plugins.AmazonCloudDrive
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Json

    {

        public class Metadata
        {
            public string eTagResponse { get; set; }
            public string id { get; set; }
            public string kind { get; set; }
            public long version { get; set; }
            public List<string> labels { get; set; }
            public ContentProperties contentProperties { get; set; }
            public DateTime createdDate { get; set; }
            public string createdBy { get; set; }
            public string description { get; set; }
            public bool restricted { get; set; }
            public DateTime modifiedDate { get; set; }
            public string name { get; set; }
            public bool isShared { get; set; }
            public List<string> parents { get; set; }
            public string status { get; set; }
        }

        public class MetaPatch
        {
            public string name { get; set; }
            public List<string> labels { get; set; }
            public string description { get; set; }
        }
        public class Quota
        {
            public long quota { get; set; }
            public string lastCalculated { get; set; }
            public long available { get; set; }
        }
        public class Video
        {
            public int rotate { get; set; }
            public long bitrate { get; set; }
            public string audioCodec { get; set; }
            public double videoFrameRate { get; set; }
            public string encoder { get; set; }
            public long audioBitrate { get; set; }
            public int audioSampleRate { get; set; }
            public string audioChannelLayout { get; set; }
            public double duration { get; set; }
            public long videoBitrate { get; set; }
            public int audioChannels { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string videoCodec { get; set; }
            public DateTime createdDate { get; set; }
            public string location { get; set; }
            public string title { get; set; }
            public string make { get; set; }
            public string model { get; set; }

        }
        
        public class Image
        {
            public string apertureValue { get; set; }
            public DateTime dateTime { get; set; }
            public string exposureTime { get; set; }
            public string software { get; set; }
            public string whiteBalance { get; set; }
            public DateTime dateTimeDigitized { get; set; }
            public string colorSpace { get; set; }
            public string gpsTimeStamp { get; set; }
            public string resolutionUnit { get; set; }
            public string model { get; set; }
            public string make { get; set; }
            public string iso { get; set; }
            public int height { get; set; }
            public string sharpness { get; set; }
            public string meteringMode { get; set; }
            public string orientation { get; set; }
            public DateTime dateTimeOriginal { get; set; }
            public string exposureProgram { get; set; }
            public string sensingMethod { get; set; }
            public int width { get; set; }
            public string location { get; set; }
            public string exposureMode { get; set; }
            public string flash { get; set; }
            public string focalLength { get; set; }
            public int xresolution { get; set; }
            public int yresolution { get; set; }
        }
        public class ContentProperties
        {
            public DateTime contentDate { get; set; }
            public Image image { get; set; }
            public Video video { get; set; }
            public string extension { get; set; }
            public long size { get; set; }
            public string contentType { get; set; }
            public long version { get; set; }
            public string md5 { get; set; }
            public Dictionary<string,Dictionary<string,object>> properties=new Dictionary<string, Dictionary<string, object>>(); 
        }


        public class MoveData
        {
            public string fromParent { get; set; }
            public string childId { get; set; }
        }

        public class ChangeModifiedDate
        {
            public DateTime modifiedDate { get; set; }
        }

        public class ChangeName
        {
            public string name { get; set; }
        }

        public class AddProperty
        {
            public string value { get; set; }
        }
    }
}
