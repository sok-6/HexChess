using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core.AI
{
    public class ThreadSafeString
    {
        private object _locker;
        private string _data;

        public ThreadSafeString()
        {
            _locker = new object();
            _data = "";
        }

        public void Write(string newString)
        {
            lock (_locker)
            {
                _data = newString;
            }
        }

        public string Read()
        {
            lock (_locker)
            {
                return _data;
            }
        }
    }
}
