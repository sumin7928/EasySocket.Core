using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocket.Core.Options
{
    public class SocketOptions
    {
        public int ReadTimeout { set; get; }
        public int IdleTimeout { set; get; }
        public int ReceiveBufferSize { set; get; }
        public int SendBufferSize { set; get; }
    }
}
