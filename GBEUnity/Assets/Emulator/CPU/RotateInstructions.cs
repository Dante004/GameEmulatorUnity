
namespace Emulator.CPU
{
    class RotateInstructions
    {
        private readonly Register _register;

        public RotateInstructions(Register register)
        {
            _register = register;
        }

        public void RotateARight()
        {
            RegisterFlags registerToSet = RegisterFlags.None;
            byte lowBit = (byte)(_register.A & 0x01);
            if (lowBit == 1)
            {
                registerToSet |= RegisterFlags.C;
            }

            _register.A = (byte)((_register.A >> 1) | (lowBit << 7));
            _register.SetFlags(registerToSet);
            _register.ClearFlags(RegisterFlags.H | RegisterFlags.N);

        }

        public void RotateARightThroughCarry()
        {
            byte highBit = _register.GetFlag(RegisterFlags.C) ? (byte)0x80 : (byte)0x00;
            if ((_register.A & 0x01) == 0x01) _register.SetFlags(RegisterFlags.C);
            _register.A = (byte)(highBit | (_register.A >> 1));
            _register.ClearFlags(RegisterFlags.H|RegisterFlags.N);
        }

        public void RotateALeftThroughCarry()
        {
            byte highBit = _register.GetFlag(RegisterFlags.C) ? (byte)1 : (byte)0;
            if(_register.A > 0x7F) _register.SetFlags(RegisterFlags.C);
            _register.A = (byte)(((_register.A << 1) & 0xFF) | highBit);
            _register.ClearFlags(RegisterFlags.H | RegisterFlags.N);
        }

        private void RotateRight(ref byte value)
        {
            byte lowBit = (byte)(value & 0x01);
            if(lowBit == 1) _register.SetFlags(RegisterFlags.C);
            value = (byte)((value >> 1) | (lowBit << 7));
            if(value == 0) _register.SetFlags(RegisterFlags.Z);
            _register.ClearFlags(RegisterFlags.H | RegisterFlags.N);
        }

        private void RotateRightThroughCarry(ushort address)
        {
            //TODO: read and write memory
        }

        private void RotateRightThroughCarry(ref byte value)
        {
            byte lowBit = _register.GetFlag(RegisterFlags.C) ? (byte)0x80 : (byte)0;
            if((value & 0x01) == 1) _register.SetFlags(RegisterFlags.C);
            value = (byte)((value >> 1) | lowBit);
            if(value == 0) _register.SetFlags(RegisterFlags.Z);
            _register.ClearFlags(RegisterFlags.H | RegisterFlags.N);
        }

        private void RotateLeftThroughCarry(ushort address)
        {
            //TODO: read and write memory
        }

        private void RotateLeftThroughCarry(ref byte value)
        {
            byte highBit = _register.GetFlag(RegisterFlags.C) ? (byte)1 : (byte)0;
            if((value >> 7) == 1) _register.SetFlags(RegisterFlags.C);
            if (value == 0) _register.SetFlags(RegisterFlags.Z);
            _register.ClearFlags(RegisterFlags.H | RegisterFlags.N);
        }

        private void RotateLeft(ushort address)
        {
            //TODO: read and write memory
        }

        private void RotateRight(ushort address)
        {
            //TODO: read and write memory
        }

        private void RotateLeft(ref byte value)
        {
            byte highBit = (byte)(value >> 7);
            if(highBit == 1) _register.SetFlags(RegisterFlags.C);
            value = (byte)(((value << 1) & 0xFF) | highBit);
            if (value == 0) _register.SetFlags(RegisterFlags.Z);
            _register.ClearFlags(RegisterFlags.H | RegisterFlags.N);
        }

        private void RotateALeft()
        {
            byte highBit = (byte)(_register.A >> 7);
            if(highBit == 1) _register.SetFlags(RegisterFlags.C);
            _register.A = (byte)(((_register.A << 1) & 0xFF) | highBit);
            _register.ClearFlags(RegisterFlags.H | RegisterFlags.N);
        }
    }
}
