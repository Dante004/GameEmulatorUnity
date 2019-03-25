using System;
using Emulator.Cartridge;
using Emulator.GPU;
using Emulator.CPU;

namespace Emulator.Memory
{
    public class GameBoyMemory
    {

        public bool LeftKeyPressed;
        public bool RightKeyPressed;
        public bool UpKeyPressed;
        public bool DownKeyPressed;
        public bool AButtonPressed;
        public bool BButtonPressed;
        public bool StartButtonPressed;
        public bool SelectButtonPressed;
        public bool KeyP14, KeyP15;

        public byte[] HighRam;
        public byte[] VideoRam;
        public byte[] WorkRam;
        public byte[] Oam;
        public ICartridge Cartridge;

        public GameBoyMemory()
        {
            HighRam = new byte[256];
            VideoRam = new byte[8 * 1024];
            WorkRam = new byte[8 * 1024];
            Oam = new byte[256];
        }

        public byte ReadByte(ushort address)
        {
            if (address <= 0x7FFF || (address >= 0xA000 && address <= 0xBFFF))
            {
                return Cartridge.ReadByte(address);
            }
            else if (address >= 0x8000 && address <= 0x9FFF)
            {
                return VideoRam[address - 0x8000];
            }
            else if (address >= 0xC000 && address <= 0xDFFF)
            {
                return WorkRam[address - 0xC000];
            }
            else if (address >= 0xE000 && address <= 0xFDFF)
            {
                return WorkRam[address - 0xE000];
            }
            else if (address >= 0xFE00 && address <= 0xFEFF)
            {
                return Oam[address - 0xFE00];
            }
            else if (address >= 0xFF80 && address <= 0xFFFE)
            {
                return HighRam[0xFF & address];
            }
            else
            {
                switch (address)
                {
                    case 0xFF00: // key pad
                        if (KeyP14)
                        {
                            byte value = 0;
                            if (!DownKeyPressed)
                            {
                                value |= 0x08;
                            }
                            if (!UpKeyPressed)
                            {
                                value |= 0x04;
                            }
                            if (!LeftKeyPressed)
                            {
                                value |= 0x02;
                            }
                            if (!RightKeyPressed)
                            {
                                value |= 0x01;
                            }
                            return value;
                        }
                        else if (KeyP15)
                        {
                            byte value = 0;
                            if (!StartButtonPressed)
                            {
                                value |= 0x08;
                            }
                            if (!SelectButtonPressed)
                            {
                                value |= 0x04;
                            }
                            if (!BButtonPressed)
                            {
                                value |= 0x02;
                            }
                            if (!AButtonPressed)
                            {
                                value |= 0x01;
                            }
                            return value;
                        }
                        break;
                    case 0xFF04: // Timer divider
                        return (byte)(GameBoyCPU.Ticks & 0xFF);
                    case 0xFF05: // Timer counter
                        return (byte)(GameBoyCPU.TimerCounter & 0xFF);
                    case 0xFF06: // Timer modulo
                        return (byte)(GameBoyCPU.TimerModulo & 0xFF);
                    case 0xFF07:
                        { // Time Control
                            byte value = 0;
                            if (GameBoyCPU.TimerRunning)
                            {
                                value |= 0x04;
                            }
                            value |= (byte)(GameBoyCPU.TimerFrequency);
                            return value;
                        }
                    case 0xFF0F:
                        { // Interrupt Flag (an interrupt request)
                            byte value = 0;
                            if (GameBoyCPU.KeyPressedInterruptRequested)
                            {
                                value |= 0x10;
                            }
                            if (GameBoyCPU.SerialIoTransferCompleteInterruptRequested)
                            {
                                value |= 0x08;
                            }
                            if (GameBoyCPU.TimerOverflowInterruptRequested)
                            {
                                value |= 0x04;
                            }
                            if (GameBoyCPU.LcdcInterruptRequested)
                            {
                                value |= 0x02;
                            }
                            if (GameBoyCPU.VBlankInterruptRequested)
                            {
                                value |= 0x01;
                            }
                            return value;
                        }
                    case 0xFF40:
                        { // LCDC control
                            byte value = 0;
                            if (GameBoyCPU.LcdControlOperationEnabled)
                            {
                                value |= 0x80;
                            }
                            if (GameBoyGPU.WindowTileMapDisplaySelect)
                            {
                                value |= 0x40;
                            }
                            if (GameBoyGPU.windowDisplayed)
                            {
                                value |= 0x20;
                            }
                            if (GameBoyGPU.BackgroundAndWindowTileDataSelect)
                            {
                                value |= 0x10;
                            }
                            if (GameBoyGPU.BackgroundTileMapDisplaySelect)
                            {
                                value |= 0x08;
                            }
                            if (GameBoyGPU.largeSprites)
                            {
                                value |= 0x04;
                            }
                            if (GameBoyGPU.spritesDisplayed)
                            {
                                value |= 0x02;
                            }
                            if (GameBoyGPU.backgroundDisplayed)
                            {
                                value |= 0x01;
                            }
                            return value;
                        }
                    case 0xFF41:
                        {// LCDC Status
                            byte value = 0;
                            if (GameBoyGPU.lcdcLycLyCoincidenceInterruptEnabled)
                            {
                                value |= 0x40;
                            }
                            if (GameBoyGPU.lcdcOamInterruptEnabled)
                            {
                                value |= 0x20;
                            }
                            if (GameBoyGPU.lcdcVBlankInterruptEnabled)
                            {
                                value |= 0x10;
                            }
                            if (GameBoyGPU.lcdcHBlankInterruptEnabled)
                            {
                                value |= 0x08;
                            }
                            if (GameBoyGPU.ly == GameBoyGPU.lyCompare)
                            {
                                value |= 0x04;
                            }
                            value |= (byte)GameBoyGPU.LcdcMode;
                            return value;
                        }
                    case 0xFF42: // Scroll Y
                        return GameBoyGPU.scrollY;
                    case 0xFF43: // Scroll X
                        return GameBoyGPU.scrollX;
                    case 0xFF44: // LY
                        return GameBoyGPU.ly;
                    case 0xFF45: // LY Compare
                        return GameBoyGPU.lyCompare;
                    case 0xFF47:
                        { // Background palette
                            GameBoyGPU.InvalidateAllBackgroundTilesRequest = true;
                            byte value = 0;
                            for (var i = 3; i >= 0; i--)
                            {
                                value <<= 2;
                                switch (GameBoyGPU.BackgroundPalette[i])
                                {
                                    case 0xFF000000:
                                        value |= 3;
                                        break;
                                    case 0xFF555555:
                                        value |= 2;
                                        break;
                                    case 0xFFAAAAAA:
                                        value |= 1;
                                        break;
                                    case 0xFFFFFFFF:
                                        break;
                                }
                            }
                            return value;
                        }
                    case 0xFF48:
                        { // Object palette 0
                            GameBoyGPU.InvalidateAllSpriteTilesRequest = true;
                            byte value = 0;
                            for (var i = 3; i >= 0; i--)
                            {
                                value <<= 2;
                                switch (GameBoyGPU.ObjectPallete0[i])
                                {
                                    case 0xFF000000:
                                        value |= 3;
                                        break;
                                    case 0xFF555555:
                                        value |= 2;
                                        break;
                                    case 0xFFAAAAAA:
                                        value |= 1;
                                        break;
                                    case 0xFFFFFFFF:
                                        break;
                                }
                            }
                            return value;
                        }
                    case 0xFF49:
                        { // Object palette 1
                            GameBoyGPU.InvalidateAllSpriteTilesRequest = true;
                            byte value = 0;
                            for (var i = 3; i >= 0; i--)
                            {
                                value <<= 2;
                                switch (GameBoyGPU.ObjectPallete1[i])
                                {
                                    case 0xFF000000:
                                        value |= 3;
                                        break;
                                    case 0xFF555555:
                                        value |= 2;
                                        break;
                                    case 0xFFAAAAAA:
                                        value |= 1;
                                        break;
                                    case 0xFFFFFFFF:
                                        break;
                                }
                            }
                            return value;
                        }
                    case 0xFF4A: // Window Y
                        return GameBoyGPU.windowY;
                    case 0xFF4B: // Window X
                        return GameBoyGPU.windowX;
                    case 0xFFFF:
                        { // Interrupt Enable
                            byte value = 0;
                            if (GameBoyCPU.KeyPressedInterruptEnabled)
                            {
                                value |= 0x10;
                            }
                            if (GameBoyCPU.SerialIoTransferCompleteInterruptEnabled)
                            {
                                value |= 0x08;
                            }
                            if (GameBoyCPU.TimerOverflowInterruptEnabled)
                            {
                                value |= 0x04;
                            }
                            if (GameBoyCPU.LcdcInterruptEnabled)
                            {
                                value |= 0x02;
                            }
                            if (GameBoyCPU.VBlankInterruptEnabled)
                            {
                                value |= 0x01;
                            }
                            return value;
                        }
                }
            }
            return 0;
        }

