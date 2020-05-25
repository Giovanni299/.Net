//-----------------------------------------------------------------------------
// <copyright file="TemplateModelXML.cs" company="Primestone S.A.S.">
//     Copyright © Primestone S.A.S. All rights reserved.
// </copyright>
// <summary>
// Template Model XML.
// </summary>
//-----------------------------------------------------------------------------

namespace ConsoleApp1.TemplateReg
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Array Of Devices Download Configuration.
    /// </summary>
    [Serializable]
    public class ArrayOfDevicesDownloadConfiguration
    {
        /// <summary>
        /// Gets or sets the devices download configuration.
        /// </summary>
        /// <value>
        /// The devices download configuration.
        /// </value>
        [XmlElement("DevicesDownloadConfiguration")]
        public DevicesDownload[] DevicesDownloadConfiguration { get; set; }
    }

    /// <summary>
    /// Devices Download.
    /// </summary>
    [Serializable]
    public class DevicesDownload
    {
        /// <summary>
        /// Gets or sets the supported device types.
        /// </summary>
        /// <value>
        /// The supported device types.
        /// </value>
        [XmlArrayItem("DeviceTypeV10", IsNullable = false)]
        public string[] SupportedDeviceTypes { get; set; }

        /// <summary>
        /// Gets or sets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        [XmlArrayItem("VariableDownloadConfiguration", IsNullable = false)]
        public Variables[] Variables { get; set; }
    }

    /// <summary>
    /// Variables.
    /// </summary>
    [Serializable]
    public class Variables
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the protocol code.
        /// </summary>
        /// <value>
        /// The protocol code.
        /// </value>
        [XmlAttribute]
        public string ProtocolCode { get; set; }

        /// <summary>
        /// Gets or sets the length of the data type.
        /// </summary>
        /// <value>
        /// The length of the data type.
        /// </value>
        [XmlAttribute]
        public string DataTypeLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Variables"/> is download.
        /// </summary>
        /// <value>
        ///   <c>true</c> if download; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool Download { get; set; }
    }
}
