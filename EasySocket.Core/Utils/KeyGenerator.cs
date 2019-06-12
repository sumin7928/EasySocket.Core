using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EasySocket.Core.Utils
{
    public static class KeyGenerator
    {
        private static int incrementNo;

        public static string GetClientSocketId()
        {
            int number = Interlocked.Increment(ref incrementNo) % 10;
            if (number < 0)
            {
                number = -number;
            }

            return $"C{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()}{number}";
        }

        public static string GetServerSocketId()
        {
            int number = Interlocked.Increment(ref incrementNo) % 10;
            if (number < 0)
            {
                number = -number;
            }

            return $"S{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()}{number}";
        }
    }
}
