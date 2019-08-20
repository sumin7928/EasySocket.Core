using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        protected readonly Timer _idleTimer;

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

            if (SocketConfiguration.IdleTimeout > 0)
            {
                _idleTimer = new Timer(id =>
                {
                    _idleTimeoutAction?.Invoke();
                    Socket.Shutdown(SocketShutdown.Both);
                });
            }
        }

        public void IdleTimeoutHandler(Action action)
        {
            _idleTimeoutAction = action;
            _logger?.LogDebug("[{0}] Add IdleTimeoutHandler", SocketId);
        }

        public void ReadTimeoutHandler(Action action)
        {
            _readTimeoutAction = action;
            _logger?.LogDebug("[{0}] Add CloseHandler", SocketId);
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
            _idleTimer?.Change(SocketConfiguration.IdleTimeout, Timeout.Infinite);

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
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                ReceiveToken token = (ReceiveToken)args.UserToken;
                token.BufferLength += args.BytesTransferred;

                if (Socket.Available == 0)
                {
                    byte[] receiveBuffer = new byte[token.BufferLength];
                    Array.Copy(args.Buffer, receiveBuffer, token.BufferLength);
                    token.BufferLength = 0;

                    Task.Run(() =>
                    {
                        token.ReceiveHandler(receiveBuffer);
                    });

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
