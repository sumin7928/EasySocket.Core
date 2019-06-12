using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocket.Core.Networks.Support
{
    public class TotalLengthObject
    {
        public int TotalLengthOffset { get; set; } = 0;
        public int TotalLengthSize { get; set; } = 4;
        public bool IsBigEndian { get; set; } = false;
    }
}
