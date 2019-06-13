using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocket.Core.Options
{
    public class ClientOptions : SocketOptions
    {
        private static readonly string defaultHost = "127.0.0.1";
        private static readonly int defaultPort = 12000;
        private static readonly bool defaultNoDelay = true;

        // socket options
        private static readonly int defaultReadTimeout = 5 * 60 * 1000; // 5분
        private static readonly int defaultIdleTimeout = 0;
        private static readonly int defaultReceiveBufferSize = 2048;
        private static readonly int defaultSendBufferSize = 2048;

        public string Host { set; get; }
        public int Port { set; get; }
        public bool NoDelay { set; get; }

        public ClientOptions()
        {
            Host = defaultHost;
            Port = defaultPort;
            NoDelay = defaultNoDelay;

            ReadTimeout = defaultReadTimeout;
            IdleTimeout = defaultIdleTimeout;
            ReceiveBufferSize = defaultReceiveBufferSize;
            SendBufferSize = defaultSendBufferSize;

        }
    }
}
