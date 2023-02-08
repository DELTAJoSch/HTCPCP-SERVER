using HTCPCP_Server.Enumerations;
using HTCPCP_Server.Logging;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Helpers
{
    static internal class CommandExecutor
    {
        public static Command HandleCommand()
        {
            string command = AnsiConsole.Ask<string>("CMD> ");
            switch (command)
            {
                case "help":
                    Help();
                    return Command.NOCOMMAND;
                case "reload":
                    return Command.RELOAD;
                case "exit":
                    return Command.COMEXIT;
                case "verbose=false":
                    Log.IsVerbose = false;
                    Log.Info("Verbose mode deactivated!");
                    return Command.NOCOMMAND;
                case "verbose=true":
                    Log.IsVerbose = true;
                    Log.Info("Verbose mode activated!");
                    return Command.NOCOMMAND;
                case "":
                    return Command.NOCOMMAND;
                case "clear":
                    AnsiConsole.Clear();
                    return Command.NOCOMMAND;
                default:
                    Log.Info("Unknown Command: type help for more info on available commands");
                    return Command.COMUNKNWON;
            }
        }

        private static void Help()
        {
            AnsiConsole.MarkupLine("[bold deeppink1]--HELP--[/]");
            AnsiConsole.MarkupLine("[lightsteelblue]help:[/] Display information about available commands");
            AnsiConsole.MarkupLine("[lightsteelblue]exit:[/] Exit server");
            AnsiConsole.MarkupLine("[lightsteelblue]clear:[/] Clear Console");
            AnsiConsole.MarkupLine("[lightsteelblue]verbose:[/] Set verbosity: verbose=true | verbose=false");
            AnsiConsole.MarkupLine("[lightsteelblue]reload:[/] Enter Menu to reload coffee");
        }
    }
}
