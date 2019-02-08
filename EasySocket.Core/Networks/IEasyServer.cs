using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocket.Core.Networks
{
    public interface IEasyServer
    {
        /// <summary>
        /// Accept가 발생할 때 Connection Handler 로 소켓 정보를 전달한다.
        /// Notice : 반드시 ConnectionHandler 를 지정해야 서버가 구동이 된다.
        /// </summary>
        /// <param name="action"> NPTcpSocket를 받는 handler. </param>
        void ConnectHandler(Action<IEasySocket> action);

        /// <summary>
        /// 서버 구동 시 Exception이 발생할 때 Exception Handler 로 예외 정보를 전달한다.
        /// </summary>
        /// <param name="action"> Exception을 받는 handler.</param>
        void ExceptionHandler(Action<Exception> action);

        /// <summary>
        /// 구동중인 서버를 종료한다.
        /// </summary>
        void Stop();

        /// <summary>
        /// 서버 생성 시 옵션에 설정된 포트로 Port Listening을 진행한다.
        /// </summary>
        void Listen();

        /// <summary>
        /// 해당 포트로 Port Listening을 진행한다.(주소는 옵션값 설정)
        /// </summary>
        /// <param name="port"> Listen Port 지정.</param>
        void Listen(int port);

        /// <summary>
        /// 해당 주소와 포트로 Port Listening을 진행한다.
        /// </summary>
        /// <param name="port"> Listen Port 지정.</param>
        void Listen(string address, int port);

    }
}
