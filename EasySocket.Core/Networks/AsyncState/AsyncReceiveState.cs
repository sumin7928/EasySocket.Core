using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EasySocket.Core.Networks.AsyncState
{
    class AsyncReceiveState
    {
        public const int ChunkSize = 2048;

        public int ChunkBufferOffset = 0;

        public Socket AsyncSocket { set; get; }

        public Action<byte[]> ReceiveBuffer { set; get; }

        public int Offset { set; get; }

        public int Length { set; get; }

        public byte[] ChunkBuffer { set; get; }

        public AsyncReceiveState()
        {
            ChunkBuffer = new byte[ChunkSize];
        }

        public void DoAction( int receivedSize )
        {
            ChunkBufferOffset += receivedSize;

            // 총 데이터 사이즈 파싱 처리일 경우 
            if (Length - Offset > 0)
            {
                byte[] sizeByte = new byte[Length - Offset];
                Array.Copy(ChunkBuffer, sizeByte, sizeByte.Length);

                // LittleEndian 계열의 Cpu를 사용하는 머신일 경우
                // 클라이언트에서 그냥 네트워크 바이트 배열을 리틀앤드안으로 보낼것으로 예상..
                //if(BitConverter.IsLittleEndian)
                //{
                //    Array.Reverse(sizeByte);
                //}

                int size = BitConverter.ToInt32(sizeByte, 0);

                if(ChunkBufferOffset >= size)
                {
                    byte[] respose = new byte[size];
                    Array.Copy(ChunkBuffer, respose, respose.Length);

                    if(ChunkBufferOffset > size)
                    {
                        // ChunkBufferOffset 재정렬
                        int remainOffset = ChunkBufferOffset - size;
                        byte[] temp = new byte[ChunkSize];
                        Array.Copy(ChunkBuffer, temp, remainOffset);

                        Array.Clear(ChunkBuffer, 0, ChunkSize);
                        Array.Copy(temp, ChunkBuffer, remainOffset);
                        ChunkBufferOffset = remainOffset;

                        // 다시 파싱 시도
                        DoAction(0);
                        
                    } else
                    {
                        Array.Clear(ChunkBuffer, 0, ChunkSize);
                        ChunkBufferOffset = 0;
                    }
                    ReceiveBuffer(respose);
                }
            }
            // 일반 패킷 처리일 경우
            else
            {
                byte[] respose = new byte[ChunkBufferOffset];
                Array.Copy(ChunkBuffer, respose, respose.Length);
                Array.Clear(ChunkBuffer, 0, ChunkSize);
                ChunkBufferOffset = 0;
                ReceiveBuffer(respose);

            }
        }
    }
}
