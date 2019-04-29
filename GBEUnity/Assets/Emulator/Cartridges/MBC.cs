using System.Collections.Generic;

namespace Emulator.Cartridges
{
    public abstract class Mbc : ICartridge
    {
        protected int ramBank;
        protected int ramBankSize;
        protected int selectedRomBank;
        protected int selectedRamBank;
        protected byte[,] ram;
        protected byte[,] rom;

        protected Mbc(int romBanks, int romSize, IReadOnlyList<byte> fileData)
        {
            LoadRom(romBanks, romSize, fileData);
        }

        private void LoadRom(int romBanks, int romSize, IReadOnlyList<byte> fileData)
        {
            var bankSize = romSize / romBanks;
            rom = new byte[romBanks, bankSize];

            for (int i = 0, k = 0; i < romBanks; ++i)
            {
                for (var j = 0; j < bankSize; ++j, ++k)
                {
                    rom[i, j] = fileData[k];
                }
            }
        }

        public virtual int ReadByte(int address)
        {
            throw new System.NotImplementedException();
        }

        public virtual void WriteByte(int address, int value)
        {
            throw new System.NotImplementedException();
        }
    }
}
