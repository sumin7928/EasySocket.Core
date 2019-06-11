using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EasySocket.Core.Factory;
using Xunit;
using Xunit.Abstractions;

namespace EasySocket.Core.Tests.Fixture
{
    public class EchoServerFixture : IDisposable
    {
        public const int EchoServerPort = 14000;

        public EchoServerFixture()
        {
            var server = EasySocketFactory.CreateServer();
            server.ConnectHandler(socket =>
            {
                socket.Receive(receivedData =>
                {
                    socket.Send(receivedData, sendSize =>
                    {
                    });
                });
                socket.CloseHandler(clientId =>
                {
                });
                socket.ExceptionHandler(exception =>
                {
                });
            });
            server.ExceptionHandler(exception =>
            {
            });
            server.Run(EchoServerPort);
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
