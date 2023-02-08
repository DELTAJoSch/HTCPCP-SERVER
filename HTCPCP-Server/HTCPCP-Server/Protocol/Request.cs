using HTCPCP_Server.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Protocol
{
    internal class Request
    {
        public HTCPCPType Type { get; }
        public StatusCode Status { get; }
        public string? Pot { get; }
        public List<Tuple<Option, int>>? Additions { get; }

        /// <summary>
        /// Create a request that is valid
        /// </summary>
        /// <param name="type">The request type</param>
        /// <param name="pot">The pot it concerns</param>
        /// <param name="additions">The additions of the request</param>
        public Request(HTCPCPType type, string? pot, List<Tuple<Option, int>>? additions)
        {
            Type = type;
            Pot = pot;
            Status = StatusCode.OK;
            Additions = additions;
        }

        /// <summary>
        /// Create an invalid request
        /// </summary>
        /// <param name="type">The request type</param>
        /// <param name="code"></param>
        public Request(HTCPCPType type, StatusCode code)
        {
            Type = type;
            Status = code;
        }
    }
}
