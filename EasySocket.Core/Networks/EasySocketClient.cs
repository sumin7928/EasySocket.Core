using EasySocket.Core.Options;
using EasySocket.Core.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EasySocket.Core.Networks
{
    class EasySocketClient : IEasySocketClient
    {
        private readonly ClientOptions _options;

        private Action<IEasySocket> _connectAction;
        private Action<Exception> _exceptionAction;

        public ILogger Logger { get; set; }

        public EasySocketClient(ClientOptions options)
        {
            this._options = options;
        }

        public void Connect()
        {
            Connect(_options.Host, _options.Port);
        }

        public void Connect(string host, int port)
        {
            if (_connectAction == null)
            {
                Logger?.LogError("[EasySocket Client] Not found connectHandler");
                throw new InvalidOperationException("Not found connectHandler");
            }

            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.BeginConnect(host, port, new AsyncCallback(ConnectCallback), socket);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            socket.EndConnect(ar);

            var socketId = KeyGenerator.GetClientSocketId();

            var tcpSocket = new EasySocket(socketId, socket, Logger, _options);
            Logger?.LogInformation("[{0}] Connected - [{1}] -> [{2}]", socketId, socket.LocalEndPoint, socket.RemoteEndPoint);

            _connectAction(tcpSocket);
        }

        public void ConnectHandler(Action<IEasySocket> action)
        {
            _connectAction = action;
            Logger?.LogDebug("[EasySocket Client] Add ConnectHandler");
        }

        public void ExceptionHandler(Action<Exception> action)
        {
            _exceptionAction = action;
            Logger?.LogDebug("[EasySocket Client] Add ExceptionHandler");
        }
    }
}
