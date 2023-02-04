using HTCPCP_Server.Database.Interfaces;
using HTCPCP_Server.Logging;
using Spectre.Console;
namespace HTCPCP_Server.Helpers
{
    internal static class ReloadCoffeeCommand
    {
        /// <summary>
        /// Reload the coffee machine
        /// </summary>
        /// <param name="driver"></param>
        public static void ReloadCoffee(IDatabaseDriver driver)
        {
            Log.Block();
            string name = AnsiConsole.Ask<string>("Enter pot name> ", "pot-0");
            int count = AnsiConsole.Ask<int>("Enter coffee amount> ", 1);

            lock(driver)
            {
                driver.Add(Enumerations.Option.Coffee, name, count);
            }
            Log.Unblock();
        }
    }
}
