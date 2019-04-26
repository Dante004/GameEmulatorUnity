using System;
using Emulator.Audio;
using Emulator.Cartridges;

namespace Emulator
{


    public enum TimerFrequencyType
    {
        hz4096 = 0,
        hz262144 = 1,
        hz65536 = 2,
        hz16384 = 3
    }

    public class Memory
    {
        public bool leftKeyPressed;
        public bool rightKeyPressed;
        public bool upKeyPressed;
        public bool downKeyPressed;
        public bool aButtonPressed;
        public bool bButtonPressed;
        public bool startButtonPressed;
        public bool selectButtonPressed;
        public bool keyP14, keyP15;

        public bool timerRunning;
        public int timerCounter;
        public int timerModulo;

        public byte[] highRam = new byte[256];
        public byte[] videoRam = new byte[8 * 1024];
        public byte[] workRam = new byte[8 * 1024];
        public byte[] oam = new byte[256];
        public ICartridge cartridge;
        public TimerFrequencyType timerFrequency;

        public CPU cpu;
        public PPU ppu;
        public SoundChip SoundChip;
        private readonly int[] soundRegisters = new int[0x30];

        public Memory()
        {
            SoundChip = new SoundChip();
        }


        public void WriteWord(int address, int value)
        {
            WriteByte(address, value & 0xFF);
            WriteByte(address + 1, value >> 8);
        }

        public int ReadWord(int address)
        {
            int low = ReadByte(address);
            int high = ReadByte(address + 1);
            return (high << 8) | low;
        }

