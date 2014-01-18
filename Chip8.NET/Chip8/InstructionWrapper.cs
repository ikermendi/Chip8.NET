using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.NET.Chip8
{
    public class InstructionWrapper
    {
        public string Address { get; set;}
        public string Instruction { get; set; }

        public InstructionWrapper(string address, string instruction)
        {
            this.Address = address;
            this.Instruction = instruction;
        }
    }
}
