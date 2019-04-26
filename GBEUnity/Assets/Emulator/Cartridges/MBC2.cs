using UnityEngine;

namespace Emulator.Cartridges
{
    public class MBC2 : ICartridge
    {
        private const int RamSize = 512;
        private int _selectedRomBank = 1;
        private readonly byte[] _ram = new byte[RamSize];
        private readonly byte[,] _rom;

        public MBC2(byte[] fileData, int romSize, int romBanks)
        { 
            var bankSize = romSize / romBanks;
            _rom = new byte[romBanks, bankSize];
            for (int i = 0, k = 0; i < romBanks; ++i)
            {
                for (var j = 0; j < bankSize; ++j, ++k)
                {
                    _rom[i, j] = fileData[k];
                }
            }
        }

        public int ReadByte(int address)
        {
            if (address <= 0x3FFF)
            {
                return _rom[0, address];
            }
            else if (address >= 0x4000 && address <= 0x7FFF)
            {
                return _rom[_selectedRomBank, address - 0x4000];
            }
            else if (address >= 0xA000 && address <= 0xA1FF)
            {
                return _ram[address - 0xA000];
            }
            Debug.LogError($"Invalid cartridge read: {address:X}");
            return 0;
        }

        public void WriteByte(int address, int value)
        {
            if (address >= 0xA000 && address <= 0xA1FF)
            {
                _ram[address - 0xA000] = (byte)(0x0F & value);
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                _selectedRomBank = 0x0F & value;
            }
        }
    }
}
