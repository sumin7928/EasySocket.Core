using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocket.Core.Networks.Base.Token
{
    class ReceiveToken
    {
        public int BufferLength { get; set; }
        public Action<byte[]> ReceiveHandler { get; set; }
    }
}
