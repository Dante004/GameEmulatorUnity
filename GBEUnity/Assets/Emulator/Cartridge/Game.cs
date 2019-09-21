using Emulator.Cartridges;
using UnityEngine;

namespace Emulator.Cartridge
{
    internal class Game
    {
        public string title;
        public bool gameBoyColorGame;
        public int licenseCode;
        public bool gameBoy;
        public CartridgeType romType;
        public int romSize;
        public int romBanks;
        public int ramSize;
        public int ramBanks;
        public bool japanese;
        public int oldLicenseCode;
        public int maskRomVersion;
        public int checksum;
        public int actualChecksum;
        public int headerChecksum;
        public int actualHeaderChecksum;
        public bool noVerticalBlankInterruptHandler;
        public bool noLCDCStatusInterruptHandler;
        public bool noTimerOverflowInterruptHandler;
        public bool noSerialTransferCompletionInterruptHandler;
        public bool noHighToLowOfP10ToP13InterruptHandler;
        public ICartridge cartridge;

        internal static Game Load(byte[] fileData)
        {
            return new Game(fileData);
        }

        Game(byte[] fileData)
        {
            romType = (CartridgeType) fileData[0x0147];

            switch (fileData[0x0148])
            {
                case 0x00:
                    romSize = 32 * 1024;
                    romBanks = 2;
                    break;
                case 0x01:
                    romSize = 64 * 1024;
                    romBanks = 4;
                    break;
                case 0x02:
                    romSize = 128 * 1024;
                    romBanks = 8;
                    break;
                case 0x03:
                    romSize = 256 * 1024;
                    romBanks = 16;
                    break;
                case 0x04:
                    romSize = 512 * 1024;
                    romBanks = 32;
                    break;
                case 0x05:
                    romSize = 1024 * 1024;
                    romBanks = 64;
                    break;
                case 0x06:
                    romSize = 2 * 1024 * 1024;
                    romBanks = 128;
                    break;
                case 0x52:
                    romSize = 1179648;
                    romBanks = 72;
                    break;
                case 0x53:
                    romSize = 1310720;
                    romBanks = 80;
                    break;
                case 0x54:
                    romSize = 1572864;
                    romBanks = 96;
                    break;
            }

            switch (fileData[0x0149])
            {
                case 0x00:
                    ramSize = 0;
                    ramBanks = 0;
                    break;
                case 0x01:
                    ramSize = 2 * 1024;
                    ramBanks = 1;
                    break;
                case 0x02:
                    ramSize = 8 * 1024;
                    ramBanks = 1;
                    break;
                case 0x03:
                    ramSize = 32 * 1024;
                    ramBanks = 4;
                    break;
                case 0x04:
                    ramSize = 128 * 1024;
                    ramBanks = 16;
                    break;
            }

            switch (romType)
            {
                case CartridgeType.ROM:
                    cartridge = new RomOnly(fileData);
                    break;
                case CartridgeType.ROM_MBC1:
                case CartridgeType.ROM_MBC1_RAM:
                case CartridgeType.ROM_MBC1_RAM_BATT:
                    cartridge = new MBC1(fileData, romSize, romBanks, ramSize, ramBanks);
                    break;
                case CartridgeType.ROM_MBC2:
                case CartridgeType.ROM_MBC2_BATTERY:
                    cartridge = new MBC2(fileData, romSize, romBanks);
                    break;
                case CartridgeType.ROM_MBC3:
                case CartridgeType.ROM_MBC3_RAM:
                case CartridgeType.ROM_MBC3_RAM_BATT:
                case CartridgeType.ROM_MBC3_TIMER_BATT:
                case CartridgeType.ROM_MBC3_TIMER_RAM_BATT:
                    cartridge = new MBC3(fileData, romSize, romBanks, ramSize, ramBanks);
                    break;
                case CartridgeType.ROM_MBC5:
                case CartridgeType.ROM_MBC5_RAM:
                case CartridgeType.ROM_MBC5_RAM_BATT:
                    cartridge = new MBC5(fileData, romSize, romBanks, ramSize, ramBanks);
                    break;
                default:
                    Debug.LogError($"Cannot emulate cartridge type {romType}");
                    cartridge = null;
                    break;
            }
        }
    }
}
