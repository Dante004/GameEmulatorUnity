using UnityEngine;

namespace Emulator.Cartridges
{
    public class MBC1 : ICartridge
    {
        private const int RamBank = 4;
        private const int RamBankSize = 8192;
        private bool _ramBankingMode;
        private int _selectedRomBank = 1;
        private int _selectedRamBank;
        private readonly byte[,] _ram = new byte[RamBank, RamBankSize];
        private readonly byte[,] _rom;

        public MBC1(byte[] fileData, int romSize, int romBanks)
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
            if (address >= 0x4000 && address <= 0x7FFF)
            {
                return _rom[_selectedRomBank, address - 0x4000];
            }
            if (address >= 0xA000 && address <= 0xBFFF)
            {
                return _ram[_selectedRamBank, address - 0xA000];
            }
            Debug.LogError($"Invalid cartridge read: {address:X}");
            return 0;
        }

        public void WriteByte(int address, int value)
        {
            if (address >= 0xA000 && address <= 0xBFFF)
            {
                _ram[_selectedRamBank, address - 0xA000] = (byte)(0xFF & value);
            }
            else if (address >= 0x6000 && address <= 0x7FFF)
            {
                _ramBankingMode = (value & 0x01) == 0x01;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                var selectedRomBankLow = 0x1F & value;
                if (selectedRomBankLow == 0x00)
                {
                    selectedRomBankLow++;
                }
                _selectedRomBank = (_selectedRomBank & 0x60) | selectedRomBankLow;
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                if (_ramBankingMode)
                {
                    _selectedRamBank = 0x03 & value;
                }
                else
                {
                    _selectedRomBank = (_selectedRomBank & 0x1F) | ((0x03 & value) << 5);
                }
            }
        }
    }
}
