using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using EasySocket.Core.Options;
using EasySocket.Core.Utils;
using Microsoft.Extensions.Logging;

namespace EasySocket.Core.Networks
{
    class EasySocketServer : IEasySocketServer
    {
        private readonly ServerOptions _options;

        private Action<IEasySocket> _connectAction;
        private Action<Exception> _exceptionAction;
        private TcpListener _tcpListener;
        private bool _acceptLoop = true;

        public ILogger Logger { get; set; }

        public EasySocketServer(ServerOptions options)
        {
            this._options = options;
        }

        public void ConnectHandler(Action<IEasySocket> action)
        {
            _connectAction = action;
            Logger?.LogDebug("[EasySocket Server] Add ConnectHandler");
        }

        public void ExceptionHandler(Action<Exception> action)
        {
            _exceptionAction = action;
            Logger?.LogDebug("[EasySocket Server] Add ExceptionHandler");
        }

        public void Run(int port)
        {
            _options.Port = port;
            Run();
        }

        public void Run()
        {
            try
            {
                _tcpListener = StartTcpListener();

                Task.Factory.StartNew(async () =>
                {
                    while (_acceptLoop)
                    {
                        Socket socket = await _tcpListener.AcceptSocketAsync();
                        string socketId = KeyGenerator.GetServerSocketId();
                        Logger?.LogInformation("[{0}] Connected - [{1}] -> [{2}]", socketId, socket.RemoteEndPoint, socket.LocalEndPoint);

                        _connectAction(new EasySocket(socketId, socket, Logger, _options));
                    }
                });
            }
            catch (Exception e)
            {
                _exceptionAction?.Invoke(e);
            }
        }

        public void Stop()
        {
            _acceptLoop = false;
            if (_tcpListener != null)
            {
                _tcpListener.Stop();
            }
        }

        private TcpListener StartTcpListener()
        {
            if (_connectAction == null)
            {
                Logger?.LogError("[EasySocket Server] Not found connectHandler");
                throw new InvalidOperationException("Not found connection action handler");
            }

            IPAddress addr = IPAddress.Parse(_options.Host);
            TcpListener tcpListener = new TcpListener(addr, _options.Port);

            tcpListener.Server.ReceiveBufferSize = _options.ReceiveBufferSize;
            tcpListener.Server.SendBufferSize = _options.SendBufferSize;
            tcpListener.Server.NoDelay = _options.NoDelay;
            tcpListener.Server.LingerState = _options.Linger;

            tcpListener.Start(_options.ListenBackLog);

            return tcpListener;
        }
    }
}