        public void WriteByte(int address, int value)
        {
            if (address >= 0xC000 && address <= 0xDFFF)
            {
                workRam[address - 0xC000] = (byte)value;
            }
            else if (address >= 0xFE00 && address <= 0xFEFF)
            {
                oam[address - 0xFE00] = (byte)value;
            }
            else if (address >= 0xFF80 && address <= 0xFFFE)
            {
                highRam[0xFF & address] = (byte)value;
            }
            else if (address >= 0x8000 && address <= 0x9FFF)
            {
                int videoRamIndex = address - 0x8000;
                videoRam[videoRamIndex] = (byte)value;
                if (address < 0x9000)
                {
                    ppu.spriteTileInvalidated[videoRamIndex >> 4] = true;
                }
                if (address < 0x9800)
                {
                    ppu.invalidateAllBackgroundTilesRequest = true;
                }
                else if (address >= 0x9C00)
                {
                    int tileIndex = address - 0x9C00;
                    ppu.backgroundTileInvalidated[tileIndex >> 5, tileIndex & 0x1F] = true;
                }
                else
                {
                    int tileIndex = address - 0x9800;
                    ppu.backgroundTileInvalidated[tileIndex >> 5, tileIndex & 0x1F] = true;
                }
            }
            else if (address <= 0x7FFF || (address >= 0xA000 && address <= 0xBFFF))
            {
                cartridge.WriteByte(address, value);
            }
            else if (address >= 0xE000 && address <= 0xFDFF)
            {
                workRam[address - 0xE000] = (byte)value;
            }
            else
            {
                switch (address)
                {
                    case 0xFF00: // key pad
                        keyP14 = (value & 0x10) != 0x10;
                        keyP15 = (value & 0x20) != 0x20;
                        break;
                    case 0xFF04: // Timer divider
                        break;
                    case 0xFF05: // Timer counter
                        timerCounter = value;
                        break;
                    case 0xFF06: // Timer modulo
                        timerModulo = value;
                        break;
                    case 0xFF07:  // Time Control
                        timerRunning = (value & 0x04) == 0x04;
                        timerFrequency = (TimerFrequencyType)(0x03 & value);
                        break;
                    case 0xFF0F: // Interrupt Flag (an interrupt request)
                        cpu.keyPressedInterruptRequested = (value & 0x10) == 0x10;
                        cpu.serialIOTransferCompleteInterruptRequested = (value & 0x08) == 0x08;
                        cpu.timerOverflowInterruptRequested = (value & 0x04) == 0x04;
                        cpu.lcdcInterruptRequested = (value & 0x02) == 0x02;
                        cpu.vBlankInterruptRequested = (value & 0x01) == 0x01;
                        break;
                    case 0xFF10:
                        SoundChip.channel1.SetSweep(
                            (value & 0x70) >> 4,
                            (value & 0x07),
                            (value & 0x08) == 1);
                        soundRegisters[0x10 - 0x10] = value;
                        break;

                    case 0xFF11:           // Sound channel 1, length and wave duty
                        SoundChip.channel1.SetDutyCycle((value & 0xC0) >> 6);
                        SoundChip.channel1.SetLength(value & 0x3F);
                        soundRegisters[0x11 - 0x10] = value;
                        break;

                    case 0xFF12:           // Sound channel 1, volume envelope
                        SoundChip.channel1.SetEnvelope(
                            (value & 0xF0) >> 4,
                            (value & 0x07),
                            (value & 0x08) == 8);
                        soundRegisters[0x12 - 0x10] = value;
                        break;

                    case 0xFF13:           // Sound channel 1, frequency low
                        soundRegisters[0x13 - 0x10] = value;
                        SoundChip.channel1.SetFrequency(
                            ((soundRegisters[0x14 - 0x10] & 0x07) << 8) + soundRegisters[0x13 - 0x10]);
                        break;

                    case 0xFF14:           // Sound channel 1, frequency high
                        soundRegisters[0x14 - 0x10] = value;

                        if ((soundRegisters[0x14 - 0x10] & 0x80) != 0)
                        {
                            SoundChip.channel1.SetLength(soundRegisters[0x11 - 0x10] & 0x3F);
                            SoundChip.channel1.SetEnvelope(
                                (soundRegisters[0x12 - 0x10] & 0xF0) >> 4,
                                (soundRegisters[0x12 - 0x10] & 0x07),
                                (soundRegisters[0x12 - 0x10] & 0x08) == 8);
                        }
                        if ((soundRegisters[0x14 - 0x10] & 0x40) == 0)
                        {
                            SoundChip.channel1.SetLength(-1);
                        }

                        SoundChip.channel1.SetFrequency(
                            ((soundRegisters[0x14 - 0x10] & 0x07) << 8) + soundRegisters[0x13 - 0x10]);

                        break;

                    case 0xFF17:           // Sound channel 2, volume envelope
                        SoundChip.channel2.SetEnvelope(
                            (value & 0xF0) >> 4,
                            value & 0x07,
                            (value & 0x08) == 8);
                        soundRegisters[0x17 - 0x10] = value;
                        break;

                    case 0xFF18:           // Sound channel 2, frequency low
                        soundRegisters[0x18 - 0x10] = value;
                        SoundChip.channel2.SetFrequency(
                            ((soundRegisters[0x19 - 0x10] & 0x07) << 8) + soundRegisters[0x18 - 0x10]);
                        break;

                    case 0xFF19:           // Sound channel 2, frequency high
                        soundRegisters[0x19 - 0x10] = value;

                        if ((value & 0x80) != 0)
                        {
                            SoundChip.channel2.SetLength(soundRegisters[0x21 - 0x10] & 0x3F);
                            SoundChip.channel2.SetEnvelope(
                                (soundRegisters[0x17 - 0x10] & 0xF0) >> 4,
                                (soundRegisters[0x17 - 0x10] & 0x07),
                                (soundRegisters[0x17 - 0x10] & 0x08) == 8);
                        }
                        if ((soundRegisters[0x19 - 0x10] & 0x40) == 0)
                        {
                            SoundChip.channel2.SetLength(-1);
                        }
                        SoundChip.channel2.SetFrequency(
                            ((soundRegisters[0x19 - 0x10] & 0x07) << 8) + soundRegisters[0x18 - 0x10]);
                        break;

                    case 0xFF16:           // Sound channel 2, length and wave duty
                        SoundChip.channel2.SetDutyCycle((value & 0xC0) >> 6);
                        SoundChip.channel2.SetLength(value & 0x3F);
                        soundRegisters[0x16 - 0x10] = value;
                        break;

                    case 0xFF1A:           // Sound channel 3, on/off
                        if ((value & 0x80) != 0)
                        {
                            SoundChip.channel3.SetVolume((soundRegisters[0x1C - 0x10] & 0x60) >> 5);
                        }
                        else
                        {
                            SoundChip.channel3.SetVolume(0);
                        }
                        soundRegisters[0x1A - 0x10] = value;
                        break;

                    case 0xFF1B:           // Sound channel 3, length
                        soundRegisters[0x1B - 0x10] = value;
                        SoundChip.channel3.SetLength(value);
                        break;

                    case 0xFF1C:           // Sound channel 3, volume
                        soundRegisters[0x1C - 0x10] = value;
                        SoundChip.channel3.SetVolume((value & 0x60) >> 5);
                        break;

                    case 0xFF1D:           // Sound channel 3, frequency lower 8-bit
                        soundRegisters[0x1D - 0x10] = value;
                        SoundChip.channel3.SetFrequency(
                        ((soundRegisters[0x1E - 0x10] & 0x07) << 8) + soundRegisters[0x1D - 0x10]);
                        break;

                    case 0xFF1E:           // Sound channel 3, frequency higher 3-bit
                        soundRegisters[0x1E - 0x10] = value;
                        if ((soundRegisters[0x19 - 0x10] & 0x80) != 0)
                        {
                            SoundChip.channel3.SetLength(soundRegisters[0x1B - 0x10]);
                        }
                        SoundChip.channel3.SetFrequency(
                            ((soundRegisters[0x1E - 0x10] & 0x07) << 8) + soundRegisters[0x1D - 0x10]);
                        break;

                    case 0xFF20:           // Sound channel 4, length
                        SoundChip.channel4.SetLength(value & 0x3F);
                        soundRegisters[0x20 - 0x10] = value;
                        break;

                    case 0xFF21:           // Sound channel 4, volume envelope
                        SoundChip.channel4.SetEnvelope(
                        (value & 0xF0) >> 4,
                        (value & 0x07),
                        (value & 0x08) == 8);
                        soundRegisters[0x21 - 0x10] = value;
                        break;

                    case 0xFF22:           // Sound channel 4, polynomial parameters
                        SoundChip.channel4.SetParameters(
                        (value & 0x07),
                        (value & 0x08) == 8,
                        (value & 0xF0) >> 4);
                        soundRegisters[0x22 - 0x10] = value;
                        break;

                    case 0xFF23:          // Sound channel 4, initial/consecutive
                        soundRegisters[0x23 - 0x10] = value;
                        if ((value & 0x80) != 0)
                        {
                            SoundChip.channel4.SetLength(soundRegisters[0x20 - 0x10] & 0x3F);
                        }
                        else if (((value & 0x80) & 0x40) == 0)
                        {
                            SoundChip.channel4.SetLength(-1);
                        }
                        break;
                    case 0xFF24:
                        // TODO volume
                        break;
                    case 0xFF25:           // Stereo select
                        int chanData;
                        soundRegisters[0x25 - 0x10] = value;

                        chanData = 0;
                        if ((value & 0x01) != 0)
                        {
                            chanData |= SquareWave.ChannelLeft;
                        }
                        if ((value & 0x10) != 0)
                        {
                            chanData |= SquareWave.ChannelRight;
                        }
                        SoundChip.channel1.SetChannel(chanData);

                        chanData = 0;
                        if ((value & 0x02) != 0)
                        {
                            chanData |= SquareWave.ChannelLeft;
                        }
                        if ((value & 0x20) != 0)
                        {
                            chanData |= SquareWave.ChannelRight;
                        }
                        SoundChip.channel2.SetChannel(chanData);

                        chanData = 0;
                        if ((value & 0x04) != 0)
                        {
                            chanData |= VoluntaryWave.ChannelLeft;
                        }
                        if ((value & 0x40) != 0)
                        {
                            chanData |= VoluntaryWave.ChannelRight;
                        }
                        SoundChip.channel3.SetChannel(chanData);

                        chanData = 0;
                        if ((value & 0x08) != 0)
                        {
                            chanData |= Noise.ChannelLeft;
                        }
                        if ((value & 0x80) != 0)
                        {
                            chanData |= Noise.ChannelRight;
                        }
                        SoundChip.channel4.SetChannel(chanData);

                        break;

                    case 0xFF26:
                        SoundChip.soundEnabled = (value & 0x80) == 1;
                        break;
                    case 0xFF30:
                    case 0xFF31:
                    case 0xFF32:
                    case 0xFF33:
                    case 0xFF34:
                    case 0xFF35:
                    case 0xFF36:
                    case 0xFF37:
                    case 0xFF38:
                    case 0xFF39:
                    case 0xFF3A:
                    case 0xFF3B:
                    case 0xFF3C:
                    case 0xFF3D:
                    case 0xFF3E:
                    case 0xFF3F:
                        SoundChip.channel3.SetSamplePair(address - 0xFF30, value);
                        soundRegisters[address - 0xFF10] = value;
                        break;

                    case 0xFF40:
                        { // LCDC control
                            bool _backgroundAndWindowTileDataSelect = ppu.backgroundAndWindowTileDataSelect;
                            bool _backgroundTileMapDisplaySelect = ppu.backgroundTileMapDisplaySelect;
                            bool _windowTileMapDisplaySelect = ppu.windowTileMapDisplaySelect;

                            ppu.lcdControlOperationEnabled = (value & 0x80) == 0x80;
                            ppu.windowTileMapDisplaySelect = (value & 0x40) == 0x40;
                            ppu.windowDisplayed = (value & 0x20) == 0x20;
                            ppu.backgroundAndWindowTileDataSelect = (value & 0x10) == 0x10;
                            ppu.backgroundTileMapDisplaySelect = (value & 0x08) == 0x08;
                            ppu.largeSprites = (value & 0x04) == 0x04;
                            ppu.spritesDisplayed = (value & 0x02) == 0x02;
                            ppu.backgroundDisplayed = (value & 0x01) == 0x01;

                            if (_backgroundAndWindowTileDataSelect != ppu.backgroundAndWindowTileDataSelect
                                || _backgroundTileMapDisplaySelect != ppu.backgroundTileMapDisplaySelect
                                || _windowTileMapDisplaySelect != ppu.windowTileMapDisplaySelect)
                            {
                                ppu.invalidateAllBackgroundTilesRequest = true;
                            }

                            break;
                        }
                    case 0xFF41: // LCDC Status
                        ppu.lcdcLycLyCoincidenceInterruptEnabled = (value & 0x40) == 0x40;
                        ppu.lcdcOamInterruptEnabled = (value & 0x20) == 0x20;
                        ppu.lcdcVBlankInterruptEnabled = (value & 0x10) == 0x10;
                        ppu.lcdcHBlankInterruptEnabled = (value & 0x08) == 0x08;
                        ppu.lcdcMode = (LcdcModeType)(value & 0x03);
                        break;
                    case 0xFF42: // Scroll Y;
                        ppu.scrollY = value;
                        break;
                    case 0xFF43: // Scroll X;
                        ppu.scrollX = value;
                        break;
                    case 0xFF44: // LY
                        ppu.ly = value;
                        break;
                    case 0xFF45: // LY Compare
                        ppu.lyCompare = value;
                        break;
                    case 0xFF46: // Memory Transfer
                        value <<= 8;
                        for (int i = 0; i < 0x8C; i++)
                        {
                            WriteByte(0xFE00 | i, ReadByte(value | i));
                        }
                        break;
                    case 0xFF47: // Background palette
                        System.Console.WriteLine("[0xFF47] = {0:X}", value);
                        for (int i = 0; i < 4; i++)
                        {
                            switch (value & 0x03)
                            {
                                case 0:
                                    ppu.backgroundPalette[i] = ppu.WHITE;
                                    break;
                                case 1:
                                    ppu.backgroundPalette[i] = ppu.LIGHT_GRAY;
                                    break;
                                case 2:
                                    ppu.backgroundPalette[i] = ppu.DARK_GRAY;
                                    break;
                                case 3:
                                    ppu.backgroundPalette[i] = ppu.BLACK;
                                    break;
                            }
                            value >>= 2;
                        }
                        ppu.invalidateAllBackgroundTilesRequest = true;
                        break;
                    case 0xFF48: // Object palette 0
                        for (int i = 0; i < 4; i++)
                        {
                            switch (value & 0x03)
                            {
                                case 0:
                                    ppu.objectPalette0[i] = ppu.WHITE;
                                    break;
                                case 1:
                                    ppu.objectPalette0[i] = ppu.LIGHT_GRAY;
                                    break;
                                case 2:
                                    ppu.objectPalette0[i] = ppu.DARK_GRAY;
                                    break;
                                case 3:
                                    ppu.objectPalette0[i] = ppu.BLACK;
                                    break;
                            }
                            value >>= 2;
                        }
                        ppu.invalidateAllSpriteTilesRequest = true;
                        break;
                    case 0xFF49: // Object palette 1
                        for (int i = 0; i < 4; i++)
                        {
                            switch (value & 0x03)
                            {
                                case 0:
                                    ppu.objectPalette1[i] = ppu.WHITE;
                                    break;
                                case 1:
                                    ppu.objectPalette1[i] = ppu.LIGHT_GRAY;
                                    break;
                                case 2:
                                    ppu.objectPalette1[i] = ppu.DARK_GRAY;
                                    break;
                                case 3:
                                    ppu.objectPalette1[i] = ppu.BLACK;
                                    break;
                            }
                            value >>= 2;
                        }
                        ppu.invalidateAllSpriteTilesRequest = true;
                        break;
                    case 0xFF4A: // Window Y
                        ppu.windowY = value;
                        break;
                    case 0xFF4B: // Window X
                        ppu.windowX = value;
                        break;
                    case 0xFFFF: // Interrupt Enable
                        cpu.keyPressedInterruptEnabled = (value & 0x10) == 0x10;
                        cpu.serialIOTransferCompleteInterruptEnabled = (value & 0x08) == 0x08;
                        cpu.timerOverflowInterruptEnabled = (value & 0x04) == 0x04;
                        cpu.lcdcInterruptEnabled = (value & 0x02) == 0x02;
                        cpu.vBlankInterruptEnabled = (value & 0x01) == 0x01;
                        break;
                }
            }
        }

