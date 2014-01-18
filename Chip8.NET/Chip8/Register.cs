using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.NET.Chip8
{
    public class Register
    {
        public string Name { get; private set; }

        public byte Reg { get; set; }

        public Register(string name)
        {
            Name = name;
            Reg = 0;
        }
    }
}
