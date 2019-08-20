using System;
using System.Collections.Generic;
using System.Text;
using EasySocket.Core.Networks;
using EasySocket.Core.Networks.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasySocket.Core.Server
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        private readonly IConfiguration _config;
        private readonly EasyServer _server;

        public Startup(ILogger<Startup> logger, IConfiguration config, EasyServer server)
        {
            _logger = logger;
            _config = config;
            _server = server;
        }

        public void Run(string addr, int port)
        {
            _server.ConnectHandler(socket =>
            {
                string socketId = socket.SocketId;

                _logger.LogInformation($"[{socketId}] Connected");

                socket.ExceptionHandler(exception =>
                {
                    _logger.LogError($"[{socketId}] Exception - {exception.Message}");
                });
                socket.IdleTimeoutHandler(() =>
                {
                    _logger.LogInformation($"[{socketId}] Idle Timeout");
                });
                socket.CloseHandler(() =>
                {
                    _logger.LogInformation($"[{socketId}] Closed");
                });
                socket.Receive(receiveData =>
                {
                    string recvStringData = Encoding.UTF8.GetString(receiveData);
                    _logger.LogInformation($"[{socketId}] Receive data - {recvStringData}, size:{receiveData.Length}");

                    socket.Send(receiveData, size =>
                    {
                        string sendStringData = Encoding.UTF8.GetString(receiveData);
                        _logger.LogInformation($"[{socketId}] Send data - {sendStringData}, size:{size}");
                    });
                });

                int count = _server.GetConnectedCount();
                _logger.LogInformation($"[{socketId}] now total connection count - {count}");

            });
            _server.ExceptionHandler(exception =>
            {
                _logger.LogError($"Exception - {exception.Message}");
            });

            _server.Start(addr, port);
        }
    }
}
