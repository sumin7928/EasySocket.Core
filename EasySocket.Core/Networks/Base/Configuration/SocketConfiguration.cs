using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocket.Core.Networks.Base.Configuration
{
    public class SocketConfiguration
    {
        public int ReceiveBufferSize { get; set; } = 2048;
        public int SendBufferSize { get; set; } = 2048;
        public int IdleTimeout { get; set; } = 0;
        public int ReadTimeout { get; set; } = 0;
    }
}
