using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.NET.Chip8
{
    public class Stack
    {
        ushort _sp;

        public ushort[] Data { get; private set; }

        public Stack()
        {
            //Reset stack pointer
            _sp = 0;
            Data = new ushort[16];
        }

        internal void Push(ushort _pc)
        {
            Data[_sp] = _pc;
            _sp++;
        }

        internal ushort Pop()
        {
            --_sp;
            return Data[_sp];
        }
    }
}
