using HTCPCP_Server.Logging.Implementations;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine.Help;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Logging
{
    /// <summary>
    /// Implements a Logger that logs to the console
    /// </summary>
    public class ConsoleLog:ILogger
    {
        private readonly bool verbose;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="verbose">If true, use verbose logging</param>
        internal ConsoleLog(bool verbose)
        {
            this.verbose = verbose;
        }

        /// <summary>
        /// Log an info message
        /// </summary>
        /// <param name="message">The info message</param>
        public void Info(string message)
        {
            AnsiConsole.MarkupLineInterpolated($"[underline turquoise2]LOG - INFO:[/] {message}");
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Warn(string message)
        {
            AnsiConsole.MarkupLineInterpolated($"[underline orange1]LOG - WARN:[/] {message}");
        }

        /// <summary>
        /// Log an error
        /// </summary>
        /// <param name="message">the error message</param>
        public void Error(string message)
        {
            AnsiConsole.MarkupLineInterpolated($"[underline bold red1]LOG - ERROR:[/] {message}");
        }

        /// <summary>
        /// Log a fatal error
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Fatal(string message)
        {
            AnsiConsole.Write(new FigletText("FATAL EXCEPTION").LeftAligned().Color(Color.Red));
            AnsiConsole.MarkupLineInterpolated($"[underline bold red1]LOG - FATAL:[/] {message}");
        }

        /// <summary>
        /// Logs a fatal error and an exception
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="exception">The exception to log</param>
        public void Fatal(string message, Exception exception)
        {
            AnsiConsole.Write(new FigletText("FATAL EXCEPTION").LeftAligned().Color(Color.Red));
            AnsiConsole.MarkupLineInterpolated($"[underline bold red1]LOG - FATAL:[/] {message}: {exception.Message} at\n {exception.Source}");
        }

        /// <summary>
        /// logs a debug message
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Debug(string message)
        {
            AnsiConsole.MarkupLineInterpolated($"[underline chartreuse2]LOG - DEBUG:[/] {message}");
        }
    }
}
