using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using EasySocket.Core.Networks.Support;
using EasySocket.Core.Options;
using Microsoft.Extensions.Logging;

namespace EasySocket.Core.Networks
{
    public interface IEasySocket
    {

        /// <summary>
        /// 소켓 정보를 가져올 수 있다.
        /// </summary>
        Socket Socket { get; }

        /// <summary>
        /// 소켓 Identity 값을 가져올 수 있다.
        /// </summary>
        string SocketId { get; }

        /// <summary>
        /// 소켓 옵션을 가져올 수 있다.
        /// </summary>
        SocketOptions SocketOptions { get; }

        /// <summary>
        /// 추가 소켓 저장 정보를 위한 아이템(object) 저장소를 가져올 수 있다.
        /// </summary>
        Dictionary<object, object> Items { get; }

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
        /// 비동기로 socket receive 처리 구현, action handler로 받은 데이터 그대로 넘겨준다.
        /// </summary>
        /// <param name="action"></param>
        void Receive(Action<byte[]> action);

        /// <summary>
        /// 비동기로 socket receive 처리 구현, action handler로 지정된 패킷 총 길이 만큼 byte[]을 받는다.
        /// </summary>
        /// <param name="totalLengthObject ">패킷 총길이에 따른 처리 설정 지정 오프젝트</param>
        /// <param name="receiveBuffer">패킷 byte[]을 받는 handler.</param>
        void Receive(TotalLengthObject totalLengthObject, Action<byte[]> receiveBuffer);

        /// <summary>
        /// 비동기로 socket send 처리 구현, action handler로 보낸 총 바이트 수를 받는다.
        /// </summary>
        /// <param name="sendData">보낼 byte[] 데이터.</param>
        void Send(byte[] sendData);

        /// <summary>
        /// 비동기로 socket send 처리 구현, action handler로 보낸 총 바이트 수를 받는다.
        /// </summary>
        /// <param name="sendData">보낼 byte[] 데이터.</param>
        /// <param name="offset">보낼 byte[]의 offset</param>
        /// <param name="size">보낼 byte[]의 길이</param>
        void Send(byte[] sendData, int offset, int size);

        /// <summary>
        /// 비동기로 socket send 처리 구현, action handler로 보낸 총 바이트 수를 받는다.
        /// </summary>
        /// <param name="sendData">보낼 byte[] 데이터.</param>
        /// <param name="length">보낸 패킷 사이즈를 받는 handler.</param>
        void Send(byte[] sendData, Action<int> length);

        /// <summary>
        /// 비동기로 socket send 처리 구현, action handler로 보낸 총 바이트 수를 받는다.
        /// </summary>
        /// <param name="sendData">보낼 byte[] 데이터.</param>
        /// <param name="offset">보낼 byte[]의 offset</param>
        /// <param name="size">보낼 byte[]의 길이</param>
        /// <param name="length">보낸 패킷 사이즈를 받는 handler.</param>
        void Send(byte[] sendData, int offset, int size, Action<int> length);

        /// <summary>
        /// 소켓을 종료시킨다. ( Shutdown 진행 )
        /// </summary>
        void Close();

    }
}
