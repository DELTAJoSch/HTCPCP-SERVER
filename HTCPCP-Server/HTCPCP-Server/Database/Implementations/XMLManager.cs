using HTCPCP_Server.Database.Interfaces;
using HTCPCP_Server.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Database.Implementations
{
    /// <summary>
    /// A Database Manager for XML files
    /// </summary>
    [FileExtension(".xml")]
    [FileExtension(".XML")]
    internal class XMLManager : IDatabaseManager
    {
        private IDatabaseDriver driver;

        /// <summary>
        /// Creates an xml manager with the given database driver
        /// </summary>
        /// <param name="driver">The driver to use when creating</param>
        public XMLManager(IDatabaseDriver driver)
        {
            this.driver = driver;
        }

        /// <summary>
        /// Loads an xml file into the database
        /// Replaces existing content of the database
        /// </summary>
        /// <param name="file">The file to Load</param>
        /// <returns>Returns true if successful</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Load(FileInfo file)
        {
            return true;
        }

        /// <summary>
        /// Creates an xml file from the database
        /// </summary>
        /// <param name="file">The file to write to</param>
        /// <returns>Returns true if successful</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Save(FileInfo file)
        {
            throw new NotImplementedException();
        }
    }
}
