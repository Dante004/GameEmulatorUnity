using System;
using Emulator.Cartridge;
using UnityEngine;

namespace Emulator.Cartridges
{
    public class MBC3 : ICartridge
    {
        private int _selectedRomBank = 1;
        private int _selectedRamBank = 0;
        private int _rtcRegister = 0;
        private readonly byte[,] _ram;
        private readonly byte[,] _rom;
        private bool _rtcEnable = false;
        private bool _ramEnable = false;
        private DateTime _latchClock;
        private int _latchClockData = 0x01;

        public MBC3(byte[] fileData, int romSize, int romBanks, int ramSize, int ramBanks)
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

        public byte ReadByte(ushort address)
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
                if (_selectedRamBank >= 0)
                {
                    if (_ramEnable)
                    {
                        return _ram[_selectedRamBank, address - 0xA000];
                    }
                    else
                    {
                        Debug.LogError($"Attempting to read from disabled ram {address:X}");
                        return 0xFF;
                    }
                }
                else if (_rtcEnable)
                {
                    Debug.Log("Get RTC register");
                    switch (_rtcRegister)
                    {
                        case 0x08:
                            return (byte) _latchClock.Second;
                        case 0x09:
                            return (byte) _latchClock.Minute;
                        case 0x0A:
                            return (byte) _latchClock.Hour;
                        case 0x0B:
                            return (byte) (_latchClock.DayOfYear & 0x00FF);
                        case 0x0C:
                            return (byte) ((_latchClock.DayOfYear & 0x01FF) >> 8);
                        default:
                            return 0xFF;
                    }
                }
                else
                {
                    Debug.LogError($"Attempting to read from disabled rtc {address:X}");
                    return 0xFF;
                }
            }
            Debug.LogError($"Invalid cartridge read: {address:X}");
            return 0;
        }

        public void WriteByte(ushort address, byte value)
        {
            if (address <= 0x1FFF)
            {
                _ramEnable = (value & 0x0F) == 0x0A;
                _rtcEnable = (value & 0x0F) == 0x0A;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                _selectedRomBank = value & 0x7F;
                if (_selectedRomBank == 0x00)
                {
                    _selectedRomBank = 0x01;
                }
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                if (value >= 0x08 && value <= 0x0C)
                {
                    if (_rtcEnable)
                    {
                        _rtcRegister = value;
                        _selectedRamBank = -1;
                    }
                    else
                    {
                        Debug.LogError($"Attempting to write rtc register when rtc is disabled {address:X}, {value:X}");
                    }
                }
                else if (value <= 0x03)
                {
                    _selectedRamBank = value;
                }
                else
                {
                    Debug.LogError($"Attempting to select unknown register {address:X}, {value:X}");
                }
            }
            else if (address >= 0x6000 && address <= 0x7FFF)
            {
                if (((0x01 & value) == 0x01) && (_latchClockData == 0x00))
                    _latchClock = DateTime.Now;
                _latchClockData = 0x01 & value;
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (_selectedRamBank >= 0)
                {
                    if (_ramEnable)
                    {
                        _ram[_selectedRamBank,address - 0xA000] = (byte) (value & 0xFF);
                    }
                    else
                    {
                        Debug.LogError($"Attempting to write on ram when ram is disabled {address:X}, {value:X}");
                    }
                }
                else if(_rtcEnable)
                {
                    switch (_rtcRegister)
                    {
                        case 0x08:
                            _latchClock.AddSeconds(value);
                            break;
                        case 0x09:
                            _latchClock.AddMinutes(value);
                            break;
                        case 0x0A:
                            _latchClock.AddHours(value);
                            break;
                        case 0x0B:
                            _latchClock.AddDays(value);
                            break;
                        case 0x0C:
                            _latchClock.AddDays(_latchClock.DayOfYear & 0x80 | value & 0xC1);
                            break;
                    }
                }
                else
                {
                    Debug.LogError($"Attempting to write on rtc when rtc is disabled {address:X}, {value:X}");
                }
            }
            else
            {
                Debug.LogError($"Invalid cartridge write: {address:X}, {value:X}");
            }
        }
    }
}
