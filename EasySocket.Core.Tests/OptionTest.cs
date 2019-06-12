using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasySocket.Core.Factory;
using EasySocket.Core.Options;
using Xunit;
using Xunit.Abstractions;

namespace EasySocket.Core.Tests
{
    public class OptionTest
    {
        private const int asyncDelayTime = 1500;

        private readonly ITestOutputHelper _output;

        public OptionTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void IdleTimeoutTest()
        {
            bool timeout = false;
            int targetPort = 15000;

            ServerOptions options = new ServerOptions();
            options.Port = targetPort;
            options.IdleTimeout = 800;

            var server = EasySocketFactory.CreateServer(options);
            var countdownEvent = new CountdownEvent(1);

            server.ConnectHandler(socket =>
            {
                socket.IdleTimeoutHandler(clientId =>
                {
                    _output.WriteLine("server socket idle timeout - id: " + clientId);
                    timeout = true;
                    countdownEvent.Signal();
                });
                socket.CloseHandler(clientId =>
                {
                    _output.WriteLine("server closed socket - id: " + clientId);
                });

                socket.ExceptionHandler(exception =>
                {
                    _output.WriteLine("server socket exception - " + exception);
                });

                socket.Receive(receivedData =>
                {
                    _output.WriteLine("server received data - " + Encoding.UTF8.GetString(receivedData));

                });
            });
            server.ExceptionHandler(exception =>
            {
                _output.WriteLine("server exception - " + exception);
            });

            server.Run();

            var client = EasySocketFactory.CreateClient();

            client.ConnectHandler(socket =>
            {
                socket.CloseHandler(clientId =>
                {
                    _output.WriteLine("client closed socket - id: " + clientId);
                });

                socket.ExceptionHandler(exception =>
                {
                    _output.WriteLine("client socket exception" + exception);
                });
            });

            client.ExceptionHandler(exception =>
            {
                _output.WriteLine("client exception" + exception);
            });

            client.Connect("127.0.0.1", targetPort);

            countdownEvent.Wait(asyncDelayTime);
            Assert.True(timeout);
        }

        [Fact]
        public void IdleTimeoutTestWithAlive()
        {
            bool timeout = false;
            int targetPort = 15001;

            ServerOptions options = new ServerOptions();
            options.Port = targetPort;
            options.IdleTimeout = 800;

            var server = EasySocketFactory.CreateServer(options);
            var countdownEvent = new CountdownEvent(1);

            server.ConnectHandler(socket =>
            {
                socket.IdleTimeoutHandler(clientId =>
                {
                    _output.WriteLine("server socket idle timeout - id: " + clientId);
                    timeout = true;
                    countdownEvent.Signal();
                });
                socket.CloseHandler(clientId =>
                {
                    _output.WriteLine("server closed socket - id: " + clientId);
                });

                socket.ExceptionHandler(exception =>
                {
                    _output.WriteLine("server socket exception - " + exception);
                });

                socket.Receive(receivedData =>
                {
                    _output.WriteLine("server received data - " + Encoding.UTF8.GetString(receivedData));

                });
            });
            server.ExceptionHandler(exception =>
            {
                _output.WriteLine("server exception - " + exception);
            });

            server.Run();

            var client = EasySocketFactory.CreateClient();

            client.ConnectHandler(socket =>
            {
                byte[] sendData = Encoding.UTF8.GetBytes("testData");

                Thread sendThead = new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(400);

                        socket.Send(sendData, sendSize =>
                        {
                            _output.WriteLine("complete send - size : {0}", sendSize);
                        });
                    }
                });
                sendThead.Start();

                socket.Receive(receivedData =>
                {
                    _output.WriteLine("complete receive - size : {0}", receivedData.Length);
                });

                socket.CloseHandler(clientId =>
                {
                    _output.WriteLine("client closed socket - id: " + clientId);
                });

                socket.ExceptionHandler(exception =>
                {
                    _output.WriteLine("client socket exception" + exception);
                });
            });

            client.ExceptionHandler(exception =>
            {
                _output.WriteLine("client exception" + exception);
            });

            client.Connect("127.0.0.1", targetPort);

            countdownEvent.Wait(asyncDelayTime);
            Assert.False(timeout);

        }

        [Fact]
        public void ReadTimeoutTest()
        {
            bool timeout = false;
            int targetPort = 15002;

            ServerOptions options = new ServerOptions();
            options.Port = targetPort;

            var server = EasySocketFactory.CreateServer(options);
            var countdownEvent = new CountdownEvent(1);

            server.ConnectHandler(socket =>
            {
                socket.CloseHandler(clientId =>
                {
                    _output.WriteLine("server closed socket - id: " + clientId);
                });

                socket.ExceptionHandler(exception =>
                {
                    _output.WriteLine("server socket exception - " + exception);
                });

                socket.Receive(receivedData =>
                {
                    _output.WriteLine("server received data - " + Encoding.UTF8.GetString(receivedData));

                });
            });
            server.ExceptionHandler(exception =>
            {
                _output.WriteLine("server exception - " + exception);
            });

            server.Run();

            ClientOptions clientOptions = new ClientOptions();
            clientOptions.ReadTimeout = 800;

            var client = EasySocketFactory.CreateClient(clientOptions);

            client.ConnectHandler(socket =>
            {
                byte[] sendData = Encoding.UTF8.GetBytes("testData");

                socket.Send(sendData, sendSize =>
                {
                    _output.WriteLine("complete send - size : {0}", sendSize);
                });

                socket.ReadTimeoutHandler(clientId =>
                {
                    _output.WriteLine("client socket read timeout - id: " + clientId);
                    timeout = true;
                    countdownEvent.Signal();
                });

                socket.CloseHandler(clientId =>
                {
                    _output.WriteLine("client closed socket - id: " + clientId);
                });

                socket.ExceptionHandler(exception =>
                {
                    _output.WriteLine("client socket exception" + exception);
                });
            });

            client.ExceptionHandler(exception =>
            {
                _output.WriteLine("client exception" + exception);
            });

            client.Connect("127.0.0.1", targetPort);

            countdownEvent.Wait(asyncDelayTime);
            Assert.True(timeout);
        }
    }
}
