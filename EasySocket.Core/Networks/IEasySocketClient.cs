using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace EasySocket.Core.Networks
{
    public interface IEasySocketClient
    {
        /// <summary>
        /// 사용되는 Logger를 지정한다.
        /// </summary>
        ILogger Logger { get; set; }

        /// <summary>
        /// 클라이언트 생성 시 설정된 옵션정보로 host와 port에 connection 요청을 진행한다.
        /// </summary>
        void Connect();

        /// <summary>
        /// host와 post을 입력받아 connection 요청을 진행한다.
        /// </summary>
        /// <param name="host">서버 호스트 정보.</param>
        /// <param name="port">서버 포트 정보.</param>
        void Connect(string host, int port);

        /// <summary>
        /// Connection이 발생할 때 Connection Handler 로 소켓 정보를 전달한다.
        /// Notice : ConnectionHandler 를 지정하지 않으면 클라이언트 연결 시 예외가 발생한다.
        /// </summary>
        /// <param name="action">NPTcpSocket을 받는 handler.</param>
        void ConnectHandler(Action<IEasySocket> action);

        /// <summary>
        /// 서버 연결 시 Exception이 발생할 때 Exception Handler 로 예외 정보를 전달한다.
        /// </summary>
        /// <param name="action"> Exception을 받는 handler.</param>
        void ExceptionHandler(Action<Exception> action);
    }
}
