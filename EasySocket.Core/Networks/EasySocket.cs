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
        private readonly string _socketId;

        private Timer _readTimeoutTImer;
        private Timer _idleTimeoutTimer;

        private Action<string> _readTimeoutAction;
        private Action<string> _idleTimeoutAction;
        private Action<string> _closedAction;
        private Action<Exception> _exceptionAction;

        public ILogger Logger { get; private set; }
        public Socket Socket { get; private set; }
        public SocketOptions SocketOptions { get; private set; }
        public Dictionary<object, object> Items { get; private set; } = new Dictionary<object, object>();

        public EasySocket(string socketId, Socket socket, ILogger logger, ServerOptions serverOptions)
        {
            this._socketId = socketId;
            Initialize(socket, logger, serverOptions);
        }

        public EasySocket(string socketId, Socket socket, ILogger logger, ClientOptions clientOptions)
        {
            this._socketId = socketId;
            Initialize(socket, logger, clientOptions);
        }


        public void Close()
        {
            if (Socket.Connected)
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
                Logger?.LogInformation("[{0}] Closed", _socketId);
            }
            if (_idleTimeoutTimer != null)
            {
                _idleTimeoutTimer.Dispose();
                _idleTimeoutTimer = null;
                Logger?.LogDebug("[{0}] Release idleTimeout", _socketId);
            }
            if (_readTimeoutTImer != null)
            {
                _readTimeoutTImer.Dispose();
                _readTimeoutTImer = null;
                Logger?.LogDebug("[{0}] Release readTimeout", _socketId);
            }
        }

        public void ReadTimeoutHandler(Action<string> action)
        {
            _readTimeoutAction = action;
            Logger?.LogDebug("[{0}] Add ReadTimeoutHandler", _socketId);
        }

        public void IdleTimeoutHandler(Action<string> action)
        {
            _idleTimeoutAction = action;
            Logger?.LogDebug("[{0}] Add IdleTimeoutHandler", _socketId);
        }

        public void CloseHandler(Action<string> action)
        {
            _closedAction = action;
            Logger?.LogDebug("[{0}] Add CloseHandler", _socketId);
        }

        public void ExceptionHandler(Action<Exception> action)
        {
            _exceptionAction = action;
            Logger?.LogDebug("[{0}] Add ExceptionHandler", _socketId);
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
                    _closedAction?.Invoke(_socketId);
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
                _exceptionAction?.Invoke(exception);

                if (!Socket.Connected)
                {
                    _closedAction?.Invoke(_socketId);
                }
            }
        }

        private void Initialize(Socket socket, ILogger logger, SocketOptions socketOptions)
        {
            Socket = socket;
            Logger = logger;
            SocketOptions = socketOptions;

            if (SocketOptions.IdleTimeout > 0)
            {
                Logger?.LogDebug("[{0}] Apply IdleTimeout - {1}", _socketId, SocketOptions.IdleTimeout);

                _idleTimeoutTimer = new Timer(timeout =>
                {
                    _idleTimeoutAction?.Invoke(_socketId);
                });
                _idleTimeoutTimer.Change(SocketOptions.IdleTimeout, Timeout.Infinite);
            }

            if (SocketOptions.ReadTimeout > 0)
            {
                Logger?.LogDebug("[{0}] Apply readTimeout - {1}", _socketId, SocketOptions.ReadTimeout);

                _readTimeoutTImer = new Timer(timeout =>
                {
                    _readTimeoutAction?.Invoke(_socketId);
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
                    _closedAction?.Invoke(_socketId);
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

                Logger?.LogInformation("[{0}] Receive - size:{1}", _socketId, receivedSize);

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
                        _closedAction?.Invoke(_socketId);
                    }
                }
            }
            catch (Exception excepton)
            {
                _exceptionAction?.Invoke(excepton);

                if (!Socket.Connected)
                {
                    _closedAction?.Invoke(_socketId);
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
                Logger?.LogInformation("[{0}] Send - size:{1}", _socketId, sendSize);

                state.SendLength(sendSize);
            }
            catch (Exception exception)
            {
                _exceptionAction?.Invoke(exception);

                if (!Socket.Connected)
                {
                    _closedAction?.Invoke(_socketId);
                }
            }
        }

    }
}
