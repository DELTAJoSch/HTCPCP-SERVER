using HTCPCP_Server.Database.Implementations;
using HTCPCP_Server.Database.Interfaces;
using HTCPCP_Server.Helpers;
using HTCPCP_Server.Logging;
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

        var configOption = new Option<FileInfo?>(name: "--endpoint", description: "The config file");
        configOption.SetDefaultValue(null);
        configOption.AddAlias("-e");

        var startCommand = new Command("start", "Starts the server") { 
            configOption,
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

        loadCommand.SetHandler(async (file, verbose) => { await load(file, verbose); }, fileOption, verboseOption);
        exportCommand.SetHandler(async (file, verbose) => { await export(file, verbose); }, fileOption, verboseOption);
        startCommand.SetHandler(async (config, verbose) => { await start(config, verbose); }, configOption, verboseOption);

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
    internal static async Task load(FileInfo? load,bool verbose = false) {
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
    internal static async Task export(FileInfo? export, bool verbose = false) {
        AnsiConsole.MarkupLine($"[underline turqouise1]Export Called:[/] File: {export?.Name}, Verbose: {verbose}");
    }

    internal static async Task start(FileInfo? config, bool verbose = false) {
        AnsiConsole.MarkupLine($"[underline orange1]Start Called:[/] File: {config?.Name}, Verbose: {verbose}");
    }
}