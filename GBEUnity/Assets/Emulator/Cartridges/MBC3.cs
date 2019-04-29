using System;
using UnityEngine;

namespace Emulator.Cartridges
{
    public class MBC3 : ICartridge
    {
        private const int RamBank = 4;
        private const int RamBankSize = 8192;
        private int _selectedRomBank = 1;
        private int _selectedRamBank = 0;
        private readonly byte[,] _ram = new byte[RamBank, RamBankSize];
        private readonly byte[,] _rom;
        private bool _ramRTCEnambleFlag = false;
        private DateTime _latchClock;
        private int _latchClockData = 0x01;

        public MBC3(byte[] fileData, int romSize, int romBanks)
        {
            var bankSize = romSize / romBanks;
            _rom = new byte[romBanks, bankSize];

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
                if (_ramRTCEnambleFlag)
                {
                    if (_selectedRamBank < 4)
                    {
                        return _ram[_selectedRamBank, address - 0xA000];
                    }
                    else
                    {
                        Debug.Log("Get RTC register");
                        switch (_selectedRamBank)
                        {
                            case 0x08:
                                return _latchClock.Second;
                            case 0x09:
                                return _latchClock.Minute;
                            case 0x0A:
                                return _latchClock.Hour;
                            case 0x0B:
                                return _latchClock.DayOfYear & 0x00FF;
                            case 0x0C:
                                return (_latchClock.DayOfYear & 0x01FF) >> 8;
                        }
                    }
                }
            }
            Debug.LogError($"Invalid cartridge read: {address:X}");
            return 0;
        }

        public void WriteByte(int address, int value)
        {
            if (address <= 0x1FFF)
            {
                _ramRTCEnambleFlag = (value & 0x0F) == 0x0A;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                _selectedRomBank = 0x7F & value;
                if (_selectedRomBank == 0x00)
                {
                    _selectedRomBank = 0x01;
                }
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                _selectedRamBank = 0x0F & value;
            }
            else if (address >= 0x6000 && address <= 0x7FFF)
            {
                if (((0x01 & value) == 0x01) && (_latchClockData == 0x00))
                    _latchClock = DateTime.Now;
                _latchClockData = 0x01 & value;
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (_ramRTCEnambleFlag && _selectedRamBank < 4)
                {
                    _ram[_selectedRamBank, address - 0xA000] = (byte)(0xFF & value);
                }
            }
            else
            {
                Debug.LogError($"Invalid cartridge read: {address:X}");
            }
        }
    }
}
