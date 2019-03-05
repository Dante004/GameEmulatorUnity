using System.Collections;
using System.Collections.Generic;

namespace Emulator.CPU
{
    public class Register
    {
        public byte A;
        public byte F;
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte H;
        public byte L;
        public ushort PC;
        public ushort SP;


        public ushort AF
        {
            get
            {
                return (ushort)((A << 8) | F);
            }
            set
            {
                A = (byte)((value >> 8) & 0xFF);
                F = (byte)(value & 0xF0);
            }
        }

        public ushort BC
        {
            get
            {
                return (ushort)((B << 8) | C);
            }
            set
            {
                B = (byte)((value >> 8) & 0xFF);
                C = (byte)(value & 0xFF);
            }
        }

        public ushort DE
        {
            get
            {
                return (ushort)((D << 8) | E);
            }
            set
            {
                D = (byte)((value >> 8) & 0xFF);
                E = (byte)(value & 0xFF);
            }
        }

        public ushort HL
        {
            get
            {
                return (ushort)((H << 8) | L);
            }
            set
            {
                H = (byte)((value >> 8) & 0xFF);
                L = (byte)((value & 0xFF));
            }
        }

        public void SetFlags(RegisterFlags flags)
        {
            F |= (byte)flags;
        }

        public void ClearFlags(RegisterFlags flags)
        {
            F &= (byte)~flags;
        }

        public void OverwriteFlagsa(RegisterFlags flags)
        {
            F = (byte)flags;
        }

        public bool GetFlag(RegisterFlags flag)
        {
            return (F & (byte)flag) == (byte)flag;
        }

        public void ResetRegister()
        {
            AF = 0x01B0;
            BC = 0x0013;
            DE = 0x00D8;
            HL = 0x014D;
            SP = 0xFFFE;
            PC = 0x100;
        }
    }
}


