using EasySocket.Core.Factory;
using EasySocket.Core.Tests.Fixture;
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
        private const int AsyncDelayTimeOut = 1500;

        private readonly ITestOutputHelper _output;

        public EchoServerTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void EchoTest()
        {
            int port = EchoServerFixture.EchoServerPort;
            string data = "testData";
            string message = "";

            var client = EasySocketFactory.CreateClient();
            var countdownEvent = new CountdownEvent(1);

            client.ConnectHandler(socket =>
            {
                byte[] sendData = Encoding.UTF8.GetBytes(data);

                socket.Send(sendData, sendSize =>
                {
                    _output.WriteLine("complete send - size : " + sendSize);
                });

                socket.Receive(receivedData =>
                {
                    _output.WriteLine("complete receive - size : " + receivedData.Length);
                    message = Encoding.UTF8.GetString(receivedData);
                    countdownEvent.Signal();
                });
            });

            client.ExceptionHandler(exception =>
            {
                _output.WriteLine("received socket exception" + exception);
            });

            client.Connect("127.0.0.1", port);
            countdownEvent.Wait(AsyncDelayTimeOut);
            Assert.Equal(data, message);
        }

        [Fact]
        public void EchoCuttedSendPacketTest()
        {
            int port = EchoServerFixture.EchoServerPort;
            string firstData = "test";
            string secondData = "Data";
            string response = "";

            var client = EasySocketFactory.CreateClient();
            var countdownEvent = new CountdownEvent(2);

            client.ConnectHandler(socket =>
            {
                byte[] firstSendData = Encoding.UTF8.GetBytes(firstData);

                socket.Send(firstSendData, sendSize =>
                {
                    _output.WriteLine("complete send - data:{0}, size:{1}", Encoding.UTF8.GetString(firstSendData), sendSize);

                });

                byte[] secondSendData = Encoding.UTF8.GetBytes(secondData);

                socket.Send(secondSendData, sendSize =>
                {
                    _output.WriteLine("complete send - data:{0}, size:{1}", Encoding.UTF8.GetString(secondSendData), sendSize);
                });

                socket.Receive(receivedData =>
                {
                    string data = Encoding.UTF8.GetString(receivedData);
                    _output.WriteLine("complete receive - data:{0}, size:{1}", data, data.Length);
                    response += data;
                    countdownEvent.Signal();
                });
            });

            client.ExceptionHandler(exception =>
            {
                _output.WriteLine("received socket exception" + exception);
            });

            client.Connect("127.0.0.1", port);
            countdownEvent.Wait(AsyncDelayTimeOut);
            Assert.Equal(firstData + secondData, response);
        }
    }
}
