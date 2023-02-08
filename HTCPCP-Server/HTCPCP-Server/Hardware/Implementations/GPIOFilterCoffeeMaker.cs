using HTCPCP_Server.Enumerations;
using HTCPCP_Server.Hardware.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Gpio;
using System.Reflection.Metadata.Ecma335;
using HTCPCP_Server.Logging;

namespace HTCPCP_Server.Hardware.Implementations
{
    internal class GPIOFilterCoffeeMaker : ICoffeeMaker
    {
        private GpioController gpio;
        private Dictionary<string, Tuple<int, bool>> pots;
        private System.Timers.Timer finishedTimer;

        public GPIOFilterCoffeeMaker() : this(new Dictionary<string, Tuple<int, bool>>()) { }

        /// <summary>
        /// Constructor for a GPIOCoffeeFilterMaker
        /// </summary>
        /// <param name="pots"></param>
        public GPIOFilterCoffeeMaker(Dictionary<string, Tuple<int, bool>> pots)
        {
            gpio = new GpioController(PinNumberingScheme.Logical);
            this.pots = pots;
            this.finishedTimer = new System.Timers.Timer(60 * 5 * 1000);
            this.finishedTimer.Elapsed += FinishedTimer_Elapsed;

            foreach(var pot in pots)
            {
                this.gpio.OpenPin(pot.Value.Item1);
                this.gpio.SetPinMode(pot.Value.Item1, PinMode.Output);
            }
        }

        /// <summary>
        /// Reset the state of all pots aftter 5mins
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void FinishedTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var pot in pots)
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

            this.finishedTimer.Start();

            Log.Verbose($"Pin {potInfo.Item1} High");
            this.gpio.Write(potInfo.Item1, PinValue.High);
            Thread.Sleep(100);
            this.gpio.Write(potInfo.Item1, PinValue.Low);
            Log.Verbose($"Pin {potInfo.Item1} Low");

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

            this.finishedTimer.Stop();

            Log.Verbose($"Pin {potInfo.Item1} High");
            this.gpio.Write(potInfo.Item1, PinValue.High);
            Thread.Sleep(250);
            this.gpio.Write(potInfo.Item1, PinValue.Low);
            Log.Verbose($"Pin {potInfo.Item1} Low");

            this.pots[pot] = new Tuple<int, bool>(potInfo.Item1, false);

            return true;
        }
    }
}
