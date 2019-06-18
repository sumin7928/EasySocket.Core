using EasySocket.Core.Networks.Support;
using EasySocket.Core.Options;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        private Timer _readTimeoutTImer;
        private Timer _idleTimeoutTimer;

        private Action<string> _readTimeoutAction;
        private Action<string> _idleTimeoutAction;
        private Action<string> _closedAction;
        private Action<Exception> _exceptionAction;

        public Socket Socket { get; private set; }
        public string SocketId { get; private set; }
        public SocketOptions SocketOptions { get; private set; }
        public Dictionary<object, object> Items { get; private set; } = new Dictionary<object, object>();

        public EasySocket(ILogger logger, string socketId, Socket socket, ServerOptions serverOptions)
        {
            this._logger = logger;
            Initialize(socketId, socket, serverOptions);
        }

        public EasySocket(ILogger logger, string socketId, Socket socket, ClientOptions clientOptions)
        {
            this._logger = logger;
            Initialize(socketId, socket, clientOptions);
        }

        public void Close()
        {
            if (Socket.Connected)
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
                _logger?.LogInformation("[{0}] Closed", SocketId);
                _closedAction?.Invoke(SocketId);
            }
            if (_idleTimeoutTimer != null)
            {
                _idleTimeoutTimer.Dispose();
                _idleTimeoutTimer = null;
                _logger?.LogDebug("[{0}] Release idleTimeout", SocketId);
            }
            if (_readTimeoutTImer != null)
            {
                _readTimeoutTImer.Dispose();
                _readTimeoutTImer = null;
                _logger?.LogDebug("[{0}] Release readTimeout", SocketId);
            }
        }

        public void ReadTimeoutHandler(Action<string> action)
        {
            _readTimeoutAction = action;
            _logger?.LogDebug("[{0}] Add ReadTimeoutHandler", SocketId);
        }

        public void IdleTimeoutHandler(Action<string> action)
        {
            _idleTimeoutAction = action;
            _logger?.LogDebug("[{0}] Add IdleTimeoutHandler", SocketId);
        }

        public void CloseHandler(Action<string> action)
        {
            _closedAction = action;
            _logger?.LogDebug("[{0}] Add CloseHandler", SocketId);
        }

        public void ExceptionHandler(Action<Exception> action)
        {
            _exceptionAction = action;
            _logger?.LogDebug("[{0}] Add ExceptionHandler", SocketId);
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
                Socket.BeginReceive(state.ChunkBuffer, state.ChunkBufferOffset, state.ChunkBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception exception)
            {
                _exceptionAction?.Invoke(exception);

                if (!Socket.Connected)
                {
                    _closedAction?.Invoke(SocketId);
                }
            }
        }

        public void Send(byte[] sendData)
        {
            Send(sendData, 0, sendData.Length, null);
        }

        public void Send(byte[] sendData, Action<int> length)
        {
            Send(sendData, 0, sendData.Length, length);
        }

        public void Send(byte[] sendData, int offset, int size)
        {
            Send(sendData, offset, size, null);
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
                _exceptionAction?.Invoke(exception);

                if (!Socket.Connected)
                {
                    _closedAction?.Invoke(SocketId);
                }
            }
        }

        private void Initialize(string socketId, Socket socket, SocketOptions socketOptions)
        {
            Socket = socket;
            SocketId = socketId;
            SocketOptions = socketOptions;

            if (SocketOptions.IdleTimeout > 0)
            {
                _logger?.LogDebug("[{0}] Apply IdleTimeout - {1}", SocketId, SocketOptions.IdleTimeout);

                _idleTimeoutTimer = new Timer(timeout =>
                {
                    _idleTimeoutAction?.Invoke(SocketId);
                });
                _idleTimeoutTimer.Change(SocketOptions.IdleTimeout, Timeout.Infinite);
            }

            if (SocketOptions.ReadTimeout > 0)
            {
                _logger?.LogDebug("[{0}] Apply readTimeout - {1}", SocketId, SocketOptions.ReadTimeout);

                _readTimeoutTImer = new Timer(timeout =>
                {
                    _readTimeoutAction?.Invoke(SocketId);
                });
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
                    _closedAction?.Invoke(SocketId);
                    return;
                }

                if (_idleTimeoutTimer != null)
                {
                    _idleTimeoutTimer.Change(SocketOptions.IdleTimeout, Timeout.Infinite);
                }
                if (_readTimeoutTImer != null)
                {
                    _readTimeoutTImer.Dispose();
                }

                int receivedSize = socket.EndReceive(result);

                _logger?.LogInformation("[{0}] Receive - size:{1}", SocketId, receivedSize);

                // 받은 데이터가 있을 경우
                if (receivedSize > 0)
                {
                    ReceiveProcess(receivedSize, state);

                    if (socket.Connected)
                    {
                        socket.BeginReceive(state.ChunkBuffer, state.ChunkBufferOffset, state.ChunkBuffer.Length - state.ChunkBufferOffset, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                    }
                }
                else
                {
                    if (socket.Available > 0)
                    {
                        socket.BeginReceive(state.ChunkBuffer, state.ChunkBufferOffset, state.ChunkBuffer.Length - state.ChunkBufferOffset, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                    }
                    else
                    {
                        _closedAction?.Invoke(SocketId);
                    }
                }
            }
            catch (Exception excepton)
            {
                _exceptionAction?.Invoke(excepton);

                if (!Socket.Connected)
                {
                    _closedAction?.Invoke(SocketId);
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

                        Array.Clear(state.ChunkBuffer, 0, state.ChunkBuffer.Length);
                        Array.Copy(temp, state.ChunkBuffer, remainOffset);
                        state.ChunkBufferOffset = remainOffset;

                        // 초과된 데이터 다시 체크
                        ReceiveProcess(0, state);
                    }
                    else
                    {
                        Array.Clear(state.ChunkBuffer, 0, state.ChunkBuffer.Length);
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
                Array.Clear(state.ChunkBuffer, 0, state.ChunkBuffer.Length);
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
            if (_readTimeoutTImer != null)
            {
                _readTimeoutTImer.Change(SocketOptions.ReadTimeout, Timeout.Infinite);
            }

            try
            {
                AsyncSendState state = (AsyncSendState)ar.AsyncState;

                int sendSize = state.AsyncSocket.EndSend(ar);
                _logger?.LogInformation("[{0}] Send - size:{1}", SocketId, sendSize);

                state.SendLength?.Invoke(sendSize);
            }
            catch (Exception exception)
            {
                _exceptionAction?.Invoke(exception);

                if (!Socket.Connected)
                {
                    _closedAction?.Invoke(SocketId);
                }
            }
        }

    }
}
