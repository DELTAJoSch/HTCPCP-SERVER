using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Helpers
{
    /// <summary>
    /// Signals that a connection has been lost during execution
    /// </summary>
    public class ConnectionLostException : Exception
    {
        private string to;

        /// <summary>
        /// The connected resource that has been lost
        /// </summary>
        public string To { get { return to; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="message">The message</param>
        public ConnectionLostException(string? message) : base(message)
        {
            this.to = "Unknown Connection";
        }

        /// <summary>
        /// Sets the to resource
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="to">The resource that was lost</param>
        public ConnectionLostException(string? message, string to) : base(message)
        {
            this.to = to;
        }
    }
}
