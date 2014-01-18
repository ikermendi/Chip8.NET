using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.NET.Chip8
{
    public class Graphic
    {
        private byte[] _screen;

        public Graphic()
        {
            _screen = new byte[64 * 32];
            Clear();
        }

        public void Draw(int position)
        {
            _screen[position] ^= 1;
        }

        public byte Get(int position)
        {
            return _screen[position];
        }

        public byte[] GetScreen()
        {
            return _screen;
        }

        public void Clear()
        {
            for (int i = 0; i < _screen.Length; i++)
            {
                _screen[i] = 0;
            }
        }
    }
}
