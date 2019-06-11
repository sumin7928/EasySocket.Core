using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EasySocket.Core.Factory;
using Xunit;
using Xunit.Abstractions;

namespace EasySocket.Core.Tests.Fixture
{
    public class FixedEchoServerFixture : IDisposable
    {
        public const int FixedEchoServerPort = 14001;

        public FixedEchoServerFixture()
        {
            var server = EasySocketFactory.CreateServer();
            server.ConnectHandler(socket =>
            {
                socket.Receive(0, 4, receivedData =>
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
            server.Run(FixedEchoServerPort);
        }

        public void Dispose()
        {

        }
    }

    [CollectionDefinition("FixedEchoServer")]
    public class FixedEchoServerCollection : ICollectionFixture<FixedEchoServerFixture>
    {
    }
}
