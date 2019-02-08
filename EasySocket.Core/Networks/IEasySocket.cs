using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EasySocket.Core.Networks
{
    public interface IEasySocket
    {

        /// <summary>
        /// socket에 헤더 정보 값을 넣어줌
        /// </summary>
        /// <param name="key">header key.</param>
        /// <param name="value">value</param>
        void Put(object key, object value);

        /// <summary>
        /// socket 헤더에 저장된 정보 가져옴
        /// </summary>
        /// <param name="key">header key.</param>
        /// <returns></returns>
        object Get(object key);

        /// <summary>
        /// socket 헤더에 저장된 정보 가져옴 없을 시 default 값 리턴
        /// </summary>
        /// <param name="key">header key.</param>
        /// <param name="defaultValue">없을 시 default 값.</param>
        /// <returns></returns>
        object Get(object key, object defaultValue);

        /// <summary>
        /// 비동기로 socket receive 처리 구현, action handler로 받은 데이터 그대로 넘겨준다.
        /// </summary>
        /// <param name="action"></param>
        void Receive(Action<byte[]> action);

        /// <summary>
        /// 비동기로 socket receive 처리 구현, action handler로 지정된 패킷 총 길이 만큼 byte[]을 받는다.
        /// </summary>
        /// <param name="totalLengthOffet">패킷 총 길이에 해당하는 binary offset 지정.</param>
        /// <param name="totalLengthSize">패킷 총 길이에 해당하는 binary size 지정.</param>
        /// <param name="action">패킷 byte[]을 받는 handler.</param>
        void Receive(int totalLengthOffet, int totalLengthSize, Action<byte[]> action);

        /// <summary>
        /// 비동기로 socket send 처리 구현, action handler로 보낸 총 바이트 수를 받는다.
        /// </summary>
        /// <param name="sendData">보낼 byte[] 데이터.</param>
        /// <param name="length">보낼 byte[] 길이.</param>
        /// <param name="action">보낸 패킷 사이즈를 받는 handler.</param>
        void Send(byte[] sendData, int length, Action<int> action);

        /// <summary>
        /// 소켓을 종료시킨다. ( Shutdown 진행 )
        /// </summary>
        void Close();

        /// <summary>
        /// 요청 후 응답 타임아웃 핸들러 지정.
        /// </summary>
        /// <param name="action">read timeout 발생 시 id를 받는 handler.</param>
        void ReadTimeoutHandler(Action<string> action);

        /// <summary>
        /// idle 상태의 타임아웃 핸들러 지정.
        /// </summary>
        /// <param name="action">idle timeout 발생 시 id를 받는 handler.</param>
        void IdleTimeoutHandler(Action<string> action);

        /// <summary>
        /// 소켓 close가 발생했을 때 처리를 위한 핸들러 지정.
        /// </summary>
        /// <param name="action">close 되었을 때 id를 받는 handler.</param>
        void CloseHandler(Action<string> action);

        /// <summary>
        /// 소켓 excetion이 발생했을 때 처리를 위한 핸들러 지정.
        /// </summary>
        /// <param name="action">Exception을 받는 handler.</param>
        void ExceptionHandler(Action<Exception> action);

        /// <summary>
        /// 소켓 정보를 가져올 수 있음.
        /// </summary>
        Socket Socket{ get; }
    }
}
