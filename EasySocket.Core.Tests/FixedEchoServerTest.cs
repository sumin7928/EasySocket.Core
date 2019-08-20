using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public void FixedEchoTest()
        {
            //int port = FixedEchoServerFixture.LittleEndianFixedEchoServerPort;
            //string data = "testData";
            //string response = "";

            //int totalLengthSize = 4;

            //MemoryStream stream = new MemoryStream();
            //stream.Write(BitConverter.GetBytes(data.Length + totalLengthSize), 0, totalLengthSize);
            //byte[] databyte = Encoding.UTF8.GetBytes(data);
            //stream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);

            //var client = EasySocketFactory.CreateClient();
            //var countdownEvent = new CountdownEvent(1);

            //client.ConnectHandler(socket =>
            //{
            //    byte[] sendData = stream.ToArray();

            //    socket.Send(sendData, sendSize =>
            //    {
            //        _output.WriteLine("complete send - size : " + sendSize);

            //    });

            //    socket.Receive(new TotalLengthObject(), receivedData =>
            //    {
            //        response = Encoding.UTF8.GetString(receivedData, totalLengthSize, receivedData.Length - totalLengthSize);
            //        _output.WriteLine("complete receive - size : " + receivedData.Length);
            //        countdownEvent.Signal();
            //    });
            //});

            //client.ExceptionHandler(exception =>
            //{
            //    _output.WriteLine("received socket exception" + exception);
            //});

            //client.Connect("127.0.0.1", port);

            //countdownEvent.Wait(AsyncDelayTime);
            //Assert.Equal(data, response);
        }

        [Fact]
        public void FixedEchoCuttedPacketTest()
        {
            //int port = FixedEchoServerFixture.LittleEndianFixedEchoServerPort;
            //string data = "testData";
            //string response = "";

            //int totalLengthSize = 4;

            //MemoryStream stream = new MemoryStream();
            //stream.Write(BitConverter.GetBytes(data.Length + totalLengthSize), 0, totalLengthSize);
            //byte[] databyte = Encoding.UTF8.GetBytes(data);
            //stream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);

            //var client = EasySocketFactory.CreateClient();
            //var countdownEvent = new CountdownEvent(2);

            //client.ConnectHandler(socket =>
            //{
            //    byte[] sendData = stream.ToArray();

            //    byte[] firstData = sendData.Take(6).ToArray();
            //    byte[] secondData = sendData.Skip(firstData.Length).Take(sendData.Length - firstData.Length).ToArray();

            //    socket.Send(firstData, sendSize =>
            //    {
            //        _output.WriteLine("complete send - size : " + sendSize);

            //    });

            //    socket.Send(secondData, sendSize =>
            //    {
            //        _output.WriteLine("complete send - size : " + sendSize);

            //    });

            //    socket.Receive(new TotalLengthObject(), receivedData =>
            //    {
            //        response = Encoding.UTF8.GetString(receivedData, totalLengthSize, receivedData.Length - totalLengthSize);
            //        _output.WriteLine("complete receive - size : " + receivedData.Length);
            //        countdownEvent.Signal();
            //    });
            //});

            //client.ExceptionHandler(exception =>
            //{
            //    _output.WriteLine("received socket exception" + exception);
            //});

            //client.Connect("127.0.0.1", port);

            //countdownEvent.Wait(AsyncDelayTime);
            //Assert.Equal(data, response);
        }

        [Fact]
        public void FixedEchoOverloadPacketTest()
        {
            //int port = FixedEchoServerFixture.LittleEndianFixedEchoServerPort;
            //string data = "testData";
            //string response = "";

            //int totalLengthSize = 4;
            //int overloadCount = 2;

            //MemoryStream stream = new MemoryStream();
            //for (int i = 0; i < overloadCount; ++i)
            //{
            //    stream.Write(BitConverter.GetBytes(data.Length + totalLengthSize), 0, totalLengthSize);
            //    byte[] databyte = Encoding.UTF8.GetBytes(data);
            //    stream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
            //}

            //var client = EasySocketFactory.CreateClient();
            //var countdownEvent = new CountdownEvent(2);

            //client.ConnectHandler(socket =>
            //{
            //    byte[] sendData = stream.ToArray();

            //    socket.Send(sendData, sendSize =>
            //    {
            //        _output.WriteLine("complete send - size : " + sendSize);

            //    });

            //    socket.Receive(new TotalLengthObject(), receivedData =>
            //    {
            //        response += Encoding.UTF8.GetString(receivedData, totalLengthSize, receivedData.Length - totalLengthSize);
            //        _output.WriteLine("complete receive - size : " + receivedData.Length);
            //        countdownEvent.Signal();
            //    });
            //});

            //client.ExceptionHandler(exception =>
            //{
            //    _output.WriteLine("received socket exception" + exception);
            //});

            //client.Connect("127.0.0.1", port);

            //countdownEvent.Wait(AsyncDelayTime);
            //Assert.Equal(data + data, response);
        }

        [Fact]
        public void FixedEchoTestForBigEndian()
        {
            //int port = FixedEchoServerFixture.BigEndianFixedEchoServerPort;
            //string data = "testData";
            //string response = "";

            //int totalLengthSize = 4;

            //MemoryStream stream = new MemoryStream();
            //byte[] totalLength = BitConverter.GetBytes(data.Length + totalLengthSize);
            //Array.Reverse(totalLength);
            //stream.Write(totalLength, 0, totalLengthSize);
            //byte[] databyte = Encoding.UTF8.GetBytes(data);
            //stream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);

            //var client = EasySocketFactory.CreateClient();
            //var countdownEvent = new CountdownEvent(1);

            //client.ConnectHandler(socket =>
            //{
            //    byte[] sendData = stream.ToArray();

            //    socket.Send(sendData, sendSize =>
            //    {
            //        _output.WriteLine("complete send - size : " + sendSize);

            //    });

            //    var totalLengthObject = new TotalLengthObject
            //    {
            //        IsBigEndian = true
            //    };
            //    socket.Receive(totalLengthObject, receivedData =>
            //    {
            //        byte[] length = new byte[4];
            //        Array.Copy(receivedData, length, 4);
            //        Array.Reverse(length);

            //        int resultLength = BitConverter.ToInt32(length);
            //        Assert.Equal(data.Length + totalLengthSize, resultLength);
            //        response = Encoding.UTF8.GetString(receivedData, totalLengthSize, receivedData.Length - totalLengthSize);
            //        _output.WriteLine("complete receive - size : " + receivedData.Length);
            //        countdownEvent.Signal();
            //    });
            //});

            //client.ExceptionHandler(exception =>
            //{
            //    _output.WriteLine("received socket exception" + exception);
            //});

            //client.Connect("127.0.0.1", port);

            //countdownEvent.Wait(AsyncDelayTime);
            //Assert.Equal(data, response);
        }
    }
}
