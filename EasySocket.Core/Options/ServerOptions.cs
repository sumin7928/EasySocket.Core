using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EasySocket.Core.Options
{
    public class ServerOptions : SocketOptions
    {
        private static readonly string defaultHost = "127.0.0.1";
        private static readonly int defaultPort = 12000;
        private static readonly int defaultListenBackLog = 128;
        private static readonly bool defaultNoDelay = true;
        private static readonly LingerOption defaultLingerOption = new LingerOption( true, 2 );

        // socket options
        private static readonly int defaultReadTimeout = 0;
        private static readonly int defaultIdleTimeout = 15 * 60 * 1000; // 15분
        private static readonly int defaultReceiveBufferSize = 2048;
        private static readonly int defaultSendBufferSize = 2048;

        public string Host { set; get; }
        public int Port { set; get; }
        public int ListenBackLog { set; get; }
        public bool NoDelay { set; get; }
        public LingerOption Linger { set; get; }

        public ServerOptions()
        {
            Host = defaultHost;
            Port = defaultPort;
            ListenBackLog = defaultListenBackLog;
            NoDelay = defaultNoDelay;
            Linger = defaultLingerOption;

            ReadTimeout = defaultReadTimeout;
            IdleTimeout = defaultIdleTimeout;
            ReceiveBufferSize = defaultReceiveBufferSize;
            SendBufferSize = defaultSendBufferSize;
        }
    }
}
