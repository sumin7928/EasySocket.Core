using System;
using System.Collections.Generic;
using System.Text;
using EasySocket.Core.Networks.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasySocket.Core.Client
{
    class Startup
    {
        private readonly ILogger<Startup> _logger;
        private readonly IConfiguration _config;
        private readonly EasyClient _client;

        public Startup(ILogger<Startup> logger, IConfiguration config, EasyClient client)
        {
            _logger = logger;
            _config = config;
            _client = client;
        }

        public void Run(string addr, int port)
        {
            _client.ConnectHandler(socket =>
            {
                string socketId = socket.SocketId;

                _logger.LogInformation($"[{socketId}] Connected");

                socket.ExceptionHandler(exception =>
                {
                    _logger.LogError($"[{socketId}] Exception - {exception.Message}");
                });
                socket.ReadTimeoutHandler(() =>
                {
                    _logger.LogInformation($"[{socketId}] Read Timeout");
                });
                socket.CloseHandler(() =>
                {
                    _logger.LogInformation($"[{socketId}] Closed");
                });


                byte[] sendData = Encoding.UTF8.GetBytes("testData");
                socket.Send(sendData, size =>
                {
                    string sendStringData = Encoding.UTF8.GetString(sendData);
                    _logger.LogInformation($"[{socketId}] Send data - {sendStringData}, size:{size}");
                });

                socket.Receive(receiveData =>
                {
                    string recvStringData = Encoding.UTF8.GetString(receiveData);
                    _logger.LogInformation($"[{socketId}] Receive data - {recvStringData}, size:{receiveData.Length}");
                });

            });
            _client.ExceptionHandler(exception =>
            {
                _logger.LogError($"Exception - {exception.Message}");
            });

            _client.Connect(addr, port);

            _logger.LogInformation("Press any key to terminate the client process....");
            Console.ReadKey();
        }
    }
}
