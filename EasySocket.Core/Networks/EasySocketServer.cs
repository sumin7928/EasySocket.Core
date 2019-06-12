using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using EasySocket.Core.Options;
using EasySocket.Core.Utils;

namespace EasySocket.Core.Networks
{
    class EasySocketServer : IEasySocketServer
    {
        private readonly ServerOptions _options;

        private Action<IEasySocket> _connectAction;
        private Action<Exception> _exceptionAction;
        private TcpListener _tcpListener;
        private bool _acceptLoop = true;

        public EasySocketServer(ServerOptions options)
        {
            this._options = options;
        }

        public void ConnectHandler(Action<IEasySocket> action)
        {
            _connectAction = action;
        }

        public void ExceptionHandler(Action<Exception> action)
        {
            _exceptionAction = action;
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
                        _connectAction(new EasySocket(socketId, socket, _options));
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
