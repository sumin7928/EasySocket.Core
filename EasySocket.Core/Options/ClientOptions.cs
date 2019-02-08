using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocket.Core.Options
{
    public class ClientOptions
    {
        private static readonly string defaultHost = "127.0.0.1";
        private static readonly int defaultPort = 12000;
        private static readonly int defaultReadTimeout = 0;
        private static readonly int defaultReceiveBufferSize = 2048;
        private static readonly int defaultSendBufferSize = 2048;
        private static readonly bool defaultNoDelay = true;

        public string Host { set; get; }
        public int Port { set; get; }
        public int ReadTimeout { set; get; }
        public int ReceiveBufferSize { set; get; }
        public int SendBufferSize { set; get; }
        public bool NoDelay { set; get; }

        public ClientOptions()
        {
            Host = defaultHost;
            Port = defaultPort;
            ReadTimeout = defaultReadTimeout;
            ReceiveBufferSize = defaultReceiveBufferSize;
            SendBufferSize = defaultSendBufferSize;
            NoDelay = defaultNoDelay;
        }
    }
}
