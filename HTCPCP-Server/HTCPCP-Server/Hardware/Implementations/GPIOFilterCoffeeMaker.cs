using HTCPCP_Server.Enumerations;
using HTCPCP_Server.Hardware.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Gpio;
using System.Reflection.Metadata.Ecma335;

namespace HTCPCP_Server.Hardware.Implementations
{
    internal class GPIOFilterCoffeeMaker : ICoffeeMaker
    {
        private GpioController gpio;
        private Dictionary<string, Tuple<int, bool>> pots;

        public GPIOFilterCoffeeMaker() : this(new Dictionary<string, Tuple<int, bool>>()) { }

        /// <summary>
        /// Constructor for a GPIOCoffeeFilterMaker
        /// </summary>
        /// <param name="pots"></param>
        public GPIOFilterCoffeeMaker(Dictionary<string, Tuple<int, bool>> pots)
        {
            gpio = new GpioController(PinNumberingScheme.Board);
            this.pots = pots;
            foreach(var pot in pots)
            {
                this.gpio.SetPinMode(pot.Value.Item1, PinMode.Output);
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

            this.gpio.Write(potInfo.Item1, PinValue.High);
            Thread.Sleep(10);
            this.gpio.Write(potInfo.Item1, PinValue.Low);

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

            this.gpio.Write(potInfo.Item1, PinValue.High);
            Thread.Sleep(100);
            this.gpio.Write(potInfo.Item1, PinValue.Low);

            this.pots[pot] = new Tuple<int, bool>(potInfo.Item1, false);

            return true;
        }
    }
}
