using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Helpers
{
    /// <summary>
    /// Contains all valid file extensions for a given class
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class FileExtensionAttribute : System.Attribute
    {
        private string extension;

        public string Extension { get { return this.extension; } }

        public FileExtensionAttribute(string extension)
        {
            this.extension = extension;
        }
    }
}
