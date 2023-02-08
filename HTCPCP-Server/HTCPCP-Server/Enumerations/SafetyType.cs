using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Enumerations
{
    /// <summary>
    /// Contains the possible retry-safety options
    /// </summary>
    public enum SafetyType
    {
        No,
        Yes,
        Token,
        UserAwake
    }
}
