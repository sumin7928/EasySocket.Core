using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasySocket.Core.Networks;
using Xunit;
using Xunit.Abstractions;

namespace EasySocket.Core.Tests.Fixture
{
    public class EchoServerFixture : IDisposable
    {
        public const int EchoServerPort = 14000;

        public EchoServerFixture()
        {
            Task.Run(() =>
            {
                EasyServer server = new EasyServer();
                server.ConnectHandler(socket =>
                {
                    socket.Receive(receivedData =>
                    {
                        socket.Send(receivedData, sendSize =>
                        {
                        });
                    });
                    socket.CloseHandler(() =>
                    {
                    });
                    socket.ExceptionHandler(exception =>
                    {
                    });
                });
                server.ExceptionHandler(exception =>
                {
                });
                server.Start("127.0.0.1", EchoServerPort);
            });
        }

        public void Dispose()
        {

        }
    }


    [CollectionDefinition("EchoServer")]
    public class EchoServerCollection : ICollectionFixture<EchoServerFixture>
    {
    }
}
