
namespace Emulator.CPU
{
    class AluInstructions
    {

        private readonly Register _register;

        public AluInstructions(Register register)
        {
            _register = register;
        }

        public void Or(byte value)
        {
            _register.A = (byte)(0xFF & (_register.A | value));
            if(_register.A==0)
            {
                _register.SetFlags(RegisterFlags.Z);
            }
            _register.ClearFlags(RegisterFlags.C | RegisterFlags.H | RegisterFlags.N);
        }

        public void Or(ushort address)
        {
            //TODO: memory read
        }

        public void OrImmediate()
        {
            //TODO: memory read
        }

        public void Xor(byte value)
        {
            _register.A = (byte)(0xFF & (_register.A ^ value));
            if (_register.A == 0)
            {
                _register.SetFlags(RegisterFlags.Z);
            }
            _register.ClearFlags(RegisterFlags.C | RegisterFlags.H | RegisterFlags.N);
        }

        public void Xor(ushort address)
        {
            //TODO: memory read
        }

        public void Xor()
        {
            //TODO: memory read
        }

        public void And(byte value)
        {
            _register.SetFlags(RegisterFlags.H);
            _register.A = (byte)(0xFF & (_register.A & value));
            _register.ClearFlags(RegisterFlags.C | RegisterFlags.N);
            if (_register.A == 0)
            {
                _register.SetFlags(RegisterFlags.Z);
            }
        }

        public void Add(byte value)
        {
            RegisterFlags registerToSet = RegisterFlags.None;
            if((_register.A & 0x0F)+(value&0x0F)>0x0F)
            {
                registerToSet |= RegisterFlags.H;
            }
            _register.A += value;
            if (_register.A > 0xFF) registerToSet |= RegisterFlags.C;
            _register.A &= 0xFF;
            if (_register.A == 0) registerToSet |= RegisterFlags.Z;
            _register.SetFlags(registerToSet);
        }

        public void Add(ref ushort firstValue,ushort secondValue)
        {
            //TODO: add 16 bit
        }

        public void Add(ushort address)
        {
            //TODO: memory read
        }

        public void AddSPtoHl()
        {
            //TODO: Add(ref _register.HL, _register.SP);
        }

        public void AddImmediate()
        {
            //TODO: memory read
        }

        public void AddWithCarry(byte value)
        {
            RegisterFlags registerToSet = RegisterFlags.None;
            byte carry = _register.GetFlag(RegisterFlags.C) ? (byte)1 : (byte)0;
            if(carry + (_register.A&0x0F)+(value&0x0F)>0x0F)
            {
                registerToSet |= RegisterFlags.H;
            }
            _register.A += (byte)(value + carry);
            if (_register.A > 0xFF) registerToSet |= RegisterFlags.C;
            _register.A &= 0xFF;
            if (_register.A == 0) registerToSet |= RegisterFlags.Z;
            _register.SetFlags(registerToSet);
        }

        public void AddWithCarry(ushort address)
        {
            //TODO: memory read
        }

        public void AddWithCarryImmediate()
        {
            //TODO: memory read
        }

        public void Sub(byte value)
        {
            RegisterFlags registerToSet = RegisterFlags.None;
            if((_register.A & 0x0F) < (value & 0x0F))
            {
                registerToSet |= RegisterFlags.H;
            }
            if (value > _register.A) registerToSet |= RegisterFlags.C;
            _register.A -= value;
            _register.A &= 0xFF;
            if (_register.A == 0) registerToSet |= RegisterFlags.Z;
            _register.SetFlags(registerToSet | RegisterFlags.N);
        }

        public void SubImmediate()
        {
            //TODO: memory read
        }

        public void Sub(ushort address)
        {
            //TODO: address
        }

        public void ShiftLeft(ref byte value)
        {
            RegisterFlags registerToSet = RegisterFlags.None;
            if (value > 0x7F) registerToSet |= RegisterFlags.H;
            value = (byte)(0xFF & (value << 1));
            if (value == 0) registerToSet |= RegisterFlags.Z;
            _register.SetFlags(registerToSet);
        }

        public void ShiftLeft(ushort address)
        {
            //TODO: read nad write memory
        }

        public void UnsignedShiftRight(ref byte value)
        {
            RegisterFlags registerToSet = RegisterFlags.None;
            if ((value & 0x01) == 1) registerToSet |= RegisterFlags.H;
            value >>= 1;
            if (value == 0) registerToSet |= RegisterFlags.Z;
            _register.SetFlags(registerToSet);
        }

        public void UnsignedShiftRight(ushort address)
        {
            //TODO: read and write memory
        }

        public void SignedShiftRight(ref byte value)
        {
            RegisterFlags registerToSet = RegisterFlags.None;
            if ((value & 0x01) == 1) registerToSet |= RegisterFlags.H;
            value = (byte)((value & 0x80) | (value >> 1));
            if (value == 0) registerToSet |= RegisterFlags.Z;
            _register.SetFlags(registerToSet);
        }

        public void SignedShiftRight(ushort address)
        {
            //TODO: read and write memory
        }

        public void DecrementWord(ref ushort value)
        {
            if(value == 0)
            {
                value = 0xFFFF;
            }
            else
            {
                value --;
            }
        }

        public void IncrementWord(ref ushort value)
        {
            if(value == 0xFFFF)
            {
                value = 0x0000;
            }
            else
            {
                value++;
            }
        }

        public void Increment(ref byte value)
        {
            RegisterFlags registerToSet = RegisterFlags.None;
            if ((value & 0x0F0) == 0x0F) registerToSet |= RegisterFlags.H;
            value++;
            value &= 0xFF;
            if (value == 0) registerToSet |= RegisterFlags.Z;
            _register.SetFlags(registerToSet);
        }

        public void Decrement(ref byte value)
        {
            RegisterFlags registerToSet = RegisterFlags.None;
            if ((value & 0x0F0) == 0x00) registerToSet |= RegisterFlags.H;
            value--;
            value &= 0xFF;
            if (value == 0) registerToSet |= RegisterFlags.Z;
            _register.SetFlags(registerToSet | RegisterFlags.N);
        }

        public void Increment(ref byte hightValue,ref byte lowValue)
        {
            if (lowValue == 0xFF)
            {
                hightValue = (byte) (0xFF & (hightValue + 1));
                lowValue = 0;
            }
            else
            {
                lowValue++;
            }
        }

        public void Decrement(ref byte hightValue,ref byte lowValue)
        {
            if (lowValue == 0)
            {
                hightValue =(byte) (0xFF & (hightValue - 1));
                lowValue = 0xFF;
            }
            else
            {
                lowValue--;
            }
        }

        public void IncrementMemory()
        {
            //TODO: read and write memory
        }

        public void DecrementMemory()
        {
            //TODO: read and write memory
        }


        public void Compare(ushort address)
        {
            //TODO: read memory
        }

    }
}
