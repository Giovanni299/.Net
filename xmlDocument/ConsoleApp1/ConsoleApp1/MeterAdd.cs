using System;
using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// 
/// </summary>
namespace ConsoleApp1
{
    /// <summary>
    /// Class meter map.
    /// </summary>
    [Serializable()]
    [XmlRoot("meterMap")]
    public class MeterAdd
    {
        /// <summary>
        /// Gets or sets the map.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        [XmlArray("variableType")]
        [XmlArrayItem("map", typeof(map))]
        public List<map> map { get; set; }
    }

    /// <summary>
    /// Class map variables.
    /// </summary>
    [Serializable()]
    public class map
    {
        /// <summary>
        /// Gets or sets the program identifier.
        /// </summary>
        /// <value>
        /// The program identifier.
        /// </value>
        [XmlElement("programId")]
        public string programId { get; set; }

        /// <summary>
        /// Gets or sets the variable.
        /// </summary>
        /// <value>
        /// The variable.
        /// </value>
        [XmlElement("variable")]
        public string variable { get; set; }
    }
}
