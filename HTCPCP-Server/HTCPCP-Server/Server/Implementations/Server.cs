using HTCPCP_Server.Database.Interfaces;
using HTCPCP_Server.Helpers;
using HTCPCP_Server.Logging;
using HTCPCP_Server.Protocol;
using HTCPCP_Server.Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Server.Implementations
{
    internal class Server : IHTCPCPServer
    {
        private IDatabaseDriver? driver;
        private int port = 0;
        private bool running = false;
        private CancellationTokenSource? token = null;

        /// <summary>
        /// Builds a response from a given request
        /// </summary>
        /// <param name="request">The request to parse</param>
        /// <returns>Returns a parsed response</returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<Response> BuildResponse(Request request)
        {
            switch (request.Status)
            {
                case Enumerations.StatusCode.OK:
                    break;
                case Enumerations.StatusCode.Deprecated:
                    return new Response(request.Status, "Deprecated", 0, "", "yes");
                case Enumerations.StatusCode.BadRequest:
                    return new Response(request.Status, "Bad Request", 0, "", "if-user-awake");
                case Enumerations.StatusCode.NotAcceptable:
                    return new Response(request.Status, "Not Acceptable", 0, "", "if-user-awake");
                case Enumerations.StatusCode.Teapot:
                    return new Response(request.Status, "I'm a Teapot", 0, "", "no");
                case Enumerations.StatusCode.UnavailableForLegalReasons:
                    return new Response(request.Status, "Unavailable for Legal reasons", 0, "", "yes");
                case Enumerations.StatusCode.NotImplemented:
                    return new Response(request.Status, "Not Implemented", 0, "", "yes");
            }

            // do something here!

            return new Response(request.Status, "OK", 0, "", "no");
        }

        /// <summary>
        /// Start the server instance on port with database
        /// </summary>
        /// <param name="port">The port to listen on</param>
        /// <param name="db">The database driver to use</param>
        /// <returns>true if successful, otherwise false</returns>
        public bool Start(int port, IDatabaseDriver db)
        {
            if (!this.running)
            {
                this.token = new CancellationTokenSource();
                this.driver = db;
                this.port = port;
                this.running = true;
                Task.Factory.StartNew( async () => {await this.HandleClients(); }, this.token.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
                return true;
            }
            else
            {
                Log.Info("Tried to start already running server! Ignoring request.");
                return false;
            }
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        /// <returns>Returns true if stopping was successful</returns>
        public bool Stop()
        {
            if(!this.running)
            {
                Log.Info("Tried to stop non-running server! Ignoring request.");
                return false;
            }

            this.token?.Cancel();
            this.running = false;
            return true;
        }

        /// <summary>
        /// Starts the socket and handles incoming requests
        /// </summary>
        /// <returns></returns>
        private async Task HandleClients()
        {
            if(this.token == null)
            {
                Log.Error("Task does not have a cancellation token!");
                return;
            }

            var ipEndPoint = new IPEndPoint(IPAddress.Any, this.port);
            TcpListener listener = new TcpListener(ipEndPoint);
            List<Task> connections = new List<Task>();

            try
            {
                listener.Start();
                while (!this.token!.IsCancellationRequested)
                {
                    try
                    {
                        var accepted = await listener.AcceptTcpClientAsync();
                        connections.Add(Task.Factory.StartNew(async () => { await this.HandleConnection(accepted); }));
                    }
                    catch (SocketException ex)
                    {
                        Log.Warn($"Failed to accept socket: {ex.Message}");
                    }
                }
            }
            finally
            {
                listener.Stop();
                foreach(var connection in connections)
                {
                    await connection;
                }
            }
        }

        /// <summary>
        /// Handles a single client connection
        /// </summary>
        /// <param name="client">The client to handle</param>
        /// <returns></returns>
        private async Task HandleConnection(TcpClient client)
        {
            using (NetworkStream stream = client.GetStream())
            {
                string requeststring = "";
                int bytelength = 0;
                bool timedout = false;

                // this parsing doesn't quite work - we need to do this differently (on the fly)
                while ((!requeststring.EndsWith("\r\n\r\n")) && (bytelength < 8192))
                {
                    TimeSpan timeout = new TimeSpan(0, 0, 30);
                    CancellationTokenSource ct = new CancellationTokenSource(timeout);
                    using (ct.Token.Register(() => stream.Close()))
                    {
                        try
                        {
                            byte[] buf = new byte[1024];
                            var length = await stream.ReadAsync(buf, 0, buf.Length, ct.Token).ConfigureAwait(false);
                            if (length > 0)
                            {
                                bytelength += length;
                                var append = Encoding.UTF8.GetString(buf);
                                requeststring += append.Replace("\\r\\n", "\r\n");
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            timedout = true;
                            break;
                        }
                    }

                    ct.Dispose();
                }


                Log.Verbose($"Got request: {requeststring}");

                if (bytelength >= 8192)
                {
                    string resp = $"HTCPCP/1.0 414 URI Too Long\r\n" +
                        $"Date: {DateTime.UtcNow.ToString("R")}\r\n" +
                        $"Server: HTCPCP-Server v1.0\r\n" +
                        $"Content-Length: 0\r\n" +
                        $"Safe: if-user-awake\r\n" +
                        $"Connection: Closed\r\n\r\n";

                    Log.Verbose($"Sending: {resp}");

                    byte[] buf = Encoding.UTF8.GetBytes(resp);
                    stream.Write(buf, 0, buf.Length);
                    stream.Close();
                }
                else if (timedout)
                {
                    Log.Verbose("Timed out!");
                }
                else
                {
                    var resp = await this.BuildResponse(RequestParser.Parse(requeststring));
                    string respString = $"HTCPCP/1.0 {(int)resp.Status} {resp.StatusMessage}\r\n" +
                        $"Date: {DateTime.UtcNow.ToString("R")}\r\n" +
                        $"Server: HTCPCP-Server v1.0\r\n" +
                        $"Safe: {resp.Safe}\r\n" +
                        $"Content-Length: {resp.ContentLength}\r\n" +
                        $"Connection: Closed\r\n" +
                        $"\r\n{resp.Content}";

                    Log.Verbose($"Sending: {respString}");

                    byte[] buf = Encoding.UTF8.GetBytes(respString);
                    stream.Write(buf, 0, buf.Length);
                    stream.Close();
                }
            }


           client.Close();
        }
    }
}
