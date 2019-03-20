using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.CPU
{
    class WriteInstructions
    {
        private readonly Register _register;

        public WriteInstructions(Register register)
        {
            _register = register;
        }

        public void WriteByte(ushort address)
        {
            //TODO: write memory
        }

        public void WriteWordToImediateAddress(byte value)
        {

        }
    }
}
