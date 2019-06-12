using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EasySocket.Core.Networks.Support
{
    public class AsyncSendState
    {
        public Socket AsyncSocket { get; set; }
        public Action<int> SendLength { get; set; }
        public byte[] SendBuffer { get; set; }
    }
}
