using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Enumerations
{
    /// <summary>
    /// Contains all possible htcpcp status codes returned by the server
    /// </summary>
    public enum StatusCode
    {
        OK = 200,
        Deprecated = 309,
        BadRequest = 400,
        NotAcceptable = 406,
        Teapot = 416,
        UnavailableForLegalReasons = 451,
        NotImplemented = 501,
        InternalServerError = 500
    }
}
