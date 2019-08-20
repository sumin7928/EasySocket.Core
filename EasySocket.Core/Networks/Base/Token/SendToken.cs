using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocket.Core.Networks.Base.Token
{
    class SendToken
    {
        public Action<int> SendHandler { get; set; }
    }
}
