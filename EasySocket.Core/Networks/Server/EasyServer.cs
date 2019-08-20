using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasySocket.Core.Networks.Base.Configuration;
using EasySocket.Core.Networks.Server;
using EasySocket.Core.Networks.Server.Configuration;
using EasySocket.Core.Networks.Server.Helper;
using EasySocket.Core.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasySocket.Core.Networks
{
    public class EasyServer
    {
        private readonly ILogger<EasyServer> _logger;
        private readonly IConfiguration _config;

        private Action<IEasyServerSocket> _connectAction;
        private Action<Exception> _exceptionAction;
        private Socket _listenSocket;
        private EasyServerSocketPool _socketPool;
        private EasyServerAccessController _accessController;
        private ManualResetEvent _exitEvent = new ManualResetEvent(false);

        public EasyServerConfiguration EasyServerConfiguration { get; private set; }
        public SocketConfiguration SocketConfiguration { get; private set; }

        public EasyServer(ILogger<EasyServer> logger = null, IConfiguration config = null)
        {
            _logger = logger;
            _config = config;

            try
            {
                if (config != null)
                {
                    EasyServerConfiguration = config.GetSection("EasyServer").Get<EasyServerConfiguration>();
                    SocketConfiguration = config.GetSection("EasyServer").GetSection("EasySocket").Get<SocketConfiguration>();
                }
                else
                {
                    EasyServerConfiguration = new EasyServerConfiguration();
                    SocketConfiguration = new SocketConfiguration();
                }

                Initialize(EasyServerConfiguration.MaxConnection);
            }
            catch(Exception exception)
            {
                throw new Exception("Failed to initialize easy server...", exception);
            }
        }

        private void Initialize(int maxConnection)
        {
            _socketPool = new EasyServerSocketPool(maxConnection);
            _accessController = new EasyServerAccessController(maxConnection);

            for (int i = 0; i < maxConnection; i++)
            {
                _socketPool.Push(new EasyServerSocket(_logger, SocketConfiguration, _socketPool, _accessController));
            }

            _logger?.LogInformation("Complete initialize server - max conn:{0}", maxConnection);
        }

        public void Start(string address, int port)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(address), port);

            _listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(localEndPoint);
            _listenSocket.LingerState = EasyServerConfiguration.LingerTime > 0? new LingerOption(true, EasyServerConfiguration.LingerTime) : new LingerOption(false, 0);
            _listenSocket.Listen(EasyServerConfiguration.ListenBackLog);

            // post accepts on the listening socket
            StartAccept(null);

            _logger?.LogInformation("Start Server...");

            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                _exitEvent.Set();
            };

            Console.WriteLine("Press Ctrl + C key to terminate the server process");
            _exitEvent.WaitOne();
            Stop();
        }

        public void Stop()
        {
            _listenSocket.Close();
            _socketPool.Clear();
            _logger?.LogInformation("Stop Server...");
        }

        public void ConnectHandler(Action<IEasyServerSocket> action)
        {
            _connectAction = action;
            _logger?.LogDebug("[EasySocket Server] Add ConnectHandler");
        }

        public void ExceptionHandler(Action<Exception> action)
        {
            _exceptionAction = action;
            _logger?.LogDebug("[EasySocket Server] Add ExceptionHandler");
        }

        public int GetConnectedCount()
        {
            return _socketPool.GetCount();
        }

        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(CompleteAcceptEvent);
            }
            else
            {
                acceptEventArg.AcceptSocket = null;
            }

            _accessController.WaitOne();

            bool asyncOperation = true;
            try
            {
                asyncOperation = _listenSocket.AcceptAsync(acceptEventArg);
            }
            catch(ObjectDisposedException ode)
            {
                // server call stop method
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _exceptionAction?.Invoke(ex);
            }

            if (asyncOperation == false)
            {
                ProcessAccept(acceptEventArg);
            }
        }

        private void CompleteAcceptEvent(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs args)
        {
            Socket socket = args.AcceptSocket;
            try
            {
                Task.Run(() =>
                {
                    IEasyServerSocket easySocket = _socketPool.Pop();
                    if (easySocket != null)
                    {
                        easySocket.SocketId = KeyGenerator.GetServerSocketId();
                        easySocket.Socket = socket;

                        Console.WriteLine("Client connection accepted. There are {0} clients connected to the server", _socketPool.GetCount());

                        _connectAction(easySocket);
                    }
                    else
                    {
                        Console.WriteLine("There are no more available sockets to allocate.");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _exceptionAction?.Invoke(ex);
            }

            // Accept the next connection request.
            this.StartAccept(args);
        }
    }
}
