using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySocket.Core.Factory;
using EasySocket.Core.Tests.Fixture;
using Xunit;
using Xunit.Abstractions;

namespace EasySocket.Core.Tests
{
    [Collection("FixedEchoServer")]
    public class FixedEchoServerTest
    {
        private const int AsyncDelayTime = 1500;

        private readonly ITestOutputHelper _output;

        public FixedEchoServerTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task FixedEchoTest()
        {
            int port = FixedEchoServerFixture.FixedEchoServerPort;
            string data = "testData";

            int totalLengthOffset = 0;
            int totalLengthSize = 4;

            MemoryStream stream = new MemoryStream();
            stream.Write(BitConverter.GetBytes(data.Length + totalLengthSize), 0, totalLengthSize);
            byte[] databyte = Encoding.UTF8.GetBytes(data);
            stream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);

            string response = "";

            var result = Task.Run(() =>
            {
                var client = EasySocketFactory.CreateClient();

                client.ConnectHandler(socket =>
                {
                    byte[] sendData = stream.ToArray();

                    socket.Send(sendData, sendSize =>
                    {
                        Console.WriteLine("complete send - size : " + sendSize);

                    });

                    socket.Receive(totalLengthOffset, totalLengthSize, receivedData =>
                    {
                        response = Encoding.UTF8.GetString(receivedData, totalLengthSize, receivedData.Length - totalLengthSize);
                        Console.WriteLine("complete receive - size : " + receivedData.Length);
                    });
                });

                client.ExceptionHandler(exception =>
                {
                    Console.WriteLine("received socket exception" + exception);
                });

                client.Connect("127.0.0.1", port);
            });

            await Task.Delay(AsyncDelayTime);
            Assert.Equal(data, response);
        }

        [Fact]
        public async Task FixedEchoCuttedPacketTest()
        {
            int port = FixedEchoServerFixture.FixedEchoServerPort;
            string data = "testData";

            int totalLengthOffset = 0;
            int totalLengthSize = 4;

            MemoryStream stream = new MemoryStream();
            stream.Write(BitConverter.GetBytes(data.Length + totalLengthSize), 0, totalLengthSize);
            byte[] databyte = Encoding.UTF8.GetBytes(data);
            stream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);

            string response = "";

            var result = Task.Run(() =>
            {
                var client = EasySocketFactory.CreateClient();

                client.ConnectHandler(socket =>
                {
                    byte[] sendData = stream.ToArray();

                    byte[] firstData = sendData.Take(6).ToArray();
                    byte[] secondData = sendData.Skip(firstData.Length).Take(sendData.Length - firstData.Length).ToArray();

                    socket.Send(firstData, sendSize =>
                    {
                        Console.WriteLine("complete send - size : " + sendSize);

                    });

                    socket.Send(secondData, sendSize =>
                    {
                        Console.WriteLine("complete send - size : " + sendSize);

                    });

                    socket.Receive(totalLengthOffset, totalLengthSize, receivedData =>
                    {
                        response = Encoding.UTF8.GetString(receivedData, totalLengthSize, receivedData.Length - totalLengthSize);
                        Console.WriteLine("complete receive - size : " + receivedData.Length);
                    });
                });

                client.ExceptionHandler(exception =>
                {
                    Console.WriteLine("received socket exception" + exception);
                });

                client.Connect("127.0.0.1", port);
            });

            await Task.Delay(AsyncDelayTime);
            Assert.Equal(data, response);
        }

        [Fact]
        public async Task FixedEchoOverloadPacketTest()
        {
            int port = FixedEchoServerFixture.FixedEchoServerPort;
            string data = "testData";

            int totalLengthOffset = 0;
            int totalLengthSize = 4;
            int overloadCount = 2;

            MemoryStream stream = new MemoryStream();
            for (int i = 0; i < overloadCount; ++i)
            {
                stream.Write(BitConverter.GetBytes(data.Length + totalLengthSize), 0, totalLengthSize);
                byte[] databyte = Encoding.UTF8.GetBytes(data);
                stream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
            }

            string response = "";

            var result = Task.Run(() =>
            {
                var client = EasySocketFactory.CreateClient();

                client.ConnectHandler(socket =>
                {
                    byte[] sendData = stream.ToArray();

                    socket.Send(sendData, sendSize =>
                    {
                        Console.WriteLine("complete send - size : " + sendSize);

                    });

                    socket.Receive(totalLengthOffset, totalLengthSize, receivedData =>
                    {
                        response += Encoding.UTF8.GetString(receivedData, totalLengthSize, receivedData.Length - totalLengthSize);
                        Console.WriteLine("complete receive - size : " + receivedData.Length);
                    });
                });

                client.ExceptionHandler(exception =>
                {
                    Console.WriteLine("received socket exception" + exception);
                });

                client.Connect("127.0.0.1", port);
            });

            await Task.Delay(AsyncDelayTime);
            Assert.Equal(data + data, response);
        }
    }
}
