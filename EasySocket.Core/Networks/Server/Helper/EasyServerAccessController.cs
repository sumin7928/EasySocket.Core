using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EasySocket.Core.Networks.Server.Helper
{
    internal class EasyServerAccessController
    {
        private readonly Semaphore _semaphoreAcceptedClients;

        internal EasyServerAccessController(int maxConnection)
        {
            _semaphoreAcceptedClients = new Semaphore(maxConnection, maxConnection);
        }

        internal void WaitOne()
        {
            _semaphoreAcceptedClients.WaitOne();
        }

        internal void Release()
        {
            _semaphoreAcceptedClients.Release();
        }
    }
}
