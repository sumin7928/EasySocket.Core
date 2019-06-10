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

        public async Task Listen()
        {
            if( connectAction == null )
            {
                throw new InvalidOperationException( "Not found connect action logic" );
            }

            IPAddress addr = IPAddress.Parse( options.Host );
            TcpListener tcpListener = new TcpListener( addr, options.Port );
            Socket serverSocket = tcpListener.Server;

            serverSocket.ReceiveBufferSize = options.ReceiveBufferSize;
            serverSocket.SendBufferSize = options.SendBufferSize;
            serverSocket.NoDelay = options.NoDelay;
            serverSocket.LingerState = options.Linger;

            try
            {
                tcpListener.Start( options.ListenBackLog );

                while(true)
                {
                    Socket socket = await tcpListener.AcceptSocketAsync();
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
