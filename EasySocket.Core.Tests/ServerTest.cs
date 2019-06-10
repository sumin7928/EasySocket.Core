using EasySocket.Core.Networks;
using System;
using Xunit;

namespace EasySocket.Core.Tests
{
    public class ServerTest
    {
        [Fact]
        public async void StartServerTest()
        {
            IEasyServer easyServer = EasyCore.CreateServer();
            easyServer.ConnectHandler( socket =>
            {

            } );
            easyServer.ExceptionHandler( exception =>
            {

            } );
            await easyServer.Listen();
        }
    }
}
