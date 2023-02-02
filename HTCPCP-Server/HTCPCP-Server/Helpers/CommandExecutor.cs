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
                case "exit":
                    return Command.COMEXIT;
                case "":
                    return Command.NOCOMMAND;
                case "clear":
                    AnsiConsole.Clear();
                    AnsiConsole.WriteLine("CMD> ");
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
        }
    }
}
