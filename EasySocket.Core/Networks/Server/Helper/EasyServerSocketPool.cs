using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocket.Core.Networks.Server.Helper
{
    internal class EasyServerSocketPool
    {
        private int _count;
        private readonly Stack<IEasyServerSocket> _pool;

        internal EasyServerSocketPool(int capacity)
        {
            _count = capacity;
            _pool = new Stack<IEasyServerSocket>(capacity);
        }

        internal IEasyServerSocket Pop()
        {
            lock (_pool)
            {
                if (_pool.Count > 0)
                {
                    ++_count;
                    return _pool.Pop();
                }
                else
                {
                    return null;
                }
            }
        }

        internal void Push(IEasyServerSocket item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
            }
            lock (_pool)
            {
                --_count;
                _pool.Push(item);
            }
        }

        internal int GetCount()
        {
            return _count;
        }

        internal void Clear()
        {

        }
    }
}
