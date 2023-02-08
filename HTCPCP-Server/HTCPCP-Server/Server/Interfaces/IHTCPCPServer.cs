using HTCPCP_Server.Database.Interfaces;
using HTCPCP_Server.Hardware.Interfaces;
using HTCPCP_Server.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Server.Interfaces
{
    internal interface IHTCPCPServer
    {
        /// <summary>
        /// Start the server instance on port with database
        /// </summary>
        /// <param name="port">The port to listen on</param>
        /// <param name="db">The database driver to use</param>
        /// <param name="coffee">The coffee maker</param>
        /// <returns>true if successful, otherwise false</returns>
        bool Start(int port, IDatabaseDriver db, ICoffeeMaker coffee);

        /// <summary>
        /// Stops the server
        /// </summary>
        /// <returns>Returns true if stopping was successful</returns>
        bool Stop();
    }
}
