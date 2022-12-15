using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Enumerations
{
    /// <summary>
    /// Contains all possible htcpcp request types
    /// </summary>
    public enum HTCPCPType
    {
        Get,
        Post,
        Put,
        Brew,
        Propfind,
        When
    }
}
