using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EasySocket.Core.Networks;
using Xunit;
using Xunit.Abstractions;

namespace EasySocket.Core.Tests.Fixture
{
    public class FixedEchoServerFixture : IDisposable
    {
        public const int LittleEndianFixedEchoServerPort = 14001;
        public const int BigEndianFixedEchoServerPort = 14002;

        public FixedEchoServerFixture()
        {
            EasyServer littelEndianServer = new EasyServer();

            //littelEndianServer.ConnectHandler(socket =>
            //{
            //    var totalLengthObject = new TotalLengthObject
            //    {
            //        IsBigEndian = false
            //    };
            //    socket.Receive(totalLengthObject, receivedData =>
            //    {
            //        socket.Send(receivedData, sendSize =>
            //        {
            //        });
            //    });
            //    socket.CloseHandler(clientId =>
            //    {
            //    });
            //    socket.ExceptionHandler(exception =>
            //    {
            //    });
            //});
            //littelEndianServer.ExceptionHandler(exception =>
            //{
            //});
            //littelEndianServer.Run(LittleEndianFixedEchoServerPort);

            //var bigEndianServer = EasySocketFactory.CreateServer();

            //bigEndianServer.ConnectHandler(socket =>
            //{
            //    var totalLengthObject = new TotalLengthObject
            //    {
            //        IsBigEndian = true
            //    };
            //    socket.Receive(totalLengthObject, receivedData =>
            //    {
            //        socket.Send(receivedData, sendSize =>
            //        {
            //        });
            //    });
            //    socket.CloseHandler(clientId =>
            //    {
            //    });
            //    socket.ExceptionHandler(exception =>
            //    {
            //    });
            //});
            //bigEndianServer.ExceptionHandler(exception =>
            //{
            //});
            //bigEndianServer.Run(BigEndianFixedEchoServerPort);
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
