using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.CPU
{
    class JumpInstructions
    {
        private readonly Register _register;

        public JumpInstructions(Register register)
        {
            _register = register;
        }
    }
}
