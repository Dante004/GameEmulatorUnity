using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.CPU
{
    class SaveInstructions
    {
        Register register;

        public SaveInstructions(Register register)
        {
            this.register = register;
        }

        public void SaveAToC()
        {
            //TODO: write memory 
        }

        public void SaveA()
        {
            register.PC += 2;
        }

        public void SaveWithOffset()
        {
            //TODO: write memory 
        }

        public void Swap(ushort value)
        {
            //TODO: write memory 
        }

        public void Swap(ref byte value)
        {
            value = (byte)(0xFF & ((value << 4) | (value >> 4)));
        }
    }
}
