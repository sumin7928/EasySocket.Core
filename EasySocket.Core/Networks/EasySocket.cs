using EasySocket.Core.Networks.AsyncState;
using EasySocket.Core.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EasySocket.Core.Networks
{
    class EasySocket : IEasySocket
    {
        private readonly string socketId;

        private Dictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

        private Timer readTimeoutTImer;
        private Timer idleTimeoutTimer;

        private Action<string> readTimeoutAction;
        private Action<string> idleTimeoutAction;
        private Action<string> closedAction;
        private Action<Exception> exceptionAction;

        public Socket Socket { get; }

        public int ReadTimeout { get; set; }

        public int IdleTimeout { get; set; }

        public EasySocket(string socketId, Socket socket, ServerOptions serverOptions)
        {
            this.socketId = socketId;
            Socket = socket;

            IdleTimeout = serverOptions.IdleTimeout;

            if (serverOptions.IdleTimeout > 0)
            {
                idleTimeoutTimer = new Timer((timeout) =>
               {
                   idleTimeoutAction?.Invoke(socketId);
               });
                idleTimeoutTimer.Change(serverOptions.IdleTimeout, Timeout.Infinite);
            }
        }

        public EasySocket(string socketId, Socket socket, ClientOptions clientOptions)
        {
            this.socketId = socketId;
            Socket = socket;
            ReadTimeout = clientOptions.ReadTimeout;

            if (clientOptions.ReadTimeout > 0)
            {
                readTimeoutTImer = new Timer((timeout) =>
                {
                    readTimeoutAction?.Invoke(socketId);
                });
            }
        }

        public void Receive(Action<byte[]> action)
        {
            Receive(0, 0, action);
        }

        public void Receive(int totalLengthOffet, int totalLengthSize, Action<byte[]> receiveBuffer)
        {
            AsyncReceiveState state = new AsyncReceiveState
            {
                ReceiveBuffer = receiveBuffer,
                Offset = totalLengthOffet,
                Length = totalLengthSize,
                AsyncSocket = Socket
            };

            try
            {
                Socket.BeginReceive(state.ChunkBuffer, state.ChunkBufferOffset, AsyncReceiveState.ChunkSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception exception)
            {
                exceptionAction?.Invoke(exception);

                if (!Socket.Connected)
                {
                    closedAction?.Invoke(socketId);
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                AsyncReceiveState state = (AsyncReceiveState)ar.AsyncState;

                Socket socket = state.AsyncSocket;
                if (!socket.Connected)
                {
                    closedAction?.Invoke(socketId);
                    return;
                }

                if (idleTimeoutTimer != null)
                {
                    idleTimeoutTimer.Change(IdleTimeout, Timeout.Infinite);
                }
                if (readTimeoutTImer != null)
                {
                    readTimeoutTImer.Dispose();
                }

                int receivedSize = socket.EndReceive(ar);

                if (receivedSize > 0)
                {
                    state.DoAction(receivedSize);

                    if (socket.Connected)
                    {
                        socket.BeginReceive(state.ChunkBuffer, state.ChunkBufferOffset, AsyncReceiveState.ChunkSize - state.ChunkBufferOffset, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                    }
                }
                else
                {
                    if (socket.Available > 0)
                    {
                        socket.BeginReceive(state.ChunkBuffer, state.ChunkBufferOffset, AsyncReceiveState.ChunkSize - state.ChunkBufferOffset, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                    }
                    else
                    {
                        closedAction?.Invoke(socketId);
                    }
                }
            }
            catch (Exception excepton)
            {
                exceptionAction?.Invoke(excepton);

                if (!Socket.Connected)
                {
                    closedAction?.Invoke(socketId);
                }
            }
        }

        public void Send(byte[] sendData, Action<int> length)
        {
            Send(sendData, 0, sendData.Length, length);
        }

        public void Send(byte[] sendData, int offset, int size, Action<int> length)
        {
            AsyncSendState state = new AsyncSendState
            {
                AsyncSocket = Socket,
                SendLength = length,
                SendBuffer = sendData
            };

            try
            {
                Socket.BeginSend(sendData, offset, size, SocketFlags.None, new AsyncCallback(SendCallBack), state);
            }
            catch (Exception exception)
            {
                exceptionAction?.Invoke(exception);

                if (!Socket.Connected)
                {
                    closedAction?.Invoke(socketId);
                }
            }
        }

        private void SendCallBack(IAsyncResult ar)
        {
            if (readTimeoutTImer != null)
            {
                readTimeoutTImer.Change(ReadTimeout, Timeout.Infinite);
            }

            try
            {
                AsyncSendState state = (AsyncSendState)ar.AsyncState;

                int sendSize = state.AsyncSocket.EndSend(ar);

                state.SendLength(sendSize);
            }
            catch (Exception exception)
            {
                exceptionAction?.Invoke(exception);

                if (!Socket.Connected)
                {
                    closedAction?.Invoke(socketId);
                }
            }
        }

        public void Close()
        {
            if (Socket.Connected)
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
            }
        }

        public void ReadTimeoutHandler(Action<string> action)
        {
            readTimeoutAction = action;
        }

        public void IdleTimeoutHandler(Action<string> action)
        {
            idleTimeoutAction = action;
        }

        public void CloseHandler(Action<string> action)
        {
            closedAction = action;
        }

        public void ExceptionHandler(Action<Exception> action)
        {
            exceptionAction = action;
        }

        public string ToRemoteLog()
        {
            return $"id:{socketId} [{Socket.LocalEndPoint}]->[{Socket.RemoteEndPoint}]";
        }

        public string ToLocalLog()
        {
            return $"id:{socketId} [{Socket.RemoteEndPoint}]->[{Socket.LocalEndPoint}]";
        }

    }
}
