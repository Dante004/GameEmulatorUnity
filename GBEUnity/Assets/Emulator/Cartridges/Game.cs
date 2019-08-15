using System.IO;
using System.Text;
using UnityEngine;

namespace Emulator.Cartridges
{ 
    internal class RomLoader
    {
        public static Game Load(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            var fileData = new byte[fileInfo.Length];
            var fileStream = fileInfo.OpenRead();
            fileStream.Read(fileData, 0, fileData.Length);
            fileStream.Close();

            return new Game(fileData);
        }
    }

    internal class Game
    {
        public string title;
        public bool gameBoyColorGame;
        public int licenseCode;
        public bool gameBoy;
        public RomType romType;
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

        public Game(byte[] fileData)
        {
            title = ExtractGameTitle(fileData);
            gameBoyColorGame = fileData[0x0143] == 0x80;
            licenseCode = (((int) fileData[0x0144]) << 4) | fileData[0x0145];
            gameBoy = fileData[0x0146] == 0x00;
            romType = (RomType) fileData[0x0147];

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

            japanese = fileData[0x014A] == 0x00;
            oldLicenseCode = fileData[0x014B];
            maskRomVersion = fileData[0x014C];

            headerChecksum = fileData[0x014D];
            for (var i = 0x0134; i <= 0x014C; ++i)
            {
                actualHeaderChecksum = actualHeaderChecksum - fileData[i] - 1;
            }

            actualHeaderChecksum &= 0xFF;

            checksum = (((int) fileData[0x014E]) << 8) | fileData[0x014F];
            for (var i = 0; i < fileData.Length; i++)
            {
                if (i != 0x014E && i != 0x014F)
                {
                    actualChecksum += fileData[i];
                }
            }

            actualChecksum &= 0xFFFF;

            noVerticalBlankInterruptHandler = fileData[0x0040] == 0xD9;
            noLCDCStatusInterruptHandler = fileData[0x0048] == 0xD9;
            noTimerOverflowInterruptHandler = fileData[0x0050] == 0xD9;
            noSerialTransferCompletionInterruptHandler = fileData[0x0058] == 0xD9;
            noHighToLowOfP10ToP13InterruptHandler = fileData[0x0060] == 0xD9;

            Debug.Log(ToString());

            switch (romType)
            {
                case RomType.ROM:
                    cartridge = new RomOnly(fileData);
                    break;
                case RomType.ROM_MBC1:
                case RomType.ROM_MBC1_RAM:
                case RomType.ROM_MBC1_RAM_BATT:
                    cartridge = new MBC1(fileData, romSize, romBanks, ramSize, ramBanks);
                    break;
                case RomType.ROM_MBC2:
                case RomType.ROM_MBC2_BATTERY:
                    cartridge = new MBC2(fileData, romSize, romBanks);
                    break;
                case RomType.ROM_MBC3:
                case RomType.ROM_MBC3_RAM:
                case RomType.ROM_MBC3_RAM_BATT:
                case RomType.ROM_MBC3_TIMER_BATT:
                case RomType.ROM_MBC3_TIMER_RAM_BATT:
                    cartridge = new MBC3(fileData, romSize, romBanks);
                    break;
                case RomType.ROM_MBC5:
                case RomType.ROM_MBC5_RAM:
                case RomType.ROM_MBC5_RAM_BATT:
                    cartridge = new MBC5(fileData, romSize, romBanks);
                    break;
                default:
                    Debug.LogError($"Cannot emulate cartridge type {romType}");
                    cartridge = null;
                    break;
            }
        }

        private static string ExtractGameTitle(byte[] fileData)
        {
            var title = new StringBuilder();
            for (var i = 0x0134; i <= 0x0142; ++i)
            {
                if (fileData[i] == 0x00)
                {
                    break;
                }

                title.Append((char) fileData[i]);
            }

            return title.ToString();
        }

        public sealed override string ToString()
        {
            return "title = " + title + "\n"
                   + "game boy color game = " + gameBoyColorGame + "\n"
                   + "license code = " + licenseCode + "\n"
                   + "game boy = " + gameBoy + "\n"
                   + "rom type = " + romType + "\n"
                   + "rom size = " + romSize + "\n"
                   + "rom banks = " + romBanks + "\n"
                   + "ram size = " + ramSize + "\n"
                   + "ram banks = " + ramBanks + "\n"
                   + "japanese = " + japanese + "\n"
                   + "old license code = " + oldLicenseCode + "\n"
                   + "mask rom version = " + maskRomVersion + "\n"
                   + "header checksum = " + headerChecksum + "\n"
                   + "actual header checksum = " + actualHeaderChecksum + "\n"
                   + "checksum = " + checksum + "\n"
                   + "actual checksum = " + actualChecksum + "\n"
                   + "no vertical blank interrupt handler = " + noVerticalBlankInterruptHandler + "\n"
                   + "no lcd status interrupt handler = " + noLCDCStatusInterruptHandler + "\n"
                   + "no timer overflow interrupt handler = " + noTimerOverflowInterruptHandler + "\n"
                   + "no serial transfer completion interrupt handler = " + noSerialTransferCompletionInterruptHandler +
                   "\n"
                   + "no high to lower of P10-P13 interrupt handler = " + noHighToLowOfP10ToP13InterruptHandler + "\n";
        }
    }
}
