using HTCPCP_Server.Database.Implementations;
using HTCPCP_Server.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Helpers
{
    /// <summary>
    /// Contains helpers for creating the correct db manager
    /// </summary>
    internal static class DbManagerCreationHelper
    {
        /// <summary>
        /// Create tasajdasdj
        /// </summary>
        /// <param name="info"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        internal static IDatabaseManager CreateFromFileType(FileInfo info, IDatabaseDriver driver) {
            return new XMLManager(driver);
        }
    }
}
