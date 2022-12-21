using HTCPCP_Server.Enumerations;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HTCPCP_Server.Helpers;

namespace HTCPCP_Server.Database.Interfaces
{
    /// <summary>
    /// Describes a database driver
    /// </summary>
    internal interface IDatabaseDriver: IDisposable
    {
        /// <summary>
        /// Consumes one element of type option from reserve of pot
        /// </summary>
        /// <param name="option">The option to consume</param>
        /// <param name="pot">The pot to consume from</param>
        /// <returns>Returns true if possible</returns>
        /// <exception cref="ConnectionLostException">if connection is lost</exception>
        Task<bool> Consume(Option option, string pot);

        /// <summary>
        /// Consumes multiple elements of option from pot
        /// PRE-CONDITION: count must be larger than 0
        /// </summary>
        /// <param name="option">The option to consume</param>
        /// <param name="pot">The pot to consume from</param>
        /// <param name="count">The count to consume</param>
        /// <returns>Returns true if possible</returns>
        /// <exception cref="ArgumentOutOfRangeException"> if count is < 1</exception>
        /// <exception cref="ConnectionLostException">if connection is lost</exception>
        Task<bool> Consume(Option option, string pot, int count);

        /// <summary>
        /// Increases number of available uses for option of pot
        /// </summary>
        /// <param name="option">The option to increase</param>
        /// <param name="pot">The pot to add the option to</param>
        /// <param name="count">Count of added options (Default: 1)</param>
        /// <exception cref="ArgumentOutOfRangeException"> if count is < 1</exception>
        /// <exception cref="ConnectionLostException">if connection is lost</exception>
        Task Add(Option option, string pot, int count = 1);

        /// <summary>
        /// Gets the entire database as a Dict(pot --> Dict(option --> count))
        /// </summary>
        /// <returns>Returns the database as a dict or null if not possible</returns>
        Task<Dictionary<string, Dictionary<Option, int>>?> GetAsDict();

        /// <summary>
        /// Loads the dictionary into the database.
        /// Replaces existing content
        /// </summary>
        /// <param name="dict">The Dict(pot --> Dict(option --> count))</param>
        /// <returns>Returns true if successful</returns>
        Task<bool> InitDb(Dictionary<string, Dictionary<Option, int>> dict);
    }
}
