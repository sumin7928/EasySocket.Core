using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EasySocket.Core.AsyncState
{
    public class AsyncSendState
    {
        public Socket AsyncSocket { set; get; }

        public Action<int> Action { set; get; }

        public byte[] SendBuffer { set; get; }
    }
}
