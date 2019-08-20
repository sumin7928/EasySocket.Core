using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EasySocket.Core.Networks.Base;
using EasySocket.Core.Networks.Base.Configuration;
using EasySocket.Core.Networks.Server.Configuration;
using EasySocket.Core.Networks.Server.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasySocket.Core.Networks.Server
{
    class EasyServerSocket : BaseSocket, IEasyServerSocket
    {
        private readonly EasyServerSocketPool _socketPool;
        private readonly EasyServerAccessController _accessController;

        public EasyServerSocket(ILogger logger, SocketConfiguration socketConfig, EasyServerSocketPool socketPool, EasyServerAccessController accessController)
            : base(logger, socketConfig)
        {
            this._socketPool = socketPool;
            this._accessController = accessController;
        }

        public override void Close()
        {
            try
            {
                Socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                _logger?.LogInformation("[{0}] Exception - Failed Shutdown method", SocketId);
            }

            SocketId = null;
            Socket.Close();
            Socket.Dispose();
            ReceiveSocketAsyncEventArgs.UserToken = null;
            Items.Clear();

            _socketPool.Push(this);
            _accessController.Release();

            _closedAction?.Invoke();
            Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", _socketPool.GetCount());
        }
    }
}
