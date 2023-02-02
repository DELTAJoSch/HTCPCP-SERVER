using HTCPCP_Server.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Protocol
{
    internal class Response
    {
        public StatusCode Status { get; }
        public string StatusMessage { get; }
        public int ContentLength { get; }
        public string Content { get; }
        public string Safe { get; }


        public Response(StatusCode statusCode, string statusMessage, int contentLength, string content, string safe)
        {
            Status = statusCode;
            StatusMessage = statusMessage;
            ContentLength = contentLength;
            Content = content;
            Safe = safe;
        }
    }
}
