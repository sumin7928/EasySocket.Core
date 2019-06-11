using EasySocket.Core.Factory;
using EasySocket.Core.Networks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit.Abstractions;

namespace NPStandardTcpLibTest.Helper
{
    public class TestHelper
    {
        protected readonly ITestOutputHelper _output;

        public TestHelper(ITestOutputHelper output)
        {
            _output = output;
        }

        public void RunEchoServer(int port)
        {
            var server = EasySocketFactory.CreateServer();

            var thread = new Thread(() =>
            {
                server.ConnectHandler(socket =>
                {
                    socket.Receive(receivedData =>
                    {
                        _output.WriteLine("received data - " + Encoding.UTF8.GetString(receivedData));

                        socket.Send(receivedData, sendSize =>
                        {
                            _output.WriteLine("complete send - size : " + sendSize);

                        });
                    });
                    socket.CloseHandler(clientId =>
                    {
                        _output.WriteLine("closed socket - id: " + clientId);
                    });
                    socket.ExceptionHandler(exception =>
                    {
                        _output.WriteLine("received socket exception - " + exception);
                    });
                });
                server.ExceptionHandler(exception =>
                {
                    _output.WriteLine("test exception - " + exception);
                });

                server.Run(port);
            });
            thread.Start();
        }

        public void RunFixedLengthEchoServer(int port)
        {
            var server = EasySocketFactory.CreateServer();
            int totalSizeOffset = 0;
            int totalSizeLength = 4;

            var thread = new Thread(() =>
            {
                server.ConnectHandler(socket =>
                {
                    socket.Receive(totalSizeOffset, totalSizeLength, receivedData =>
                    {
                        _output.WriteLine("received data - " + BitConverter.ToString(receivedData));

                        socket.Send(receivedData, sendSize =>
                        {
                            _output.WriteLine("complete send - size : " + sendSize);
                        });
                    });

                    socket.CloseHandler(clientId =>
                    {
                        _output.WriteLine("closed socket - id: " + clientId);
                    });
                    socket.ExceptionHandler(exception =>
                    {
                        _output.WriteLine("received socket exception - " + exception);
                    });
                });
                server.ExceptionHandler(exception =>
                {
                    _output.WriteLine("test exception - " + exception);
                });

                server.Run(port);
            });
            thread.Start();
        }
    }
}
