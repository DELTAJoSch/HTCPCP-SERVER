using HTCPCP_Server.Logging.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Logging
{
    /// <summary>
    /// Static singleton logger
    /// </summary>
    public class Log
    {
        public static bool IsVerbose { get; set; }

        /// <summary>
        /// The loggers the log uses
        /// </summary>
        private static List<ILogger> loggers = new List<ILogger>() { new ConsoleLog(false) };

        /// <summary>
        /// Setup the loggers
        /// </summary>
        /// <param name="loggerTypes"></param>
        internal static void Setup(List<ILogger> loggerTypes)
        {
            loggers = loggerTypes;
        }

        /// <summary>
        /// Logs an info message
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Info(string message)
        {
            loggers.ForEach(logger => logger.Info(message));
        }

        /// <summary>
        /// Logs an verbose info message
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Verbose(string message)
        {
            if(IsVerbose)
                loggers.ForEach(logger => logger.Info(message + "\nCMD> "));
        }

        /// <summary>
        /// Logs a warning
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Warn(string message) 
        {
            loggers.ForEach((logger) => logger.Warn(message));
        }

        /// <summary>
        /// Logs an error
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Error(string message)
        {
            loggers.ForEach((log) => log.Error(message));
        }

        /// <summary>
        /// Logs a fatal error
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Fatal(string message)
        {
            loggers.ForEach((log) => log.Fatal(message));
        }

        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Debug(string message)
        {
            loggers.ForEach((log) => log.Debug(message));
        }

        /// <summary>
        /// logs a fatal exception
        /// </summary>
        /// <param name="message">the message to log</param>
        /// <param name="exception">the exception to log</param>
        public static void Fatal(string message, Exception exception)
        {
            loggers.ForEach((log) => log.Fatal(message, exception));
        }
    }
}
