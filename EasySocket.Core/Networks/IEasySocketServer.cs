using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasySocket.Core.Options;
using Microsoft.Extensions.Logging;

namespace EasySocket.Core.Networks
{
    public interface IEasySocketServer
    {
        /// <summary>
        /// 종속성 주입된 로거 프로퍼티
        /// </summary>
        ILogger<EasySocketServer> Logger { get; }

        /// <summary>
        /// 서버 설정된 옵션 프로퍼티
        /// </summary>
        ServerOptions ServerOptions { get; set; }

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
        /// 설정된 서버 정보로 서버를 구동한다.
        /// </summary>
        void Run();

        /// <summary>
        /// 설정된 서버 정보로 서버를 구동한다.
        /// </summary>
        /// <param name="port">서버 포트 정보</param>
        /// <returns></returns>
        void Run(int port);

        /// <summary>
        /// 구동된 서버를 중지한다.
        /// </summary>
        void Stop();

    }
}
