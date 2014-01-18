using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.NET.Chip8
{
    public class Memory
    {
        private static byte[] Chip8Fontset = new byte[80] { 
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        public int Size
        {
            get
            {
                return 4096;
            }
        }

        public byte[] Data { get; private set; }

        public Memory()
        {
            Data = new byte[Size];

            //Load fonts
            for (int i = 0; i < 80; i++)
            {
                Data[i] = Chip8Fontset[i];
            }
        }

        private void LoadFontSet()
        {
            
        }

        internal void SetData(int location, byte data)
        {
            Data[location] = data;
        }

        internal byte Get(ushort position)
        {
            return Data[position];
        }
    }
}
