using HTCPCP_Server.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Hardware.Interfaces
{
    internal interface ICoffeeMaker
    {
        /// <summary>
        /// Starts the production of the specified pot
        /// </summary>
        /// <param name="pot">The pot</param>
        /// <param name="additions">The list of additions to add</param>
        /// <returns>Returns true if successful</returns>
        public bool StartProduction(string pot, List<Tuple<Option, int>> additions);

        /// <summary>
        /// Stops the production of the specified pot
        /// </summary>
        /// <param name="pot">The pot identifier</param>
        /// <returns>Returns true if the production was stopped successfully</returns>
        public bool StopProduction(string pot);
    }
}
