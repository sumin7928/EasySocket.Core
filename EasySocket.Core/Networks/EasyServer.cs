using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using EasySocket.Core.Options;

namespace EasySocket.Core.Networks
{
    class EasyServer : IEasyServer
    {
        private readonly ServerOptions options;

        private Action<IEasySocket> connectAction;
        private Action<Exception> exceptionAction;
        private TcpListener tcpListener;


        public EasyServer(ServerOptions options)
        {
            this.options = options;
        }

        public void ConnectHandler( Action<IEasySocket> action )
        {
            connectAction = action;
        }

        public void ExceptionHandler( Action<Exception> action )
        {
            exceptionAction = action;
        }

        public void Listen()
        {
            Listen(options.Host, options.Port);
        }

        public void Listen(int port)
        {
            Listen(options.Host, port);
        }

        public void Listen(string address, int port)
        {
            if(connectAction == null)
            {
                throw new InvalidOperationException("Not found connect action logic");
            }

            IPAddress addr = IPAddress.Parse(address);
            tcpListener = new TcpListener(addr, port);
            Socket serverSocket = tcpListener.Server;

            serverSocket.ReceiveBufferSize = options.ReceiveBufferSize;
            serverSocket.SendBufferSize = options.SendBufferSize;
            serverSocket.NoDelay = options.NoDelay;
            serverSocket.LingerState = options.Linger;

            try
            {
                tcpListener.Start(options.ListenBackLog);
            }
            catch( Exception e )
            {
                exceptionAction?.Invoke(e);
            }
        }


        public void Stop()
        {
            try
            {
                tcpListener.Stop();
            }
            catch ( Exception e )
            {
                exceptionAction?.Invoke( e );
            }
        }

        public void Start()
        {
            try
            {
                Socket socket = null;
                while ( ( socket = tcpListener.AcceptSocket() ) != null && socket.Connected )
                {
                    string socketId = Guid.NewGuid().ToString();
                    connectAction( new EasySocket( socketId, socket, options ) );
                }
            }
            catch ( Exception e )
            {
                exceptionAction?.Invoke( e );
            }
        }
    }
}
