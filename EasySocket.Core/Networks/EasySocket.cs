using EasySocket.Core.Networks.Support;
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

        private Timer readTimeoutTImer;
        private Timer idleTimeoutTimer;

        private Action<string> readTimeoutAction;
        private Action<string> idleTimeoutAction;
        private Action<string> closedAction;
        private Action<Exception> exceptionAction;

        public Socket Socket { get; private set; }

        public SocketOptions SocketOptions { get; private set; }

        public Dictionary<object, object> Items { get; private set; } = new Dictionary<object, object>();

        public EasySocket(string socketId, Socket socket, ServerOptions serverOptions)
        {
            this.socketId = socketId;
            Socket = socket;

            SocketOptions = serverOptions;

            if (SocketOptions.IdleTimeout > 0)
            {
                idleTimeoutTimer = new Timer( timeout =>
                {
                    idleTimeoutAction?.Invoke(socketId);
                });
                idleTimeoutTimer.Change(SocketOptions.IdleTimeout, Timeout.Infinite);
            }
        }

        public EasySocket(string socketId, Socket socket, ClientOptions clientOptions)
        {
            this.socketId = socketId;
            Socket = socket;

            SocketOptions = clientOptions;

            if (SocketOptions.ReadTimeout > 0)
            {
                readTimeoutTImer = new Timer( timeout =>
                {
                    readTimeoutAction?.Invoke(socketId);
                });
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

        public void Receive(Action<byte[]> action)
        {
            Receive(null, action);
        }

        public void Receive(TotalLengthObject totalLengthObject, Action<byte[]> receiveBuffer)
        {
            AsyncReceiveState state = new AsyncReceiveState(SocketOptions.ReceiveBufferSize)
            {
                ReceiveBuffer = receiveBuffer,
                AsyncSocket = Socket,
                TotalLengthObject = totalLengthObject
            };
            try
            {
                Socket.BeginReceive(state.ChunkBuffer, state.ChunkBufferOffset, state.ChunkSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
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


        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                AsyncReceiveState state = (AsyncReceiveState)result.AsyncState;

                Socket socket = state.AsyncSocket;
                if (!socket.Connected)
                {
                    closedAction?.Invoke(socketId);
                    return;
                }

                if (idleTimeoutTimer != null)
                {
                    idleTimeoutTimer.Change(SocketOptions.IdleTimeout, Timeout.Infinite);
                }
                if (readTimeoutTImer != null)
                {
                    readTimeoutTImer.Dispose();
                }

                int receivedSize = socket.EndReceive(result);

                // 받은 데이터가 있을 경우
                if (receivedSize > 0)
                {
                    ReceiveProcess(receivedSize, state);

                    if (socket.Connected)
                    {
                        socket.BeginReceive(state.ChunkBuffer, state.ChunkBufferOffset, state.ChunkSize - state.ChunkBufferOffset, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                    }
                }
                else
                {
                    if (socket.Available > 0)
                    {
                        socket.BeginReceive(state.ChunkBuffer, state.ChunkBufferOffset, state.ChunkSize - state.ChunkBufferOffset, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
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

        private void ReceiveProcess(int receivedSize, AsyncReceiveState state)
        {
            state.ChunkBufferOffset += receivedSize;

            // 총 데이터 사이즈 파싱 처리일 경우 
            if (state.TotalLengthObject != null)
            {
                byte[] sizeBytes = new byte[state.TotalLengthObject.TotalLengthSize - state.TotalLengthObject.TotalLengthOffset];
                Array.Copy(state.ChunkBuffer, state.TotalLengthObject.TotalLengthOffset, sizeBytes, 0, sizeBytes.Length);

                // 총길이 값의 바이트 오더링이 빅 엔디안일 경우
                if (state.TotalLengthObject.IsBigEndian)
                {
                    Array.Reverse(sizeBytes);
                }

                int totalSize = GetTotalSize(sizeBytes);

                if (state.ChunkBufferOffset >= totalSize)
                {
                    byte[] respose = new byte[totalSize];
                    Array.Copy(state.ChunkBuffer, respose, respose.Length);

                    if (state.ChunkBufferOffset > totalSize)
                    {
                        // ChunkBufferOffset 재정렬
                        int remainOffset = state.ChunkBufferOffset - totalSize;
                        byte[] temp = new byte[remainOffset];
                        Array.Copy(state.ChunkBuffer, temp, remainOffset);

                        Array.Clear(state.ChunkBuffer, 0, state.ChunkSize);
                        Array.Copy(temp, state.ChunkBuffer, remainOffset);
                        state.ChunkBufferOffset = remainOffset;

                        // 초과된 데이터 다시 체크
                        ReceiveProcess(0, state);
                    }
                    else
                    {
                        Array.Clear(state.ChunkBuffer, 0, state.ChunkSize);
                        state.ChunkBufferOffset = 0;
                    }
                    state.ReceiveBuffer(respose);

                }
            }
            // 일반 패킷 처리일 경우 ( 받은 데이터 그대로 넘겨줌 )
            else
            {
                byte[] respose = new byte[state.ChunkBufferOffset];
                Array.Copy(state.ChunkBuffer, respose, respose.Length);
                Array.Clear(state.ChunkBuffer, 0, state.ChunkSize);
                state.ChunkBufferOffset = 0;
                state.ReceiveBuffer(respose);
            }
        }

        private int GetTotalSize(byte[] sizeBytes)
        {
            if (sizeBytes.Length == sizeof(byte))
            {
                return BitConverter.ToChar(sizeBytes, 0);
            }
            else if (sizeBytes.Length == sizeof(short))
            {
                return BitConverter.ToInt16(sizeBytes, 0);
            }
            else
            {
                return BitConverter.ToInt32(sizeBytes, 0);
            }
        }

        private void SendCallBack(IAsyncResult ar)
        {
            if (readTimeoutTImer != null)
            {
                readTimeoutTImer.Change(SocketOptions.ReadTimeout, Timeout.Infinite);
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

    }
}
