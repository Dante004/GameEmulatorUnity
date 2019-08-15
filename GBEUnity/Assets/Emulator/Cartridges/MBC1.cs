using UnityEngine;

namespace Emulator.Cartridges
{
    public class MBC1 : ICartridge
    {
        private bool _ramBankingMode;
        private int _selectedRomBank = 1;
        private int _selectedRamBank;
        private readonly int _romBanks;
        private readonly int _ramBanks;
        private readonly byte[,] _ram;
        private readonly byte[,] _rom;
        private bool _ramEnable;

        public MBC1(byte[] fileData, int romSize, int romBanks, int ramSize, int ramBanks)
        {
            _ramBankingMode = false;
            var romBankSize = romSize / romBanks;
            _rom = new byte[romBanks, romBankSize];
            if (ramSize != 0 || ramBanks != 0)
            {
                var ramBankSize = ramSize / ramBanks;
                _ram = new byte[ramBanks, ramBankSize];
            }
            else
            {
                _ram = new byte[0,0];
            }
            _ramEnable = false;
            for (int i = 0, k = 0; i < romBanks; ++i)
            {
                for (var j = 0; j < romBankSize; ++j, ++k)
                {
                    _rom[i, j] = fileData[k];
                }
            }

            _romBanks = romBanks;
            _ramBanks = ramBanks;
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
                if (_ramEnable)
                {
                    return _ram[_selectedRamBank, address - 0xA000];
                }
                else
                {
                    Debug.LogError($"Attempting read on ram when ram is disabled: {address:X}");
                    return 0xFF;
                }
            }
            Debug.LogError($"Invalid cartridge read: {address:X}");
            return 0;
        }

        public void WriteByte(int address, int value)
        {
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                _ramEnable = (value & 0x0F) == 0x0A;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                if (!_ramBankingMode)
                {
                    SelectRomBank((value & 0x1F) | (0x03 & value) << 5);
                }
                else
                {
                    SelectRomBank(value & 0x1F);
                }
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                if (_ramBankingMode)
                {
                    _selectedRamBank = 0x03 & value;
                    _selectedRamBank &= _ramBanks - 1;
                }
                else
                {
                    SelectRomBank((value & 0x1F) | (0x03 & value) << 5);
                }
            }
            else if (address >= 0x6000 && address <= 0x7FFF)
            {
                _ramBankingMode = (value & 0x01) == 0x01;
            }
            if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (_ramEnable)
                {
                    _ram[_selectedRamBank, address - 0xA000] = (byte)(0xFF & value);
                }
                else
                {
                    Debug.LogError($"Attempting write on ram when ram is disabled: {address:X}, {value:X}");
                }
            }
            Debug.LogError($"Invalid cartridge write: {address:X}, {value:X}");
        }

        private void SelectRomBank(int value)
        {
            var selectedRomBankLow = value;
            if (selectedRomBankLow == 0x00 ||
                selectedRomBankLow == 0x20 ||
                selectedRomBankLow == 0x40 ||
                selectedRomBankLow == 0x60)
            {
                selectedRomBankLow++;
            }

            _selectedRomBank = selectedRomBankLow;
            _selectedRomBank &= _romBanks - 1;
        }
    }
}
