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

        private void JumpIfCarry()
        {
            if (_register.GetFlag(RegisterFlags.C))
            {
                Jump();
            }
            else
            {
                _register.PC += 2;
            }
        }

        private void JumpIfNotCarry()
        {
            if (_register.GetFlag(RegisterFlags.C))
            {
                _register.PC += 2;
            }
            else
            {
                Jump();
            }
        }

        private void JumpIfZero()
        {
            if (_register.GetFlag(RegisterFlags.Z))
            {
                Jump();
            }
            else
            {
                _register.PC += 2;
            }
        }

        private void JumpIfNotZero()
        {
            if (_register.GetFlag(RegisterFlags.Z))
            {
                _register.PC += 2;
            }
            else
            {
                Jump();
            }
        }

        private void Jump(ushort address)
        {
            _register.PC = address;
        }

        private void Jump()
        {
            //TODO: read memory
        }
    }
}
