using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        /// 설정된 서버 정보로 서버를 구동한다.
        /// </summary>
        Task Listen();
    }
}
