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
    public class EasySocketServer : IEasySocketServer
    {
        private Action<IEasySocket> _connectAction;
        private Action<Exception> _exceptionAction;
        private TcpListener _tcpListener;
        private bool _acceptLoop = true;

        public ILogger<EasySocketServer> Logger { get; private set; }
        public ServerOptions ServerOptions { get; set; } = new ServerOptions();

        public EasySocketServer(ILogger<EasySocketServer> logger = null)
        {
            Logger = logger;
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
            ServerOptions.Port = port;
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

                        _connectAction(new EasySocket(Logger, socketId, socket, ServerOptions));
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

            IPAddress addr = IPAddress.Parse(ServerOptions.Host);
            TcpListener tcpListener = new TcpListener(addr, ServerOptions.Port);

            tcpListener.Server.ReceiveBufferSize = ServerOptions.ReceiveBufferSize;
            tcpListener.Server.SendBufferSize = ServerOptions.SendBufferSize;
            tcpListener.Server.NoDelay = ServerOptions.NoDelay;
            tcpListener.Server.LingerState = ServerOptions.Linger;

            tcpListener.Start(ServerOptions.ListenBackLog);

            return tcpListener;
        }
    }
}
