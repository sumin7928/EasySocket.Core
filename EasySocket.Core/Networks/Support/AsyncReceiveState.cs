using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EasySocket.Core.Networks.Support
{
    class AsyncReceiveState
    {
        public int ChunkSize { get; private set; }
        public int ChunkBufferOffset { get; set; }
        public byte[] ChunkBuffer { get; set; }
        public Action<byte[]> ReceiveBuffer { get; set; }
        public Socket AsyncSocket { get; set; }
        public TotalLengthObject TotalLengthObject { get; set; }

        public AsyncReceiveState(int chunkSize)
        {
            ChunkSize = chunkSize;
            ChunkBuffer = new byte[ChunkSize];
        }
    }
}