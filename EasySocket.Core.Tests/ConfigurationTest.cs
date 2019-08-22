using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasySocket.Core.Networks;
using EasySocket.Core.Networks.Client;
using Xunit;
using Xunit.Abstractions;

namespace EasySocket.Core.Tests
{
    public class ConfigurationTest
    {
        private readonly ITestOutputHelper _output;

        public ConfigurationTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ServerIdleTimeoutTest()
        {
            int port = 16001;
            bool timeout = false;

            CountdownEvent countdownEvent = new CountdownEvent(1);

            EasyServer server = new EasyServer();
            Task.Run(() =>
            {
                server.ConnectHandler(socket =>
                {
                    socket.SocketConfiguration.IdleTimeout = 2000;

                    string socketId = socket.SocketId;
                    socket.IdleTimeoutHandler(() =>
                    {
                        _output.WriteLine($"[{socketId}] server idle timeout handler");
                        timeout = true;
                        countdownEvent.Signal();
                    });
                    socket.CloseHandler(() =>
                    {
                        _output.WriteLine($"[{socketId}] server socket close handler");
                    });
                    socket.ExceptionHandler(exception =>
                    {
                        _output.WriteLine($"[{socketId}] server socket exception handler - {exception}");
                    });
                    socket.Receive(receivedData =>
                    {
                        string stringData = Encoding.UTF8.GetString(receivedData);
                        _output.WriteLine($"[{socketId}] server socket receive - {stringData}:{stringData.Length}");
                    });
                });
                server.ExceptionHandler(exception =>
                {
                    _output.WriteLine($"server start exception handler - {exception}");
                });
                server.Start("127.0.0.1", port);
            }).Wait(1000);

            EasyClient client = new EasyClient();
            client.ConnectHandler(socket =>
            {
                // skip
            });
            client.ExceptionHandler(exception =>
            {
                _output.WriteLine($"client connect exception handler - {exception}");
            });

            client.Connect("127.0.0.1", port);
            countdownEvent.Wait(5000);
            server.Stop();
            Assert.True(timeout);
        }

        [Fact]
        public void ClientReadTimeoutTest()
        {
            int port = 16002;
            bool timeout = false;

            CountdownEvent countdownEvent = new CountdownEvent(1);

            EasyServer server = new EasyServer();
            Task.Run(() =>
            {
                server.ConnectHandler(socket =>
                {
                    string socketId = socket.SocketId;
                    socket.IdleTimeoutHandler(() =>
                    {
                        _output.WriteLine($"[{socketId}] server idle timeout handler");
                    });
                    socket.CloseHandler(() =>
                    {
                        _output.WriteLine($"[{socketId}] server socket close handler");
                    });
                    socket.ExceptionHandler(exception =>
                    {
                        _output.WriteLine($"[{socketId}] server socket exception handler - {exception}");
                    });
                    socket.Receive(receivedData =>
                    {
                        string stringData = Encoding.UTF8.GetString(receivedData);
                        _output.WriteLine($"[{socketId}] server socket receive - {stringData}:{stringData.Length}");
                    });
                });
                server.ExceptionHandler(exception =>
                {
                    _output.WriteLine($"server start exception handler - {exception}");
                });
                server.Start("127.0.0.1", port);
            }).Wait(1000);

            EasyClient client = new EasyClient();
            client.ConnectHandler(socket =>
            {
                socket.SocketConfiguration.ReadTimeout = 2000;

                string socketId = socket.SocketId;
                string data = "testData";

                socket.CloseHandler(() =>
                {
                    _output.WriteLine($"[{socketId}] client socket close handler");
                });
                socket.ExceptionHandler(exception =>
                {
                    _output.WriteLine($"[{socketId}] client socket close handler");
                });
                socket.ReadTimeoutHandler(() =>
                {
                    _output.WriteLine($"[{socketId}] client socket read timeout handler");
                    timeout = true;
                    countdownEvent.Signal();
                });

                byte[] sendData = Encoding.UTF8.GetBytes(data);
                socket.Send(sendData, sendSize =>
                {
                    _output.WriteLine($"[{socketId}] client socket send  - {data}:{data.Length}");
                });
                socket.Receive(receivedData =>
                {
                    string stringData = Encoding.UTF8.GetString(receivedData);
                    _output.WriteLine($"[{socketId}] client socket receive  - {stringData}:{stringData.Length}");
                });
            });
            client.ExceptionHandler(exception =>
            {
                _output.WriteLine($"client connect exception handler - {exception}");
            });

            client.Connect("127.0.0.1", port);
            countdownEvent.Wait(5000);
            server.Stop();
            Assert.True(timeout);
        }
    }
}
