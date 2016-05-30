/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// The field types used by the <see cref="FieldSettings"/> object
    /// </summary>
    [DataContract]
    public enum FieldType
    {
        /// <summary>
        /// Textual content
        /// </summary>
        [EnumMember]
        Text = 0,
        /// <summary>
        /// Floating point number
        /// </summary>
        [EnumMember]
        DecimalNumber = 1,
        /// <summary>
        /// Whole integers with no decimal
        /// </summary>
        [EnumMember]
        Integer = 2,
        /// <summary>
        /// Monetary value
        /// </summary>
        [EnumMember]
        Currency = 3,
        /// <summary>
        /// File content
        /// </summary>
        [EnumMember]
        Attachment = 4,
        /// <summary>
        /// URL to an image
        /// </summary>
        [EnumMember]
        Image = 5,
        /// <summary>
        /// URL
        /// </summary>
        [EnumMember]        
        Hyperlink = 6,
        /// <summary>
        /// Generic object
        /// </summary>
        [EnumMember]
        Entity = 7,
        /// <summary>
        /// Value that references a lookup table
        /// </summary>
        [EnumMember]
        Lookup = 8,
        /// <summary>
        /// True or false
        /// </summary>
        [EnumMember]
        Boolean = 9,
        /// <summary>
        /// A date and/or time
        /// </summary>
        [EnumMember]
        DateTime = 10,
    }
}
