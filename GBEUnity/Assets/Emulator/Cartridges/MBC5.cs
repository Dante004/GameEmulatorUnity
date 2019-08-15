using UnityEngine;

namespace Emulator.Cartridges
{
    public class MBC5 : ICartridge
    {
        private int _selectedRomBank = 1;
        private int _selectedRamBank = 0;
        private readonly byte[,] _ram;
        private readonly byte[,] _rom;
        private bool _ramEnable = false;

        public MBC5(byte[] fileData, int romSize, int romBanks, int ramSize, int ramBanks)
        {
            var bankSize = romSize / romBanks;
            _rom = new byte[romBanks, bankSize];

            var ramBankSize = ramSize / ramBanks;
            _ram = new byte[ramBanks, ramBankSize];

            // Load the ROM
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
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (_ramEnable)
                {
                    return _ram[_selectedRamBank, address - 0xA000];
                }
                else
                {
                    return 0xFF;
                }
            }
            Debug.LogError($"Invalid cartridge read: {address:X}");
            return 0;
        }

        public void WriteByte(int address, int value)
        {
            if (address <= 0x1FFF)
            {
                _ramEnable = (value & 0x0F) == 0x0A;
            }
            else if (address >= 0x2000 && address <= 0x2FFF)
            {
                _selectedRomBank = (_selectedRomBank & 0x100) | value;
            }
            else if (address >= 0x3000 && address <= 0x3FFF)
            {
                _selectedRomBank = (_selectedRomBank & 0xFF) | (value & 1) << 8;
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                _selectedRamBank = value & 0x0F;
            }
            else if (address >= 0xA000 && address <= 0xBFFF && _ramEnable)
            {
                _ram[_selectedRamBank ,address - 0xA000] = (byte) value;
            }
            else
            {
                Debug.LogError($"Invalid cartridge write: {address:X}, {value:X}");
            }
        }
    }
}
