using EasySocket.Core.Factory;
using EasySocket.Core.Tests.Fixture;
using NPStandardTcpLibTest.Helper;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace EasySocket.Core.Tests
{
    [Collection("EchoServer")]
    public class EchoServerTest
    {
        private const int AsyncDelayTime = 1500;

        private readonly ITestOutputHelper _output;

        public EchoServerTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task EchoTest()
        {
            int port = EchoServerFixture.EchoServerPort;
            string data = "testData";
            string response = "";

            var result = Task.Run( () =>
            {
                var client = EasySocketFactory.CreateClient();

                client.ConnectHandler(socket =>
                {
                    byte[] sendData = Encoding.UTF8.GetBytes(data);

                    socket.Send(sendData, sendSize =>
                    {
                        _output.WriteLine("complete send - size : " + sendSize);

                    });

                    socket.Receive(receivedData =>
                    {
                        response = Encoding.UTF8.GetString(receivedData);
                        _output.WriteLine("complete receive - size : " + receivedData.Length);
                    });
                });

                client.ExceptionHandler(exception =>
                {
                    _output.WriteLine("received socket exception" + exception);
                });

                client.Connect( "127.0.0.1", port);
            });

            await Task.Delay(AsyncDelayTime);
            Assert.Equal(data, response);
        }

        [Fact]
        public async Task EchoCuttedPacketTest()
        {
            int port = EchoServerFixture.EchoServerPort;
            string firstData = "test";
            string secondData = "Data";
            string response = "";

            var result = Task.Run(() =>
            {
                var client = EasySocketFactory.CreateClient();

                client.ConnectHandler(socket =>
                {
                    byte[] firstSendData = Encoding.UTF8.GetBytes(firstData);

                    socket.Send(firstSendData, sendSize =>
                    {
                        _output.WriteLine("complete send - size : {0}", sendSize);

                    });

                    byte[] secondSendData = Encoding.UTF8.GetBytes(secondData);

                    socket.Send(secondSendData, sendSize =>
                    {
                        _output.WriteLine("complete send - size : {0}", sendSize);

                    });

                    socket.Receive(receivedData =>
                    {
                        response += Encoding.UTF8.GetString(receivedData);
                        _output.WriteLine("complete receive - size : {0}", receivedData.Length);
                    });
                });

                client.ExceptionHandler(exception =>
                {
                    _output.WriteLine("received socket exception" + exception);
                });

                client.Connect("127.0.0.1", port);
            });

            await Task.Delay(AsyncDelayTime);
            Assert.Equal(firstData + secondData, response);
        }
    }
}
