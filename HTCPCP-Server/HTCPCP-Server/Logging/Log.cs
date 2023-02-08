using HTCPCP_Server.Logging.Implementations;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        private static bool blocked = false;
        private static List<Action> actions = new List<Action>();

        /// <summary>
        /// The loggers the log uses
        /// </summary>
        private static List<ILogger> loggers = new List<ILogger>() { new ConsoleLog(false) };

        /// <summary>
        /// blocks the log. All messages will be queued and wait for unblock
        /// </summary>
        public static void Block()
        {
            blocked = true;
        }

        /// <summary>
        /// Unblocks the Logs and writes all stored messages out
        /// </summary>
        public static void Unblock()
        {
            blocked = false;
            foreach(var act in actions)
            {
                act.Invoke();
            }
            actions.Clear();
        }

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
            if (blocked)
            {
                actions.Add(() => Info(message));
            }
            else
            {
                loggers.ForEach(logger => logger.Info(message));
            }
        }

        /// <summary>
        /// Logs an verbose info message
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Verbose(string message)
        {
            if (IsVerbose)
            {
                if (blocked)
                {
                    actions.Add(() => {
                        loggers.ForEach(logger => logger.Info(message));
                        AnsiConsole.Write("CMD> ");
                    });
                }
                else
                {
                    loggers.ForEach(logger => logger.Info(message));
                    AnsiConsole.Write("CMD> ");
                }
            }
        }

        /// <summary>
        /// Logs a warning
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Warn(string message) 
        {
            if (blocked)
            {
                actions.Add(() => Warn(message));
            }
            else
            {
                loggers.ForEach((logger) => logger.Warn(message));
            }
        }

        /// <summary>
        /// Logs an error
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Error(string message)
        {
            if (blocked)
            {
                actions.Add(() => Error(message));
            }
            else
            {
                loggers.ForEach((logger) => logger.Warn(message));
            }
        }

        /// <summary>
        /// Logs a fatal error
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Fatal(string message)
        {
            if (blocked)
            {
                actions.Add(() => Fatal(message));
            }
            else
            {
                loggers.ForEach((log) => log.Fatal(message));
            }
        }

        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Debug(string message)
        {
            if (blocked)
            {
                actions.Add(() => Debug(message));
            }
            else
            {
                loggers.ForEach((log) => log.Debug(message));
            }
        }

        /// <summary>
        /// logs a fatal exception
        /// </summary>
        /// <param name="message">the message to log</param>
        /// <param name="exception">the exception to log</param>
        public static void Fatal(string message, Exception exception)
        {
            if (blocked)
            {
                actions.Add(() => Fatal(message, exception));
            }
            else
            {
                loggers.ForEach((log) => log.Fatal(message, exception));
            }
        }
    }
}
