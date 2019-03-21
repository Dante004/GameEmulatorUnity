using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.CPU
{
    class BitInstructions
    {
        Register register;

        public BitInstructions(Register register)
        {
            this.register = register;
        }

        public void SetBit(byte bit,ushort address)
        {
            //TODO: read memory
        }

        public void SetBit(byte bit,ref byte value)
        {
            value |= (byte)(1 << bit);
        }

        public void ResetBit(byte bit,ushort address)
        {
            //TODO: read memory
        }

        public void ResetBit(byte bit,ref byte value)
        {
            switch(bit)
            {
                case 0:
                    value &= 0xFE;
                    break;
                case 1:
                    value &= 0xFD;
                    break;
                case 2:
                    value &= 0xFB;
                    break;
                case 3:
                    value &= 0xF7;
                    break;
                case 4:
                    value &= 0xEF;
                    break;
                case 5:
                    value &= 0xDF;
                    break;
                case 6:
                    value &= 0xBF;
                    break;
                case 7:
                    value &= 0x7F;
                    break;
            }
        }

        public void TestBit(byte bit,ushort address)
        {
            //TODO: read memory
        }

        public void TestBit(byte bit,byte value)
        {
            RegisterFlags registerFlags = RegisterFlags.None;
            registerFlags |= RegisterFlags.H;
            if((value&(1<<bit))==0)
            {
                registerFlags |= RegisterFlags.Z;
            }
            register.SetFlags(registerFlags);
        }

    }
}
