using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Timers;
using EasySocket.Core.Networks.Base.Configuration;
using EasySocket.Core.Networks.Base.Token;
using EasySocket.Core.Networks.Server.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasySocket.Core.Networks.Base
{
    abstract class BaseSocket
    {
        protected readonly ILogger _logger;

        protected Timer _idleTimeoutTimer;
        protected Timer _readTimeoutTimer;

        protected Action _readTimeoutAction;
        protected Action _idleTimeoutAction;
        protected Action _closedAction;
        protected Action<Exception> _exceptionAction;

        public string SocketId { get; set; }
        public Socket Socket { get; set; }
        public SocketAsyncEventArgs ReceiveSocketAsyncEventArgs { get; set; } = new SocketAsyncEventArgs();
        public Dictionary<object, object> Items { get; private set; } = new Dictionary<object, object>();
        public SocketConfiguration SocketConfiguration { get; set; }

        public BaseSocket(ILogger logger, SocketConfiguration socketConfig)
        {
            _logger = logger;
            SocketConfiguration = socketConfig;

            ReceiveSocketAsyncEventArgs.SetBuffer(new byte[SocketConfiguration.ReceiveBufferSize], 0, SocketConfiguration.ReceiveBufferSize);
            ReceiveSocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(CompleteReadWriteEvent);
        }


        public void IdleTimeoutHandler(Action action)
        {
            if (SocketConfiguration.IdleTimeout > 0)
            {
                _idleTimeoutTimer = new Timer()
                {
                    Interval = SocketConfiguration.IdleTimeout,
                    AutoReset = false,
                    Enabled = true
                };
                _idleTimeoutTimer.Elapsed += OnIdleTimerEvent;
            }

            _idleTimeoutAction = action;
            _logger?.LogDebug("[{0}] Add IdleTimeoutHandler", SocketId);
        }

        public void ReadTimeoutHandler(Action action)
        {
            if (SocketConfiguration.ReadTimeout > 0)
            {
                _readTimeoutTimer = new Timer()
                {
                    Interval = SocketConfiguration.ReadTimeout,
                    AutoReset = false,
                    Enabled = true
                };
                _readTimeoutTimer.Elapsed += OnReadTimerEvent;
            }

            _readTimeoutAction = action;
            _logger?.LogDebug("[{0}] Add CloseHandler", SocketId);
        }

        private void OnIdleTimerEvent(Object source, ElapsedEventArgs e)
        {
            _idleTimeoutAction?.Invoke();

            if (Socket != null && Socket.Connected)
            {
                Close();
            }
        }

        private void OnReadTimerEvent(Object source, ElapsedEventArgs e)
        {
            _readTimeoutAction?.Invoke();

            if (Socket != null && Socket.Connected)
            {
                Close();
            }
        }

        public void CloseHandler(Action action)
        {
            _closedAction = action;
            _logger?.LogDebug("[{0}] Add CloseHandler", SocketId);
        }

        public void ExceptionHandler(Action<Exception> action)
        {
            _exceptionAction = action;
            _logger?.LogDebug("[{0}] Add ExceptionHandler", SocketId);
        }

        public abstract void Close();

        public void Receive(Action<byte[]> buffer)
        {
            ReceiveToken token = new ReceiveToken
            {
                ReceiveHandler = buffer
            };
            ReceiveSocketAsyncEventArgs.UserToken = token;

            StartReceive(ReceiveSocketAsyncEventArgs);
        }

        private void StartReceive(SocketAsyncEventArgs args)
        {
            try
            {
                _idleTimeoutTimer?.Start();

                if (Socket.ReceiveAsync(args) == false)
                {
                    ProcessReceive(args);
                }
            }
            catch (Exception exception)
            {
                _exceptionAction?.Invoke(exception);

                if (Socket.Connected)
                {
                    Close();
                }
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs args)
        {
            _idleTimeoutTimer?.Stop();
            _readTimeoutTimer?.Stop();

            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                ReceiveToken token = (ReceiveToken)args.UserToken;
                token.BufferLength += args.BytesTransferred;

                if (Socket.Available == 0)
                {
                    byte[] receiveBuffer = new byte[token.BufferLength];
                    Array.Copy(args.Buffer, receiveBuffer, token.BufferLength);
                    token.BufferLength = 0;
                    token.ReceiveHandler(receiveBuffer);

                    StartReceive(args);
                }
                else if (Socket.ReceiveAsync(args) == false)
                {
                    // Read the next block of data sent by client.
                    ProcessReceive(args);
                }
            }
            else
            {
                if (Socket.Connected)
                {
                    Close();
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

        public void Send(byte[] sendData, int offset, int count)
        {
            Send(sendData, offset, count, null);
        }

        public void Send(byte[] sendData, int offset, int count, Action<int> length)
        {
            SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(CompleteReadWriteEvent);
            sendArgs.SetBuffer(sendData, offset, count);

            if (length != null)
            {
                SendToken token = new SendToken
                {
                    SendHandler = length
                };
                sendArgs.UserToken = token;
            }

            StartSend(sendArgs);
        }

        private void StartSend(SocketAsyncEventArgs args)
        {
            try
            {
                _readTimeoutTimer?.Start();

                if (Socket.SendAsync(args) == false)
                {
                    ProcessSend(args);
                }
            }
            catch (Exception exception)
            {
                _exceptionAction?.Invoke(exception);

                if (!Socket.Connected)
                {
                    _closedAction?.Invoke();
                }
            }
        }

        private void ProcessSend(SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                if (args.UserToken != null)
                {
                    SendToken token = (SendToken)args.UserToken;
                    token.SendHandler?.Invoke(args.BytesTransferred);
                }
            }
            else
            {
                if (Socket.Connected)
                {
                    Close();
                }
            }
        }

        private void CompleteReadWriteEvent(object sender, SocketAsyncEventArgs args)
        {
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(args);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(args);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }
    }
}
