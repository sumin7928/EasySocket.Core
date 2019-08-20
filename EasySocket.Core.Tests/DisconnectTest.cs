using EasySocket.Core.Networks;
using EasySocket.Core.Networks.Client;
using EasySocket.Core.Tests.Fixture;
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
            int port = 15001;
            string data = "testData";
            bool closed = false;

            EasyServer server = new EasyServer();
            Task.Run(() =>
            {
                server.ConnectHandler(socket =>
                {
                    string socketId = socket.SocketId;
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
                        socket.Close();
                    });
                });
                server.ExceptionHandler(exception =>
                {
                    _output.WriteLine($"server start exception handler - {exception}");
                });
                server.Start("127.0.0.1", port);
            }).Wait(1500);

            EasyClient client = new EasyClient();
            CountdownEvent countdownEvent = new CountdownEvent(1);

            client.ConnectHandler(socket =>
            {
                string socketId = socket.SocketId;
                socket.CloseHandler(() =>
                {
                    _output.WriteLine($"[{socketId}] client socket close handler");
                    closed = true;
                    countdownEvent.Signal();
                });
                socket.ExceptionHandler(exception =>
                {
                    _output.WriteLine($"[{socketId}] client socket close handler");
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
            countdownEvent.Wait();
            server.Stop();
            Assert.True(closed);
        }
    }
}
