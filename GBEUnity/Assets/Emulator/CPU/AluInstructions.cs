using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.CPU
{
    class AluInstructions
    {

        Register register;

        public AluInstructions(Register register)
        {
            this.register = register;
        }

        /// <summary>
        /// Add two value 8-bit
        /// </summary>
        /// <param name="firstValue"></param>
        /// <param name="secondValue"></param>
        /// <param name="affectedFlags"></param>
        /// <param name="setFlags"></param>
        /// <param name="resetFlags"></param>
        public void Add(ref byte firstValue,byte secondValue,RegisterFlags affectedFlags = RegisterFlags.None,RegisterFlags setFlags = RegisterFlags.None,RegisterFlags resetFlags = RegisterFlags.None)
        {
            register.ClearFlags(resetFlags);
            register.SetFlags(setFlags);
            firstValue += secondValue;
            var setAffected = RegisterFlags.None;
            if ((firstValue & 0x0F) + (secondValue & 0x0F) > 0x0F) setAffected |= RegisterFlags.H;
            if (firstValue > 0xFF) setAffected |= RegisterFlags.C;
            firstValue &= 0xFF;
            if (firstValue == 0) setAffected |= RegisterFlags.Z;
            register.ClearFlags(affectedFlags);
            register.SetFlags(setAffected & affectedFlags);
        }

        /// <summary>
        /// Add two value 16-bit
        /// </summary>
        /// <param name="firstValue"></param>
        /// <param name="secondValue"></param>
        /// <param name="affectedFlags"></param>
        /// <param name="setFlags"></param>
        /// <param name="resetFlags"></param>
        public void Add(ref ushort firstValue,ushort secondValue, RegisterFlags affectedFlags = RegisterFlags.None, RegisterFlags setFlags = RegisterFlags.None, RegisterFlags resetFlags = RegisterFlags.None)
        {
            register.ClearFlags(resetFlags);
            register.SetFlags(setFlags);
            firstValue += secondValue;
            var setAffected = RegisterFlags.None;
            if ((firstValue & 0x0F) + (secondValue & 0x0F) > 0x0F) setAffected |= RegisterFlags.H;
            if (firstValue > 0xFF) setAffected |= RegisterFlags.C;
            firstValue &= 0xFF;
            if (firstValue == 0) setAffected |= RegisterFlags.Z;
            register.ClearFlags(affectedFlags);
            register.SetFlags(setAffected & affectedFlags);
        }


        /// <summary>
        /// Add 8bit value to 16 bit value
        /// </summary>
        /// <param name="firstValue"></param>
        /// <param name="secondValue"></param>
        /// <param name="affectedFlags"></param>
        /// <param name="setFlags"></param>
        /// <param name="resetFlags"></param>
        public void Add(ref ushort firstValue, sbyte secondValue, RegisterFlags affectedFlags = RegisterFlags.None, RegisterFlags setFlags = RegisterFlags.None, RegisterFlags resetFlags = RegisterFlags.None)
        {
            register.ClearFlags(resetFlags);
            register.SetFlags(setFlags);
            firstValue += (ushort)secondValue;
            var setAffected = RegisterFlags.None;
            if ((firstValue & 0x0F) + (secondValue & 0x0F) > 0x0F) setAffected |= RegisterFlags.H;
            if (firstValue > 0xFF) setAffected |= RegisterFlags.C;
            firstValue &= 0xFF;
            if (firstValue == 0) setAffected |= RegisterFlags.Z;
            register.ClearFlags(affectedFlags);
            register.SetFlags(setAffected & affectedFlags);
        }

        public void Sub(ref ushort firstValue, ushort secondValue, RegisterFlags affectedFlags = RegisterFlags.None, RegisterFlags setFlags = RegisterFlags.None, RegisterFlags resetFlags = RegisterFlags.None)
        {
            register.ClearFlags(resetFlags);
            register.SetFlags(setFlags);
            firstValue -= secondValue;
            var setAffected = RegisterFlags.None;
            if ((firstValue & 0x0F) < (secondValue & 0x0F)) setAffected |= RegisterFlags.H;
            if (secondValue > firstValue) setAffected |= RegisterFlags.C;
            firstValue &= 0xFF;
            if (firstValue == 0) setAffected |= RegisterFlags.Z;
            register.ClearFlags(affectedFlags);
            register.SetFlags(setAffected & affectedFlags);
        }

        public void Sub(ref byte firstValue, byte secondValue, RegisterFlags affectedFlags = RegisterFlags.None, RegisterFlags setFlags = RegisterFlags.None, RegisterFlags resetFlags = RegisterFlags.None)
        {
            register.ClearFlags(resetFlags);
            register.SetFlags(setFlags);
            firstValue -= secondValue;
            var setAffected = RegisterFlags.None;
            if ((firstValue & 0x0F) < (secondValue & 0x0F)) setAffected |= RegisterFlags.H;
            if (secondValue > firstValue) setAffected |= RegisterFlags.C;
            firstValue &= 0xFF;
            if (firstValue == 0) setAffected |= RegisterFlags.Z;
            register.ClearFlags(affectedFlags);
            register.SetFlags(setAffected & affectedFlags);
        }

        public void Adc(ref byte firstValue, byte secondValue, RegisterFlags affectedFlags = RegisterFlags.None, RegisterFlags setFlags = RegisterFlags.None, RegisterFlags resetFlags = RegisterFlags.None)
        {
            register.ClearFlags(resetFlags);
            register.SetFlags(setFlags);
            firstValue += (byte)(secondValue + (register.GetFlag(RegisterFlags.C) ? 1 : 0));
            var setAffected = RegisterFlags.None;
            if ((firstValue & 0x0F) < (secondValue & 0x0F)) setAffected |= RegisterFlags.H;
            if (secondValue > firstValue) setAffected |= RegisterFlags.C;
            firstValue &= 0xFF;
            if (firstValue == 0) setAffected |= RegisterFlags.Z;
            register.ClearFlags(affectedFlags);
            register.SetFlags(setAffected & affectedFlags);
        }

        public void Sdc(ref byte firstValue, byte secondValue, RegisterFlags affectedFlags = RegisterFlags.None, RegisterFlags setFlags = RegisterFlags.None, RegisterFlags resetFlags = RegisterFlags.None)
        {
            register.ClearFlags(resetFlags);
            register.SetFlags(setFlags);
            firstValue -= (byte)(secondValue + (register.GetFlag(RegisterFlags.C) ? 1 : 0));
            var setAffected = RegisterFlags.None;
            if ((firstValue & 0x0F) < (secondValue & 0x0F)) setAffected |= RegisterFlags.H;
            if (secondValue > firstValue) setAffected |= RegisterFlags.C;
            firstValue &= 0xFF;
            if (firstValue == 0) setAffected |= RegisterFlags.Z;
            register.ClearFlags(affectedFlags);
            register.SetFlags(setAffected & affectedFlags);
        }

        public byte And(byte value)
        {
            byte result = (byte)((register.A & value)&0xFF);
            register.OverwriteFlagsa(RegisterFlags.H | (result == 0 ? RegisterFlags.Z : RegisterFlags.None));
            return result;
        }

        public byte Or(byte value)
        {
            byte result = (byte)((register.A & value) & 0xFF);
            register.OverwriteFlagsa((result == 0 ? RegisterFlags.Z : RegisterFlags.None));
            return result;
        }

        public byte Xor(byte value)
        {
            byte result = (byte)((register.A ^ value) & 0xFF);
            register.OverwriteFlagsa((result == 0 ? RegisterFlags.Z : RegisterFlags.None));
            return result;
        }
    }
}
