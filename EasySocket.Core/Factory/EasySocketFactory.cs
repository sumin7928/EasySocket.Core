using EasySocket.Core.Networks;
using EasySocket.Core.Options;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EasySocket.Core.Factory
{
    public class EasySocketFactory
    {
        /// <summary>
        /// create server with default options.
        /// </summary>
        public static IEasySocketServer CreateServer()
        {
            return new EasySocketServer(new ServerOptions());
        }

        /// <summary>
        /// create server with custom options.
        /// </summary>
        /// <param name="options"> TcpServerOptions </param>
        public static IEasySocketServer CreateServer(ServerOptions options)
        {
            return new EasySocketServer(options);
        }

        /// <summary>
        /// create client with default options.
        /// </summary>
        public static IEasySocketClient CreateClient()
        {
            return new EasySocketClient(new ClientOptions());
        }

        /// <summary>
        /// create client with custom options.
        /// </summary>
        /// <param name="options"> TcpClientOptions </param>
        public static IEasySocketClient CreateClient(ClientOptions options)
        {
            return new EasySocketClient(options);
        }
    }
}
