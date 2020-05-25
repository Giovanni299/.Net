using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    using System;

    /// <summary>
    /// Sockets Validation Entity.
    /// </summary>
    public class SocketsValidation
    {
        /// <summary>
        /// Gets or sets the identifier soc.
        /// </summary>
        /// <value>
        /// The identifier soc.
        /// </value>
        public int Id_Soc { get; set; }

        /// <summary>
        /// Gets or sets the id meter.
        /// </summary>
        /// <value>
        /// The id meter.
        /// </value>
        public string Noins { get; set; }

        /// <summary>
        /// Gets or sets the is backup.
        /// </summary>
        /// <value>
        /// The is backup.
        /// </value>
        public int Is_Backup { get; set; }

        /// <summary>
        /// Gets or sets the fails.
        /// </summary>
        /// <value>
        /// The fails.
        /// </value>
        public long Fails { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string FailMessage { get; set; }
    }
}
