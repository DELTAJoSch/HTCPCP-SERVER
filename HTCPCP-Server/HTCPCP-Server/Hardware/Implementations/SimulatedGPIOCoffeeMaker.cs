using HTCPCP_Server.Enumerations;
using HTCPCP_Server.Hardware.Interfaces;
using HTCPCP_Server.Logging;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Hardware.Implementations
{
    internal class SimulatedGPIOCoffeeMaker: ICoffeeMaker
    {
        private Dictionary<string, Tuple<int, bool>> pots;
        private System.Timers.Timer timer = new System.Timers.Timer(10000);

        public SimulatedGPIOCoffeeMaker() : this(new Dictionary<string, Tuple<int, bool>>()) { }

        /// <summary>
        /// Constructor for a GPIOCoffeeFilterMaker
        /// </summary>
        /// <param name="pots"></param>
        public SimulatedGPIOCoffeeMaker(Dictionary<string, Tuple<int, bool>> pots)
        {
            this.pots = pots;
            this.timer.Elapsed += Timer_Elapsed;
            this.timer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            foreach(var pot in pots)
            {
                this.pots[pot.Key] = new Tuple<int, bool>(pot.Value.Item1, false);
            }
        }

        /// <summary>
        /// Starts the production of the specified pot
        /// </summary>
        /// <param name="pot">The pot</param>
        /// <param name="additions">The list of additions to add</param>
        /// <returns>Returns true if successful</returns>
        public bool StartProduction(string pot, List<Tuple<Option, int>> additions)
        {
            var potInfo = this.pots[pot];

            if (potInfo == null)
                return false;

            if (potInfo.Item2)
                return false;

            Log.Debug($"{pot} - Pin {potInfo.Item1}: HIGH");
            Thread.Sleep(10);
            Log.Debug($"{pot} - Pin {potInfo.Item1}: LOW");

            this.pots[pot] = new Tuple<int, bool>(potInfo.Item1, true);

            return true;
        }

        /// <summary>
        /// Stops the production of the specified pot
        /// </summary>
        /// <param name="pot">The pot identifier</param>
        /// <returns>Returns true if the production was stopped successfully</returns>
        public bool StopProduction(string pot)
        {
            var potInfo = this.pots[pot];

            if (potInfo == null)
                return false;

            if (!potInfo.Item2)
                return false;

            Log.Debug($"{pot}  - Pin {potInfo.Item1}: LOW");
            Thread.Sleep(100);
            Log.Debug($"{pot}  - Pin {potInfo.Item1}: HIGH");

            this.pots[pot] = new Tuple<int, bool>(potInfo.Item1, false);

            return true;
        }
    }
}
