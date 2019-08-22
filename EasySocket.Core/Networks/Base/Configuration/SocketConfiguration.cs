using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocket.Core.Networks.Base.Configuration
{
    public class SocketConfiguration
    {
        // common
        public bool NoDelay { get; set; } = true;
        public int ReceiveBufferSize { get; set; } = 2048;
        public int SendBufferSize { get; set; } = 2048;

        // for server
        public int IdleTimeout { get; set; } = 0;

        // for client
        public int ReadTimeout { get; set; } = 0;
    }
}