        public int ReadByte(int address)
        {
            if (address <= 0x7FFF || (address >= 0xA000 && address <= 0xBFFF))
            {
                return cartridge.ReadByte(address);
            }
            else if (address >= 0x8000 && address <= 0x9FFF)
            {
                return videoRam[address - 0x8000];
            }
            else if (address >= 0xC000 && address <= 0xDFFF)
            {
                return workRam[address - 0xC000];
            }
            else if (address >= 0xE000 && address <= 0xFDFF)
            {
                return workRam[address - 0xE000];
            }
            else if (address >= 0xFE00 && address <= 0xFEFF)
            {
                return oam[address - 0xFE00];
            }
            else if (address >= 0xFF80 && address <= 0xFFFE)
            {
                return highRam[0xFF & address];
            }
            else if(address >= 0xFF10 && address<0xFF3F)
            {
                return soundRegisters[address - 0xFF10];
            }
            else
            {
                switch (address)
                {
                    case 0xFF00: // key pad
                        if (keyP14)
                        {
                            int value = 0;
                            if (!downKeyPressed)
                            {
                                value |= 0x08;
                            }

                            if (!upKeyPressed)
                            {
                                value |= 0x04;
                            }

                            if (!leftKeyPressed)
                            {
                                value |= 0x02;
                            }

                            if (!rightKeyPressed)
                            {
                                value |= 0x01;
                            }

                            return value;
                        }
                        else if (keyP15)
                        {
                            int value = 0;
                            if (!startButtonPressed)
                            {
                                value |= 0x08;
                            }

                            if (!selectButtonPressed)
                            {
                                value |= 0x04;
                            }

                            if (!bButtonPressed)
                            {
                                value |= 0x02;
                            }

                            if (!aButtonPressed)
                            {
                                value |= 0x01;
                            }

                            return value;
                        }

                        break;
                    case 0xFF04: // Timer divider
                        return cpu.ticks & 0xFF;
                    case 0xFF05: // Timer counter
                        return timerCounter & 0xFF;
                    case 0xFF06: // Timer modulo
                        return timerModulo & 0xFF;
                    case 0xFF07:
                        {
                            // Time Control
                            int value = 0;
                            if (timerRunning)
                            {
                                value |= 0x04;
                            }

                            value |= (int)timerFrequency;
                            return value;
                        }
                    case 0xFF0F:
                        {
                            // Interrupt Flag (an interrupt request)
                            int value = 0;
                            if (cpu.keyPressedInterruptRequested)
                            {
                                value |= 0x10;
                            }

                            if (cpu.serialIOTransferCompleteInterruptRequested)
                            {
                                value |= 0x08;
                            }

                            if (cpu.timerOverflowInterruptRequested)
                            {
                                value |= 0x04;
                            }

                            if (cpu.lcdcInterruptRequested)
                            {
                                value |= 0x02;
                            }

                            if (cpu.vBlankInterruptRequested)
                            {
                                value |= 0x01;
                            }

                            return value;
                        }
                    case 0xFF40:
                        {
                            // LCDC control
                            int value = 0;
                            if (ppu.lcdControlOperationEnabled)
                            {
                                value |= 0x80;
                            }

                            if (ppu.windowTileMapDisplaySelect)
                            {
                                value |= 0x40;
                            }

                            if (ppu.windowDisplayed)
                            {
                                value |= 0x20;
                            }

                            if (ppu.backgroundAndWindowTileDataSelect)
                            {
                                value |= 0x10;
                            }

                            if (ppu.backgroundTileMapDisplaySelect)
                            {
                                value |= 0x08;
                            }

                            if (ppu.largeSprites)
                            {
                                value |= 0x04;
                            }

                            if (ppu.spritesDisplayed)
                            {
                                value |= 0x02;
                            }

                            if (ppu.backgroundDisplayed)
                            {
                                value |= 0x01;
                            }

                            return value;
                        }
                    case 0xFF41:
                        {
                            // LCDC Status
                            int value = 0;
                            if (ppu.lcdcLycLyCoincidenceInterruptEnabled)
                            {
                                value |= 0x40;
                            }

                            if (ppu.lcdcOamInterruptEnabled)
                            {
                                value |= 0x20;
                            }

                            if (ppu.lcdcVBlankInterruptEnabled)
                            {
                                value |= 0x10;
                            }

                            if (ppu.lcdcHBlankInterruptEnabled)
                            {
                                value |= 0x08;
                            }

                            if (ppu.ly == ppu.lyCompare)
                            {
                                value |= 0x04;
                            }

                            value |= (int)ppu.lcdcMode;
                            return value;
                        }
                    case 0xFF42: // Scroll Y
                        return ppu.scrollY;
                    case 0xFF43: // Scroll X
                        return ppu.scrollX;
                    case 0xFF44: // LY
                        return ppu.ly;
                    case 0xFF45: // LY Compare
                        return ppu.lyCompare;
                    case 0xFF47:
                        {
                            // Background palette
                            ppu.invalidateAllBackgroundTilesRequest = true;
                            int value = 0;
                            for (int i = 3; i >= 0; i--)
                            {
                                value <<= 2;
                                if(ppu.backgroundPalette[i] == ppu.BLACK) value |= 3;
                                if (ppu.backgroundPalette[i] == ppu.DARK_GRAY) value |= 2;
                                if (ppu.backgroundPalette[i] == ppu.LIGHT_GRAY) value |= 1;
                            }

                            return value;
                        }
                    case 0xFF48:
                        {
                            // Object palette 0
                            ppu.invalidateAllSpriteTilesRequest = true;
                            int value = 0;
                            for (int i = 3; i >= 0; i--)
                            {
                                value <<= 2;
                                if (ppu.objectPalette0[i] == ppu.BLACK) value |= 3;
                                if (ppu.objectPalette0[i] == ppu.DARK_GRAY) value |= 2;
                                if (ppu.objectPalette0[i] == ppu.LIGHT_GRAY) value |= 1;
                            }

                            return value;
                        }
                    case 0xFF49:
                        {
                            // Object palette 1
                            ppu.invalidateAllSpriteTilesRequest = true;
                            int value = 0;
                            for (int i = 3; i >= 0; i--)
                            {
                                value <<= 2;
                                if (ppu.objectPalette1[i] == ppu.BLACK) value |= 3;
                                if (ppu.objectPalette1[i] == ppu.DARK_GRAY) value |= 2;
                                if (ppu.objectPalette1[i] == ppu.LIGHT_GRAY) value |= 1;
                            }

                            return value;
                        }
                    case 0xFF4A: // Window Y
                        return ppu.windowY;
                    case 0xFF4B: // Window X
                        return ppu.windowX;
                    case 0xFFFF:
                        {
                            // Interrupt Enable
                            int value = 0;
                            if (cpu.keyPressedInterruptEnabled)
                            {
                                value |= 0x10;
                            }

                            if (cpu.serialIOTransferCompleteInterruptEnabled)
                            {
                                value |= 0x08;
                            }

                            if (cpu.timerOverflowInterruptEnabled)
                            {
                                value |= 0x04;
                            }

                            if (cpu.lcdcInterruptEnabled)
                            {
                                value |= 0x02;
                            }

                            if (cpu.vBlankInterruptEnabled)
                            {
                                value |= 0x01;
                            }

                            return value;
                        }
                }
            }

            return 0;
        }

        public void KeyChanged(char keyCode, bool pressed)
        {
            switch (keyCode)
            {
                case 'b':
                    bButtonPressed = pressed;
                    break;
                case 'a':
                    aButtonPressed = pressed;
                    break;
                case 's':
                    startButtonPressed = pressed;
                    break;
                case 'c':
                    selectButtonPressed = pressed;
                    break;
                case 'u':
                    upKeyPressed = pressed;
                    break;
                case 'd':
                    downKeyPressed = pressed;
                    break;
                case 'l':
                    leftKeyPressed = pressed;
                    break;
                case 'r':
                    rightKeyPressed = pressed;
                    break;
            }

            if (cpu.keyPressedInterruptEnabled)
            {
                cpu.keyPressedInterruptRequested = true;
            }
        }
    }
}
