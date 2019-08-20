using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using EasySocket.Core.Networks.Base;
using EasySocket.Core.Networks.Base.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasySocket.Core.Networks.Client
{
    class EasyClientSocket : BaseSocket, IEasyClientSocket
    {
        public EasyClientSocket(ILogger logger, SocketConfiguration socketConfig)
            :base(logger, socketConfig)
        {
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

            _closedAction?.Invoke();
        }
    }
}
