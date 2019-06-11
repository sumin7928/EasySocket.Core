using EasySocket.Core.Options;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EasySocket.Core.Networks
{
    class EasySocketClient : IEasySocketClient
    {
        private readonly ClientOptions options;

        private Action<IEasySocket> connectAction;
        private Action<Exception> exceptionAction;

        public EasySocketClient(ClientOptions options)
        {
            this.options = options;
        }

        public void Connect()
        {
            Connect(options.Host, options.Port);
        }

        public void Connect(string host, int port)
        {
            if( connectAction == null )
            {
                throw new InvalidOperationException("Not found connect action logic");
            }

            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.BeginConnect(host, port, new AsyncCallback(ConnectCallback), socket);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            socket.EndConnect(ar);

            var socketId = Guid.NewGuid().ToString();

            var tcpSocket = new EasySocket(socketId, socket, options );

            connectAction(tcpSocket);

        }

        public void ConnectHandler(Action<IEasySocket> action)
        {
            connectAction = action;
        }

        public void ExceptionHandler(Action<Exception> action)
        {
            exceptionAction = action;
        }



    }
}
