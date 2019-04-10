namespace HttpLoggingProxy
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using Symbotic.Framework.Logging;

    public class Proxy : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly int _destinationPort;
        private readonly int _fromPort;
        private readonly ILogger _logger;
        private readonly object _lock = new object();
        private readonly string _host;

        public Proxy(string host, int fromPort, int toPort, ILogger logger)
        {
            _host = host;
            _logger = logger;
            _destinationPort = toPort;
            _fromPort = fromPort;
            _listener = new TcpListener(IPAddress.Any, fromPort);

            new Thread(Start)
            {
                IsBackground = false,
                Name = $"p-{_fromPort}-{_destinationPort}"
            }
            .Start();
        }

        private void Start()
        {
            _listener.Start();
            _logger.LogInformation($"Start redirecting from {_fromPort} to {_destinationPort} port..");
            while (true)
            {
                try
                {
                    var requestClient = _listener.AcceptTcpClient();
                    var requestStream = ReadRequest(requestClient, out var message);
                    TcpClient destinationClient = null;
                    try
                    {
                        destinationClient = new TcpClient(_host, _destinationPort);
                        RedirectAndRespond(destinationClient, message, requestStream);
                    }
                    catch (SocketException x)
                    {
                        HandleRemoteException(x, requestStream);
                    }
                    finally
                    {
                        destinationClient?.Close();
                        requestClient.Close();
                    }

                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
            }
        }

        private void RedirectAndRespond(TcpClient destinationClient, byte[] message, NetworkStream requestStream)
        {
            var destinationStream = destinationClient.GetStream();

            destinationStream.Write(message, 0, message.Length);

            var responseMessage = ReadStreamToEnd(destinationStream);

            _logger.LogInformation(
                $"Response on port {_destinationPort}:\n" +
                Encoding.ASCII.GetString(responseMessage, 0, responseMessage.Length));

            requestStream.Write(responseMessage, 0, responseMessage.Length);
        }

        private NetworkStream ReadRequest(TcpClient requestClient, out byte[] message)
        {
            var requestStream = requestClient.GetStream();
            message = ReadStreamToEnd(requestStream);
            _logger.LogInformation(
                $"Received on {requestClient.Client.LocalEndPoint}:\n" +
                Encoding.ASCII.GetString(message, 0, message.Length));
            return requestStream;
        }

        private void HandleRemoteException(SocketException x, NetworkStream requestStream)
        {
            var exceptionMessage = "HTTP/1.1 500" + Environment.NewLine +
                                   $"Date: {DateTime.Now:ddd, dd MMM yyy HH:mm:ss GMT}" + Environment.NewLine +
                                   "Content - Type: text / plain; charset = UTF - 8" + Environment.NewLine +
                                   $"Content - Length: {x.Message}" + Environment.NewLine + Environment.NewLine + x;

            var response = Encoding.ASCII.GetBytes(exceptionMessage);
            requestStream.Write(response, 0, response.Length);

            _logger.LogInformation(
                $"Response on port {_destinationPort}:\n{x}");
        }

        private byte[] ReadStreamToEnd(NetworkStream stream)
        {
            var myReadBuffer = new byte[1024];
            var sb = new StringBuilder();
            int numberOfBytesRead;

            while (!stream.DataAvailable)
            {
                //Stabilizer
                Thread.Sleep(10);
            }

            while (stream.DataAvailable &&
                   (numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length)) > 0)
            {
                sb.Append(Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
            }

            return Encoding.ASCII.GetBytes(sb.ToString());

        }


        public void Dispose()
        {
            _listener.Stop();
        }
    }
}