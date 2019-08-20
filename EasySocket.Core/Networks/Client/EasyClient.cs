using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using EasySocket.Core.Networks.Base.Configuration;
using EasySocket.Core.Networks.Client.Configuration;
using EasySocket.Core.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasySocket.Core.Networks.Client
{
    public class EasyClient
    {
        private readonly ILogger<EasyClient> _logger;
        private readonly IConfiguration _config;

        private Action<IEasyClientSocket> _connectAction;
        private Action<Exception> _exceptionAction;

        public EasyClientConfiguration EasyClientConfiguration { get; private set; }
        public SocketConfiguration SocketConfiguration { get; private set; }

        public EasyClient(ILogger<EasyClient> logger = null, IConfiguration config = null)
        {
            _logger = logger;
            _config = config;

            try
            {
                if (config != null)
                {
                    EasyClientConfiguration = _config.GetSection("EasyClient").Get<EasyClientConfiguration>();
                    SocketConfiguration = _config.GetSection("EasyClient").GetSection("EasySocket").Get<SocketConfiguration>();
                }
                else
                {
                    EasyClientConfiguration = new EasyClientConfiguration();
                    SocketConfiguration = new SocketConfiguration();
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to initialize easy client...", exception);
            }
        }

        public void Connect(string address, int port)
        {
            if (_connectAction == null)
            {
                _logger?.LogError("[EasySocket Client] Not found connectHandler");
                throw new InvalidOperationException("Not found connectHandler");
            }

            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Socket socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();
            connectArgs.UserToken = socket;
            connectArgs.RemoteEndPoint = remoteEndPoint;
            connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(CompleteConnectEvent);
            socket.ConnectAsync(connectArgs);
        }

        private void CompleteConnectEvent(object sender, SocketAsyncEventArgs args)
        {
            SocketError errorCode = args.SocketError;
            if (errorCode != SocketError.Success)
            {
                _exceptionAction?.Invoke(new SocketException((int)errorCode));
                return;
            }

            IEasyClientSocket clientSocket = new EasyClientSocket(_logger, SocketConfiguration)
            {
                SocketId = KeyGenerator.GetClientSocketId(),
                Socket = (Socket)args.UserToken
            };

            clientSocket.Socket.NoDelay = EasyClientConfiguration.NoDelay;
            _connectAction.Invoke(clientSocket);
        }

        public void ConnectHandler(Action<IEasyClientSocket> action)
        {
            _connectAction = action;
            _logger?.LogDebug("[EasySocket Client] Add ConnectHandler");
        }

        public void ExceptionHandler(Action<Exception> action)
        {
            _exceptionAction = action;
            _logger?.LogDebug("[EasySocket Client] Add ExceptionHandler");
        }


    }
}
