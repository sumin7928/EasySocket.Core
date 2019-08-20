using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using EasySocket.Core.Networks.Base.Configuration;

namespace EasySocket.Core.Networks.Server
{
    public interface IEasyServerSocket
    {
        string SocketId { get; set; }
        Socket Socket { get; set; }
        Dictionary<object, object> Items { get; }
        SocketConfiguration SocketConfiguration { get; }

        void IdleTimeoutHandler(Action action);
        void CloseHandler(Action action);
        void ExceptionHandler(Action<Exception> action);
        void Close();
        void Receive(Action<byte[]> action);
        void Send(byte[] sendData);
        void Send(byte[] sendData, Action<int> length);
        void Send(byte[] sendData, int offset, int count);
        void Send(byte[] sendData, int offset, int count, Action<int> length);
    }
}
