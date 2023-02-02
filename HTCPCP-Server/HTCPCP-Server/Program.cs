using HTCPCP_Server.Database.Implementations;
using HTCPCP_Server.Database.Interfaces;
using HTCPCP_Server.Helpers;
using HTCPCP_Server.Logging;
using HTCPCP_Server.Server.Implementations;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

internal class Program
{
    private static int Main(string[] args)
    {
        AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        
        RootCommand root = new RootCommand(description: "Handles the server control");

        var fileOption = new Option<FileInfo?>(name: "--file", description: "File to load or to save to");
        fileOption.AddAlias("-f");
        fileOption.SetDefaultValue(new FileInfo("content.xml"));
        fileOption.Arity = ArgumentArity.ZeroOrOne;

        var verboseOption = new Option<bool>(name: "--verbose", description: "Verbose gives further information about what the server is currently doing");
        verboseOption.AddAlias("-v");
        verboseOption.Arity = ArgumentArity.ZeroOrOne;

        var portOption = new Option<int?>(name: "--port", description: "The port to listen on");
        portOption.SetDefaultValue(null);
        portOption.AddAlias("-p");

        var startCommand = new Command("start", "Starts the server") {
            portOption,
            verboseOption
        };

        var loadCommand = new Command("load", "Loads a file into the database") { 
            fileOption,
            verboseOption
        };

        var exportCommand = new Command("export", "Exports the database"){
            fileOption,
            verboseOption
        };

        root.AddCommand(startCommand);
        root.AddCommand(exportCommand);
        root.AddCommand(loadCommand);

        loadCommand.SetHandler(async (file, verbose) => { await Load(file, verbose); }, fileOption, verboseOption);
        exportCommand.SetHandler(async (file, verbose) => { await Export(file, verbose); }, fileOption, verboseOption);
        startCommand.SetHandler(async (port, verbose) => { await Start(port, verbose); }, portOption, verboseOption);

        return new CommandLineBuilder(root)
            .UseHelp()
            .UseTypoCorrections()
            .UseSuggestDirective()
            .RegisterWithDotnetSuggest()
            .UseParseErrorReporting()
            .CancelOnProcessTermination()
            .Build()
            .InvokeAsync(args)
            .Result;
    }

    /// <summary>
    /// Handles any unhalded exceptions
    /// </summary>
    /// <param name="sender">The event sender</param>
    /// <param name="e">The exception</param>
    /// <exception cref="NotImplementedException"></exception>
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log.Fatal("An unhandled excpetion has occurred. App Terminating", (Exception)e.ExceptionObject);
    }

    /// <summary>
    /// Handles any exceptions that come up
    /// </summary>
    /// <param name="sender">The sender</param>
    /// <param name="e">The exception</param>
    private static void CurrentDomain_FirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
    {
        Exception ex = e.Exception;
        Log.Debug($"An error has occured{ex.StackTrace}\n with message {ex.Message}");
    }

    /// <summary>
    /// Loads a file into the database
    /// </summary>
    /// <param name="load">The file to Load</param>
    /// <param name="verbose">Enable verbose logging</param>
    /// <returns>Returns 0 on success</returns>
    internal static async Task Load(FileInfo? load,bool verbose = false) {
        AnsiConsole.MarkupLine($"[underline fuchsia]Load Called:[/] File: {load?.Name}, Verbose: {verbose}");

        if (load == null) {
            Log.Info("File is null!");
            return;
        }
        
        IDatabaseDriver databaseDriver = new SQLiteDriver();
        IDatabaseManager? manager = DbManagerCreationHelper.CreateFromFileType(load, databaseDriver);

        if(manager == null)
        {
            Log.Info("No valid parser was found for file type.");
            databaseDriver.Dispose();
            return;
        }

        var res = await manager.Load(load);

        if (res)
        {
            Log.Info("Success! File was loaded.");
        }
        else 
        {
            Log.Info("File was not loaded!");
        }

        databaseDriver.Dispose();
    }

    /// <summary>
    /// Exports the database to the file
    /// </summary>
    /// <param name="export">The file to export to</param>
    /// <param name="verbose">Enable verbose logging</param>
    /// <returns>Returns 0 on success</returns>
    internal static async Task Export(FileInfo? export, bool verbose = false) {
        AnsiConsole.MarkupLine($"[underline turquoise2]Export Called:[/] File: {export?.Name}, Verbose: {verbose}");

        if (export == null)
        {
            Log.Info("No file!");
            return;
        }

        IDatabaseDriver databaseDriver = new SQLiteDriver();

        IDatabaseManager? manager = DbManagerCreationHelper.CreateFromFileType(export, databaseDriver);
        if(manager == null)
        {
            Log.Info("No parser found for file type");
            return;
        }

        await manager.Save(export);

        databaseDriver.Dispose();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="port"></param>
    /// <param name="verbose"></param>
    /// <returns></returns>
    internal static async Task Start(int? port, bool verbose = false) {
        if(port == null)
        {
            port = 80;
        }
        AnsiConsole.MarkupLine($"[underline orange1]Start Called:[/] Port: {port}, Verbose: {verbose}");

        Log.IsVerbose = verbose;

        var server = new Server();
        var dbdriver = new SQLiteDriver();
        if (server.Start(port.Value, dbdriver))
        {
            while (true)
            {
                var res = CommandExecutor.HandleCommand();
                if (res == HTCPCP_Server.Enumerations.Command.COMEXIT)
                {
                    server.Stop();
                    break;
                }
            }
        }
    }
}