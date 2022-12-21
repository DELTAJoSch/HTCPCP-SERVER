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
        /// Create the correct databse manager for a given file type
        /// </summary>
        /// <param name="info">The file info of the file the manager should ingest</param>
        /// <param name="driver">The databse driver the manager should use</param>
        /// <returns>Returns a database manager or null if no valid manager was found</returns>
        internal static IDatabaseManager? CreateFromFileType(FileInfo info, IDatabaseDriver driver) {
            // go over all types in the asembly and select thhose that
            // are valid database managers
            // then check whether the file type matches and select the first one that fits
            Type? ManagerType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                {
                    if (type.IsInterface || type.IsAbstract)
                        return false;

                    return type.GetInterfaces().Contains(typeof(IDatabaseManager));
                })
                .Where(type =>
                {
                    var attribute = Attribute.GetCustomAttributes(type, typeof(FileExtensionAttribute))
                    .Cast<FileExtensionAttribute>()
                    .Where(att => att.Extension == info.Extension)
                    .FirstOrDefault();

                    return attribute != null;
                })
                .FirstOrDefault();

            return (ManagerType != null) ? Activator.CreateInstance(ManagerType, driver) as IDatabaseManager : null;
        }
    }
}
