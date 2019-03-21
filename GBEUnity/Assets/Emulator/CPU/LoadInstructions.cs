using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.CPU
{
    class LoadInstructions
    {
        Register register;

        public LoadInstructions(Register register)
        {
            this.register = register;
        }

        public void Load(ref  int firstValue,int secondValue)
        {
            firstValue = secondValue;
        }

        public void LoadSPWithHL()
        {
            register.SP = register.HL;
        }

        public void LoadAFromImmediate()
        {
            //TODO: read memory
        }

        public void LoadAFromC()
        {
            //TODO: read Memory
        }

        public void LoadHLWithSPPlusImmediate()
        {
            //TODO: reaad memory
        }

        public void ReadByte(byte value,ushort address)
        {
            //TODO: reaad memory
        }
    }
}
