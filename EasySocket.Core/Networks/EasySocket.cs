using EasySocket.Core.AsyncState;
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

        private Dictionary<object,object> header = new Dictionary<object, object>();

        private Timer readTimeoutTImer;
        private Timer idleTimeoutTimer;

        private Action<string> readTimeoutAction;
        private Action<string> idleTimeoutAction;
        private Action<string> closedAction;
        private Action<Exception> exceptionAction;

        public Socket Socket { get; }

        public int ReadTimeout { get; set; }

        public int IdleTimeout { get; set; }

        public EasySocket( string socketId, Socket socket, ServerOptions serverOptions )
        {
            this.socketId = socketId;
            Socket = socket;

            IdleTimeout = serverOptions.IdleTimeout;

            if ( serverOptions.IdleTimeout > 0 )
            {
                idleTimeoutTimer = new Timer( ( timeout ) =>
                {
                    idleTimeoutAction?.Invoke( socketId );
                } );
                idleTimeoutTimer.Change( serverOptions.IdleTimeout, Timeout.Infinite );
            }
        }

        public EasySocket(string socketId, Socket socket, ClientOptions clientOptions )
        {
            this.socketId = socketId;
            Socket = socket;
            ReadTimeout = clientOptions.ReadTimeout;

            if( clientOptions.ReadTimeout > 0)
            {
                readTimeoutTImer = new Timer((timeout) =>
                {
                    readTimeoutAction?.Invoke(socketId);
                });
            }
        }

        public void Put(object key, object value)
        {
            header[key] = value;
        }

        public object Get(object key)
        {
            header.TryGetValue(key, out object value);
            return value;
        }

        public object Get(object key, object deafultValue)
        {
            if(!header.TryGetValue(key, out object value))
            {
                return deafultValue;
            }
            return value;
        }

        public void Receive(Action<byte[]> action)
        {
            Receive(0, 0, action);
        }

        public void Receive(int totalLengthOffet, int totalLengthSize, Action<byte[]> action)
        {
            AsyncReceiveState state = new AsyncReceiveState
            {
                Action = action,
                TotalLengthOffset = totalLengthOffet,
                TotalLengthSize = totalLengthSize,
                AsyncSocket = Socket
            };

            try
            {
                Socket.BeginReceive(state.ChunkBuffer, state.ChunkBufferOffset, AsyncReceiveState.ChunkSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception exception)
            {
                exceptionAction?.Invoke(exception);

                if(!Socket.Connected)
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
                if(!socket.Connected)
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

        public void Send(byte[] sendData, int length, Action<int> action)
        {
            AsyncSendState state = new AsyncSendState
            {
                Action = action,
                AsyncSocket = Socket,
                SendBuffer = sendData
            };

            try
            {
                Socket.BeginSend(sendData, 0, length, SocketFlags.None, new AsyncCallback(SendCallBack), state);
            }
            catch ( Exception exception)
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

                state.Action(sendSize);
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
            if(Socket.Connected)
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
            return String.Format("id:{0} [{1}]->[{2}]", socketId, Socket.LocalEndPoint, Socket.RemoteEndPoint);
        }

        public string ToLocalLog()
        {
            return String.Format("id:{0} [{1}]->[{2}]", socketId, Socket.RemoteEndPoint, Socket.LocalEndPoint);
        }

    }
}