        public ushort ReadWord(ushort address)
        {
            byte low = ReadByte(address);
            byte high = ReadByte((ushort)(address + 1));
            return (ushort)((high << 8) | low);
        }

        public void WriteWord(ushort address, ushort value)
        {
            WriteByte(address, (byte)(value & 0xFF));
            WriteByte((ushort)(address + 1), (byte)(value >> 8));
        }

        public void WriteByte(ushort address, byte value)
        {
            if (address >= 0xC000 && address <= 0xDFFF)
            {
                WorkRam[address - 0xC000] = value;
            }
            else if (address >= 0xFE00 && address <= 0xFEFF)
            {
                Oam[address - 0xFE00] = value;
            }
            else if (address >= 0xFF80 && address <= 0xFFFE)
            {
                HighRam[0xFF & address] = value;
            }
            else if (address >= 0x8000 && address <= 0x9FFF)
            {
                ushort videoRamIndex = (ushort)(address - 0x8000);
                VideoRam[videoRamIndex] = value;
                if (address < 0x9000)
                {
                    GameBoyGPU.SpriteTileInvalidated[videoRamIndex >> 4] = true;
                }
                if (address < 0x9800)
                {
                    GameBoyGPU.InvalidateAllBackgroundTilesRequest = true;
                }
                else if (address >= 0x9C00)
                {
                    int tileIndex = address - 0x9C00;
                    GameBoyGPU.BackgroundTileInvalidated[tileIndex >> 5, tileIndex & 0x1F] = true;
                }
                else
                {
                    int tileIndex = address - 0x9800;
                    GameBoyGPU.BackgroundTileInvalidated[tileIndex >> 5, tileIndex & 0x1F] = true;
                }
            }
            else if (address <= 0x7FFF || (address >= 0xA000 && address <= 0xBFFF))
            {
                Cartridge.WriteByte(address, value);
            }
            else if (address >= 0xE000 && address <= 0xFDFF)
            {
                VideoRam[address - 0xE000] = value;
            }
            else
            {
                switch (address)
                {
                    case 0xFF00: // key pad
                        KeyP14 = (value & 0x10) != 0x10;
                        KeyP15 = (value & 0x20) != 0x20;
                        break;
                    case 0xFF04: // Timer divider            
                        break;
                    case 0xFF05: // Timer counter
                        GameBoyCPU.TimerCounter = value;
                        break;
                    case 0xFF06: // Timer modulo
                        GameBoyCPU.TimerModulo = value;
                        break;
                    case 0xFF07:  // Time Control
                        GameBoyCPU.TimerRunning = (value & 0x04) == 0x04;
                        GameBoyCPU.TimerFrequency = (TimerFrequencyType)(0x03 & value);
                        break;
                    case 0xFF0F: // Interrupt Flag (an interrupt request)
                        GameBoyCPU.KeyPressedInterruptRequested = (value & 0x10) == 0x10;
                        GameBoyCPU.SerialIoTransferCompleteInterruptRequested = (value & 0x08) == 0x08;
                        GameBoyCPU.TimerOverflowInterruptRequested = (value & 0x04) == 0x04;
                        GameBoyCPU.LcdcInterruptRequested = (value & 0x02) == 0x02;
                        GameBoyCPU.VBlankInterruptRequested = (value & 0x01) == 0x01;
                        break;
                    case 0xFF40:
                        { // LCDC control
                            var backgroundAndWindowTileDataSelect = GameBoyGPU.BackgroundAndWindowTileDataSelect;
                            var backgroundTileMapDisplaySelect = GameBoyGPU.BackgroundTileMapDisplaySelect;
                            var windowTileMapDisplaySelect = GameBoyGPU.WindowTileMapDisplaySelect;

                            GameBoyCPU.LcdControlOperationEnabled = (value & 0x80) == 0x80;
                            GameBoyGPU.WindowTileMapDisplaySelect = (value & 0x40) == 0x40;
                            GameBoyGPU.windowDisplayed = (value & 0x20) == 0x20;
                            GameBoyGPU.BackgroundAndWindowTileDataSelect = (value & 0x10) == 0x10;
                            GameBoyGPU.BackgroundTileMapDisplaySelect = (value & 0x08) == 0x08;
                            GameBoyGPU.largeSprites = (value & 0x04) == 0x04;
                            GameBoyGPU.spritesDisplayed = (value & 0x02) == 0x02;
                            GameBoyGPU.backgroundDisplayed = (value & 0x01) == 0x01;

                            if (backgroundAndWindowTileDataSelect != GameBoyGPU.BackgroundAndWindowTileDataSelect
                                || backgroundTileMapDisplaySelect != GameBoyGPU.BackgroundTileMapDisplaySelect
                                || windowTileMapDisplaySelect != GameBoyGPU.WindowTileMapDisplaySelect)
                            {
                                GameBoyGPU.InvalidateAllBackgroundTilesRequest = true;
                            }

                            break;
                        }
                    case 0xFF41: // LCDC Status
                        GameBoyGPU.lcdcLycLyCoincidenceInterruptEnabled = (value & 0x40) == 0x40;
                        GameBoyGPU.lcdcOamInterruptEnabled = (value & 0x20) == 0x20;
                        GameBoyGPU.lcdcVBlankInterruptEnabled = (value & 0x10) == 0x10;
                        GameBoyGPU.lcdcHBlankInterruptEnabled = (value & 0x08) == 0x08;
                        GameBoyGPU.LcdcMode = (LcdcModeType)(value & 0x03);
                        break;
                    case 0xFF42: // Scroll Y;
                        GameBoyGPU.scrollY = value;
                        break;
                    case 0xFF43: // Scroll X;
                        GameBoyGPU.scrollX = value;
                        break;
                    case 0xFF44: // LY
                        GameBoyGPU.ly = value;
                        break;
                    case 0xFF45: // LY Compare
                        GameBoyGPU.lyCompare = value;
                        break;
                    case 0xFF46: // Memory Transfer
                        value <<= 8;
                        for (var i = 0; i < 0x8C; i++)
                        {
                            WriteByte((ushort)(0xFE00 | i), ReadByte((ushort)(value | i)));
                        }
                        break;
                    case 0xFF47: // Background palette
                        Console.WriteLine("[0xFF47] = {0:X}", value);
                        for (var i = 0; i < 4; i++)
                        {
                            switch (value & 0x03)
                            {
                                case 0:
                                    GameBoyGPU.BackgroundPalette[i] = 0xFFFFFFFF;
                                    break;
                                case 1:
                                    GameBoyGPU.BackgroundPalette[i] = 0xFFAAAAAA;
                                    break;
                                case 2:
                                    GameBoyGPU.BackgroundPalette[i] = 0xFF555555;
                                    break;
                                case 3:
                                    GameBoyGPU.BackgroundPalette[i] = 0xFF000000;
                                    break;
                            }
                            value >>= 2;
                        }
                        GameBoyGPU.InvalidateAllBackgroundTilesRequest = true;
                        break;
                    case 0xFF48: // Object palette 0
                        for (var i = 0; i < 4; i++)
                        {
                            switch (value & 0x03)
                            {
                                case 0:
                                    GameBoyGPU.ObjectPallete0[i] = 0xFFFFFFFF;
                                    break;
                                case 1:
                                    GameBoyGPU.ObjectPallete0[i] = 0xFFAAAAAA;
                                    break;
                                case 2:
                                    GameBoyGPU.ObjectPallete0[i] = 0xFF555555;
                                    break;
                                case 3:
                                    GameBoyGPU.ObjectPallete0[i] = 0xFF000000;
                                    break;
                            }
                            value >>= 2;
                        }
                        GameBoyGPU.InvalidateAllSpriteTilesRequest = true;
                        break;
                    case 0xFF49: // Object palette 1
                        for (var i = 0; i < 4; i++)
                        {
                            switch (value & 0x03)
                            {
                                case 0:
                                    GameBoyGPU.ObjectPallete1[i] = 0xFFFFFFFF;
                                    break;
                                case 1:
                                    GameBoyGPU.ObjectPallete1[i] = 0xFFAAAAAA;
                                    break;
                                case 2:
                                    GameBoyGPU.ObjectPallete1[i] = 0xFF555555;
                                    break;
                                case 3:
                                    GameBoyGPU.ObjectPallete1[i] = 0xFF000000;
                                    break;
                            }
                            value >>= 2;
                        }
                        GameBoyGPU.InvalidateAllSpriteTilesRequest = true;
                        break;
                    case 0xFF4A: // Window Y
                        GameBoyGPU.windowY = value;
                        break;
                    case 0xFF4B: // Window X
                        GameBoyGPU.windowX = value;
                        break;
                    case 0xFFFF: // Interrupt Enable
                        GameBoyCPU.KeyPressedInterruptEnabled = (value & 0x10) == 0x10;
                        GameBoyCPU.SerialIoTransferCompleteInterruptEnabled = (value & 0x08) == 0x08;
                        GameBoyCPU.TimerOverflowInterruptEnabled = (value & 0x04) == 0x04;
                        GameBoyCPU.LcdcInterruptEnabled = (value & 0x02) == 0x02;
                        GameBoyCPU.VBlankInterruptEnabled = (value & 0x01) == 0x01;
                        break;
                }
            }
        }

        public void KeyChanged(char keyCode, bool pressed)
        {
            switch (keyCode)
            {
                case 'z':
                    BButtonPressed = pressed;
                    break;
                case 'x':
                    AButtonPressed = pressed;
                    break;
                case 'k':
                    StartButtonPressed = pressed;
                    break;
                case 'l':
                    SelectButtonPressed = pressed;
                    break;
                case 'w':
                    UpKeyPressed = pressed;
                    break;
                case 's':
                    DownKeyPressed = pressed;
                    break;
                case 'a':
                    LeftKeyPressed = pressed;
                    break;
                case 'd':
                    RightKeyPressed = pressed;
                    break;
            }
        }

    }
}