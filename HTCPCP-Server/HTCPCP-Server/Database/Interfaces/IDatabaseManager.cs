using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Database.Interfaces
{
    /// <summary>
    /// Describes a class that can Load or Save a database
    /// ALL implementations must include a constructor with the single Parameter of type IDatabaseDriver
    /// in order to be picked up by the reflection algorithm
    /// </summary>
    internal interface IDatabaseManager
    {
        /// <summary>
        /// Load a file of a compatible type into the database
        /// Replaces all data currently in the database
        /// </summary>
        /// <param name="file">The file to Load</param>
        /// <returns>Returns true if successful</returns>
        Task<bool> Load(FileInfo file);

        /// <summary>
        /// Saves the content of the database to a given file
        /// </summary>
        /// <param name="file">The file to Save the content to</param>
        /// <returns>Returns true if successful</returns>
       Task<bool> Save(FileInfo file);
    }
}
