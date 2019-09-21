namespace Emulator.Processor
{
    public class Registers
    {
        private byte f;
        public byte F
        {
            get => f;
            set => f = (byte)(value & 0xF0);
        }

        public byte A, B, C, D, E, H, L;
        public ushort PC;
        public ushort SP;

        public ushort AF
        {
            get => (ushort)((A << 8) + F);
            set { A = (byte)(value >> 8 & 0x00FF); F = (byte)(value & 0x00FF); }
        }
        public ushort BC
        {
            get => (ushort)((B << 8) + C);
            set { B = (byte)(value >> 8 & 0x00FF); C = (byte)(value & 0x00FF); }
        }
        public ushort DE
        {
            get => (ushort)((D << 8) + E);
            set { D = (byte)((value & 0xFF00) >> 8); E = (byte)(value & 0x00FF); }
        }
        public ushort HL
        {
            get => (ushort)((H << 8) + L);
            set { H = (byte)(value >> 8 & 0x00FF); L = (byte)(value & 0x00FF); }
        }

        public bool flagZ
        {
            set => F = (byte)(value ? F | 0x80 : F & (~0x80));
            get => ((F & 0x80) != 0x00);
        }
        public bool flagN
        {
            set => F = (byte)(value ? F | 0x40 : F & (~0x40));
            get => ((F & 0x40) != 0x00);
        }
        public bool flagH
        {
            set => F = (byte)(value ? F | 0x20 : F & (~0x20));
            get => ((F & 0x20) != 0x00);
        }
        public bool flagC
        {
            set => F = (byte)(value ? F | 0x10 : F & (~0x10));
            get => ((F & 0x10) != 0x00);
        }
    }
}
