using EasySocket.Core.Factory;
using EasySocket.Core.Networks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace EasySocket.Core.Tests
{
    public class DisconnectTest
    {
        private const int DelayTime = 1500;

        private readonly ITestOutputHelper _output;

        public DisconnectTest(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// 서버에서 끊겼을 경우
        /// 서버에서 정상 패킷을 수신한 후 소켓을 강제로 종료해 클라에서는 Receive 처리가 없고 바로 종료 처리되는 테스트
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void ClosedSocketFromServer()
        {
            string data = "testData";
            bool closed = false;
            int targetPort = 10000;

            var server = EasySocketFactory.CreateServer();
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

                    // close
                    socket.Close();

                });
            });
            server.ExceptionHandler(exception =>
            {
                _output.WriteLine("server exception - " + exception);
            });

            server.Run(targetPort);

            var client = EasySocketFactory.CreateClient();
            var countdownEvent = new CountdownEvent(1);

            client.ConnectHandler(socket =>
            {
                byte[] sendData = Encoding.UTF8.GetBytes(data);

                socket.Receive(receivedData =>
                {
                    _output.WriteLine("client complete receive - size : " + receivedData.Length);
                });

                socket.Send(sendData, sendSize =>
                {
                    _output.WriteLine("client complete send - size : " + sendSize);

                });

                socket.CloseHandler(clientId =>
                {
                    _output.WriteLine("client closed socket - id: " + clientId);
                    closed = true;
                    countdownEvent.Signal();
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

            countdownEvent.Wait(DelayTime);
            server.Stop();
            Assert.True(closed);
        }

        /// <summary>
        /// 클라에서 끊겼을 경우
        /// 서버에서 정상 패킷을 수신한 후 정상 응답을 보냈는데 클라에서는 Send 후 바로 Close 시킨 상황이라 비정상 예외가 발생하는 테스트
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void ClosedSocketFromClient()
        {
            string data = "testData";
            bool closed = false;
            int targetPort = 10001;

            var server = EasySocketFactory.CreateServer();
            var countdownEvent = new CountdownEvent(1);

            server.ConnectHandler(socket =>
            {
                socket.CloseHandler(clientId =>
                {
                    _output.WriteLine("server closed socket - id: " + clientId);
                    closed = true;
                    countdownEvent.Signal();
                });

                socket.ExceptionHandler(exception =>
                {
                    _output.WriteLine("server socket exception - " + exception);
                });

                socket.Receive(receivedData =>
                {
                    _output.WriteLine("server received data - " + Encoding.UTF8.GetString(receivedData));

                    socket.Send(receivedData, sendSize =>
                    {
                        _output.WriteLine("server completed send - size : " + sendSize);

                    });
                });
            });
            server.ExceptionHandler(exception =>
            {
                _output.WriteLine("server exception - " + exception);
            });
            server.Run(targetPort);

            var client = EasySocketFactory.CreateClient();

            client.ConnectHandler(socket =>
            {
                byte[] sendData = Encoding.UTF8.GetBytes(data);

                socket.Receive(receivedData =>
                {
                    _output.WriteLine("client complete receive - size : " + receivedData.Length);
                });

                socket.Send(sendData, sendSize =>
                {
                    _output.WriteLine("client complete send - size : " + sendSize);

                    // close
                    socket.Close();
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

            countdownEvent.Wait(DelayTime);
            server.Stop();

            Assert.True(closed);
        }
    }
}
