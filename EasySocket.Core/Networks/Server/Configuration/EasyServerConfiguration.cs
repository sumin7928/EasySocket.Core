using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocket.Core.Networks.Server.Configuration
{
    public class EasyServerConfiguration
    {
        public int MaxConnection { get; set; } = 2000;
        public int ListenBackLog { get; set; } = 128;
        public int LingerTime { get; set; } = 2;
    }
}
