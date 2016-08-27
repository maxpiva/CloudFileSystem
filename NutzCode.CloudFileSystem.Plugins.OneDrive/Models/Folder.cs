// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NutzCode.CloudFileSystem.Plugins.OneDrive.Models
{
    /// <summary>
    /// The type Folder.
    /// </summary>
    [DataContract]
    public class Folder
    {
    
        /// <summary>
        /// Gets or sets childCount.
        /// </summary>
        [DataMember(Name = "childCount", EmitDefaultValue = false, IsRequired = false)]
        public Int32? ChildCount { get; set; }
    
        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData(ReadData = true)]
        public IDictionary<string, object> AdditionalData { get; set; }
    
    }
}
