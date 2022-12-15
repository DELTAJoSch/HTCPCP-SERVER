using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Logging.Implementations
{
    /// <summary>
    /// Describes a logger
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// prints a info
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message);

        /// <summary>
        /// prints a warning
        /// </summary>
        /// <param name="message"></param>
        public void Warn(string message);

        /// <summary>
        /// prints an error message
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message);

        /// <summary>
        /// prints a fatal exception
        /// </summary>
        /// <param name="message"></param>
        public void Fatal(string message);

        /// <summary>
        /// Prints a fatal message with exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Fatal(string message, Exception exception);

        /// <summary>
        /// Prints a debug message
        /// </summary>
        /// <param name="message"> The message to print</param>
        public void Debug(string message);
    }
}
