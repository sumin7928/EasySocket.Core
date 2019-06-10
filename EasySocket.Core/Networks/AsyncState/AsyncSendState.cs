using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EasySocket.Core.Networks.AsyncState
{
    public class AsyncSendState
    {
        public Socket AsyncSocket { set; get; }

        public Action<int> SendLength { set; get; }

        public byte[] SendBuffer { set; get; }
    }
}
