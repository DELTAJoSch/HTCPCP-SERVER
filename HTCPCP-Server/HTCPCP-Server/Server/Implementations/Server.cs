using HTCPCP_Server.Database.Interfaces;
using HTCPCP_Server.Enumerations;
using HTCPCP_Server.Hardware.Interfaces;
using HTCPCP_Server.Helpers;
using HTCPCP_Server.Logging;
using HTCPCP_Server.Protocol;
using HTCPCP_Server.Server.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace HTCPCP_Server.Server.Implementations
{
    internal class Server : IHTCPCPServer
    {
        private IDatabaseDriver? driver;
        private ICoffeeMaker? coffee;
        private int port = 0;
        private bool running = false;
        private CancellationTokenSource? token = null;

        /// <summary>
        /// Builds a response from a given request
        /// </summary>
        /// <param name="request">The request to parse</param>
        /// <param name="messageTypeOk"></param>
        /// <param name="body"></param>
        /// <returns>Returns a parsed response</returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<Response> BuildResponse(Request request, bool messageTypeOk, string body)
        {
            switch (request.Status)
            {
                case StatusCode.OK:
                    return this.HandleBrew(request, messageTypeOk, body);
                case StatusCode.Deprecated:
                    return new Response(request.Status, "Deprecated", 0, "", "yes");
                case StatusCode.BadRequest:
                    return new Response(request.Status, "Bad Request", 0, "", "if-user-awake");
                case StatusCode.NotAcceptable:
                    return new Response(request.Status, "Not Acceptable", 0, "", "if-user-awake");
                case StatusCode.Teapot:
                    return new Response(request.Status, "I'm a Teapot", 0, "", "no");
                case StatusCode.UnavailableForLegalReasons:
                    return new Response(request.Status, "Unavailable for Legal reasons", 0, "", "yes");
                case StatusCode.NotImplemented:
                    return new Response(request.Status, "Not Implemented", 0, "", "yes");
            }

            return new Response(StatusCode.InternalServerError, "Internal Server Error", 0, "", "yes");
        }

        /// <summary>
        /// Handles a brew request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="messageTypeOk">Whether the message type was as expected</param>
        /// <param name="body">The message body</param>
        /// <returns>Returns a response</returns>
        private Response HandleBrew(Request request, bool messageTypeOk, string body)
        {
            if (!messageTypeOk)
            {
                return new Response(StatusCode.BadRequest, "Bad Request", 0, "", "if-user-awake");
            }

            bool start;
            if (!RequestParser.BodyConform(body, out start))
                return new Response(StatusCode.BadRequest, "Bad Request", 0, "", "if-user-awake");

            if (request.Additions != null && this.driver != null && this.coffee != null)
            {
                if (start)
                {
                    request.Additions.Add(new Tuple<Option, int>(Option.Coffee, 1));

                    // check whether the request is fulfillable
                    lock (this.driver)
                    {
                        bool makeable = true;
                        foreach (var item in request.Additions)
                        {
                            makeable &= this.driver.CheckAvailable(item.Item1, request.Pot ?? "pot-0", item.Item2);
                        }

                        if (!makeable)
                        {
                            string available = "";
                            return new Response(StatusCode.NotAcceptable, "Not Acceptable", Encoding.UTF8.GetByteCount(available), available, "if-user-awake");
                        }
                        else
                        {
                            foreach (var item in request.Additions)
                            {
                                this.driver.Consume(item.Item1, request.Pot ?? "pot-0", item.Item2);
                            }
                        }
                    }

                    bool rollBackNeeded = false;

                    lock (this.coffee)
                    {
                        if (!coffee.StartProduction(request.Pot ?? "pot-0", request.Additions))
                        {
                            rollBackNeeded = true;
                        }
                    }

                    // the execution on the pot failed. Rollback of database needed, otherwise state is inconsistent
                    if (rollBackNeeded)
                    {
                        lock (this.driver)
                        {
                            foreach (var item in request.Additions)
                            {
                                this.driver.Add(item.Item1, request.Pot ?? "pot-0", item.Item2);
                            }
                        }

                        return new Response(StatusCode.InternalServerError, "Internal Server Error", 0, "", "yes");
                    }
                }
                else
                {
                    lock (this.coffee)
                    {
                        if (!this.coffee.StopProduction(request.Pot ?? "pot-0"))
                        {
                            return new Response(StatusCode.InternalServerError, "Internal Server Error", 0, "", "yes");
                        }
                    }
                }
            }
            else
            {
                return new Response(StatusCode.InternalServerError, "Internal Server Error", 0, "", "no");
            }

            return new Response(request.Status, "OK", 0, "", "no");
        }

        /// <summary>
        /// Start the server instance on port with database
        /// </summary>
        /// <param name="port">The port to listen on</param>
        /// <param name="db">The database driver to use</param>
        /// <returns>true if successful, otherwise false</returns>
        public bool Start(int port, IDatabaseDriver db, ICoffeeMaker coffee)
        {
            if (!this.running)
            {   
                this.coffee = coffee;
                this.token = new CancellationTokenSource();
                this.driver = db;
                this.port = port;
                this.running = true;
                Task.Factory.StartNew(async () => { await this.HandleClients().ConfigureAwait(false); }, this.token.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
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
            if (!this.running)
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
            if (this.token == null)
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
                        var accepted = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                        connections.Add(Task.Factory.StartNew(async () => { await this.HandleConnection(accepted).ConfigureAwait(false); }));
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
                foreach (var connection in connections)
                {
                    await connection.ConfigureAwait(false);
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
                Request? request = null;

                string received = "";
                int bytelength = 0;
                bool timedout = false;
                string line = "";

                System.Timers.Timer timeoutTimer = new System.Timers.Timer(30000);
                timeoutTimer.Elapsed += (Object? source, ElapsedEventArgs e) => { timedout = true; };
                timeoutTimer.Start();

                // Request Line
                bool requestRead = false;
                while ((!requestRead) && (bytelength < 4096) && (!timedout))
                {
                    TimeSpan timeout = new TimeSpan(0, 0, 30);
                    using (CancellationTokenSource ct = new CancellationTokenSource(timeout))
                    {
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
                                    received += append.Replace("\\r\\n", "\r\n");
                                    int lineEnd = received.IndexOf("\r\n");
                                    if (lineEnd > 0)
                                    {
                                        line = received.Substring(0, lineEnd);
                                        received = received.Substring(lineEnd);
                                        requestRead = true;

                                        request = RequestParser.ParseRequest(line);
                                    }
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                timedout = true;
                                break;
                            }
                        }
                    }
                }

                bool messageType = false;
                int contentLength = 0;

                // Header Fields
                if (request != null && request.Status == StatusCode.OK && !timedout)
                {
                    bool fieldEnd = false;
                    string header = "";

                    if (received.Contains("\r\n\r\n"))
                    {
                        fieldEnd = true;
                        int pos = received.IndexOf("\r\n\r\n");
                        header = received.Substring(0, pos);
                        received = received.Substring(pos + 4);
                    }

                    while ((!fieldEnd) && (bytelength < 4096) && (!timedout))
                    {
                        TimeSpan timeout = new TimeSpan(0, 0, 30);

                        using (CancellationTokenSource ct = new CancellationTokenSource(timeout))
                        {
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
                                        received += append.Replace("\\r\\n", "\r\n");

                                        if (received.Contains("\r\n\r\n"))
                                        {
                                            fieldEnd = true;
                                            int pos = received.IndexOf("\r\n\r\n");
                                            header = received.Substring(0, pos);
                                            received = received.Substring(pos + 4);
                                        }
                                    }
                                }
                                catch (OperationCanceledException)
                                {
                                    timedout = true;
                                    break;
                                }
                            }
                        }
                    }

                    messageType = RequestParser.ParseHeaderFields(header, out contentLength);
                }

                // Body
                if (contentLength > 0)
                {
                    received = received.Trim('\0');
                    int read = Encoding.UTF8.GetByteCount(received);

                    while ((read < contentLength) && (bytelength < 4096) && (!timedout))
                    {
                        TimeSpan timeout = new TimeSpan(0, 0, 30);
                        using (CancellationTokenSource ct = new CancellationTokenSource(timeout))
                        {
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
                                        received += append.Replace("\\r\\n", "\r\n");
                                    }
                                }
                                catch (OperationCanceledException)
                                {
                                    timedout = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                timeoutTimer.Stop();

                Log.Verbose($"Got request: {received}");

                if (bytelength >= 4096)
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
                else if (request != null)
                {
                    var response = await this.BuildResponse(request, messageType, received).ConfigureAwait(false);
                    string resp = $"HTCPCP/1.0 {(int)response.Status} {response.StatusMessage}\r\n" +
                        $"Date: {DateTime.UtcNow.ToString("R")}\r\n" +
                        $"Server: HTCPCP-Server v1.0\r\n" +
                        $"Safe: {response.Safe}\r\n" +
                        $"Content-Length: {response.ContentLength}\r\n" +
                        $"Connection: Closed\r\n" +
                        $"\r\n{response.Content}";

                    Log.Verbose($"Sending: {resp}");

                    byte[] buf = Encoding.UTF8.GetBytes(resp);
                    stream.Write(buf, 0, buf.Length);
                    stream.Close();
                }
                else
                {
                    Log.Verbose("No request detected!");
                }
            }


            client.Close();
        }
    }
}
