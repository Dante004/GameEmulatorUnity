using System;
using System.IO;
using Emulator.Audio;
using Emulator.Cartridge;
using Emulator.Debugger;
using Emulator.Graphics;
using Emulator.Timers;

namespace Emulator.Memories {
	public class Memory {

		public event Action<Memory, ushort> OnMemoryWritten;

        private byte[] _bios = {};
        private byte[] _highRam = new byte[256];
        private byte[] _workRam = new byte[8 * 1024];
        private byte[] _io = new byte[256];
        private byte[] _vram = new byte[8 * 1024];
        private byte[] _oam = new byte[256];
        private ICartridge _rom;

        private GbVersion _version;

        private byte _joypadButtons = 0x0F;
		private byte _joypadDirections = 0x0F;

        public bool inBios = true;

        public SoundChip soundChip;
        private readonly byte[] _soundRegisters = new byte[0x30];

        public PPU ppu;
        public Timer timer;

		public Memory(GbVersion version)
		{
            _version = version;
            _joypadButtons = 0x0F;
			_joypadDirections = 0x0F;

            soundChip = new SoundChip();
            /*
            //IO starts in general with FF (bgb)
            for (int i = 0xFF00; i < 0xFF4C; i++) {
				Write((ushort)i, (byte)0xFF);
			}
            */
        }


		public void LoadRom(string name)
        {
            var fileInfo = new FileInfo(name);
            var rom = new byte[fileInfo.Length];
            var fileStream = fileInfo.OpenRead();
            fileStream.Read(rom, 0, rom.Length);
            fileStream.Close();
            var game = Game.Load(rom);
            _rom = game.cartridge;
        }


        public byte Read(ushort address)
        {
            var result = (byte) 0;

            //Exit bios when address is 0x0100
            inBios = (inBios && address != 0x0100);

            if (inBios && address < 0x0100)
            {
                result = _bios[address];
            }
            //Switchable ROM
            else if (address < 0x8000 || (address >= 0xA000 && address <= 0xBFFF))
            {
                result = _rom.ReadByte(address);
            }
            else if (address >= 0x8000 && address <= 0x9FFF)
            {
                result = _vram[address - 0x8000];
            }
            else if (address >= 0xC000 && address <= 0xDFFF)
            {
                result = _workRam[address - 0xC000];
            }
            else if (address >= 0xE000 && address <= 0xFDFF)
            {
                result = _workRam[address - 0xC000];
            }
            else if (address >= 0xFE00 && address <= 0xFEFF)
            {
                result = _oam[address - 0xFE00];
            }
            else if (address >= 0xFF80 && address <= 0xFFFE)
            {
                result = _highRam[0xFF & address];
            }
            else if (address >= 0xFF10 && address <= 0xFF3F)
            {
                result = _soundRegisters[address - 0xFF10];
            }
            else
            {
                switch (address & 0xFF)
                {
                    case 0x00:
                        var tmp = _io[0];
                        var h = tmp & 0xF0;
                        var l = tmp & 0x0F;

                        //Select direction
                        if ((h & 0x20) != 0x00)
                        {
                            l = _joypadDirections;
                        }
                        else if ((h & 0x10) != 0x00)
                        {
                            l = _joypadButtons;
                        }

                        result = (byte)(h + l);
                        break;
                    /*
                    case 0x40:
                    case 0x41:
                    case 0x42:
                    case 0x43:
                    case 0x44:
                    case 0x45:
                    case 0x47:
                    case 0x48:
                    case 0x49:
                    case 0x4A:
                    case 0x4B:
                    case 0x4F:
                    case 0x68:
                    case 0x69:
                    case 0x6A:
                    case 0x6B:
                        result = ppu.ReadRegister((byte) (address & 0xFF));
                        break;
                        */
                    default:
                        result = _io[address - 0xFF00];
                        break;
                }
            }
            return result;
        }

        public ushort ReadW(ushort address) {
			var l = (ushort)(Read(address));
			var h = (ushort)(Read((ushort)(address + 1)));
			return (ushort)((h << 8 &0xFF00) + l);
		}


        public void Write(ushort address, byte value, bool allowReadOnlyWrite = false) {

			var allowWrite = true;

			//ROM area
			if (address < 0x8000 || (address >= 0xA000 && address <= 0xBFFF)) {
                _rom.WriteByte(address,value);

            }
            else if (address >= 0xC000 && address <= 0xDFFF)
            {
                _workRam[address - 0xC000] = value;
            }
            else if (address >= 0xE000 && address <= 0xFDFF)
            {
                _workRam[address - 0xE000] = value;
            }
            else if (address >= 0x8000 && address <= 0x9FFF)
            {
                _vram[address - 0x8000] = value;
            }
            else if (address >= 0xFE00 && address <= 0xFE9F)
            {
                _oam[address - 0xFE00] = value;
            }
            else if (address >= 0xFF80 && address <= 0xFFFE)
            {
                _highRam[address - 0xFF80] = value;
            }
            //Joypad
			else if (address == 0xFF00) {
				value = (byte)((_io[0] & 0x0F) + (value & 0xF0));
			}
			//Divider, reset if a write is done here
			else if (address == 0xFF04) {
				value = allowReadOnlyWrite ? value : (byte)0x00;
			}
			//LY, reset if a write is done here
			else if (address == 0xFF44) {
				value = allowReadOnlyWrite ? value : (byte)0x00;
			}
            else
            {
                switch (address)
                {
                    case 0xFF10:
                        soundChip.channel1.SetSweep(
                            (value & 0x70) >> 4,
                            (value & 0x07),
                            (value & 0x08) == 1);
                        _soundRegisters[0x10 - 0x10] = value;
                        break;

                    case 0xFF11:           // Sound channel 1, length and wave duty
                        soundChip.channel1.SetDutyCycle((value & 0xC0) >> 6);
                        soundChip.channel1.SetLength(value & 0x3F);
                        _soundRegisters[0x11 - 0x10] = value;
                        break;

                    case 0xFF12:           // Sound channel 1, volume envelope
                        soundChip.channel1.SetEnvelope(
                            (value & 0xF0) >> 4,
                            (value & 0x07),
                            (value & 0x08) == 8);
                        _soundRegisters[0x12 - 0x10] = value;
                        break;

                    case 0xFF13:           // Sound channel 1, frequency low
                        _soundRegisters[0x13 - 0x10] = value;
                        soundChip.channel1.SetFrequency(
                            ((_soundRegisters[0x14 - 0x10] & 0x07) << 8) + _soundRegisters[0x13 - 0x10]);
                        break;

                    case 0xFF14:           // Sound channel 1, frequency high
                        _soundRegisters[0x14 - 0x10] = value;

                        if ((_soundRegisters[0x14 - 0x10] & 0x80) != 0)
                        {
                            soundChip.channel1.SetLength(_soundRegisters[0x11 - 0x10] & 0x3F);
                            soundChip.channel1.SetEnvelope(
                                (_soundRegisters[0x12 - 0x10] & 0xF0) >> 4,
                                (_soundRegisters[0x12 - 0x10] & 0x07),
                                (_soundRegisters[0x12 - 0x10] & 0x08) == 8);
                        }
                        if ((_soundRegisters[0x14 - 0x10] & 0x40) == 0)
                        {
                            soundChip.channel1.SetLength(-1);
                        }

                        soundChip.channel1.SetFrequency(
                            ((_soundRegisters[0x14 - 0x10] & 0x07) << 8) + _soundRegisters[0x13 - 0x10]);

                        break;

                    case 0xFF17:           // Sound channel 2, volume envelope
                        soundChip.channel2.SetEnvelope(
                            (value & 0xF0) >> 4,
                            value & 0x07,
                            (value & 0x08) == 8);
                        _soundRegisters[0x17 - 0x10] = value;
                        break;

                    case 0xFF18:           // Sound channel 2, frequency low
                        _soundRegisters[0x18 - 0x10] = value;
                        soundChip.channel2.SetFrequency(
                            ((_soundRegisters[0x19 - 0x10] & 0x07) << 8) + _soundRegisters[0x18 - 0x10]);
                        break;

                    case 0xFF19:           // Sound channel 2, frequency high
                        _soundRegisters[0x19 - 0x10] = value;

                        if ((value & 0x80) != 0)
                        {
                            soundChip.channel2.SetLength(_soundRegisters[0x21 - 0x10] & 0x3F);
                            soundChip.channel2.SetEnvelope(
                                (_soundRegisters[0x17 - 0x10] & 0xF0) >> 4,
                                (_soundRegisters[0x17 - 0x10] & 0x07),
                                (_soundRegisters[0x17 - 0x10] & 0x08) == 8);
                        }
                        if ((_soundRegisters[0x19 - 0x10] & 0x40) == 0)
                        {
                            soundChip.channel2.SetLength(-1);
                        }
                        soundChip.channel2.SetFrequency(
                            ((_soundRegisters[0x19 - 0x10] & 0x07) << 8) + _soundRegisters[0x18 - 0x10]);
                        break;

                    case 0xFF16:           // Sound channel 2, length and wave duty
                        soundChip.channel2.SetDutyCycle((value & 0xC0) >> 6);
                        soundChip.channel2.SetLength(value & 0x3F);
                        _soundRegisters[0x16 - 0x10] = value;
                        break;

                    case 0xFF1A:           // Sound channel 3, on/off
                        if ((value & 0x80) != 0)
                        {
                            soundChip.channel3.SetVolume((_soundRegisters[0x1C - 0x10] & 0x60) >> 5);
                        }
                        else
                        {
                            soundChip.channel3.SetVolume(0);
                        }
                        _soundRegisters[0x1A - 0x10] = value;
                        break;

                    case 0xFF1B:           // Sound channel 3, length
                        _soundRegisters[0x1B - 0x10] = value;
                        soundChip.channel3.SetLength(value);
                        break;

                    case 0xFF1C:           // Sound channel 3, volume
                        _soundRegisters[0x1C - 0x10] = value;
                        soundChip.channel3.SetVolume((value & 0x60) >> 5);
                        break;

                    case 0xFF1D:           // Sound channel 3, frequency lower 8-bit
                        _soundRegisters[0x1D - 0x10] = value;
                        soundChip.channel3.SetFrequency(
                        ((_soundRegisters[0x1E - 0x10] & 0x07) << 8) + _soundRegisters[0x1D - 0x10]);
                        break;

                    case 0xFF1E:           // Sound channel 3, frequency higher 3-bit
                        _soundRegisters[0x1E - 0x10] = value;
                        if ((_soundRegisters[0x19 - 0x10] & 0x80) != 0)
                        {
                            soundChip.channel3.SetLength(_soundRegisters[0x1B - 0x10]);
                        }
                        soundChip.channel3.SetFrequency(
                            ((_soundRegisters[0x1E - 0x10] & 0x07) << 8) + _soundRegisters[0x1D - 0x10]);
                        break;

                    case 0xFF20:           // Sound channel 4, length
                        soundChip.channel4.SetLength(value & 0x3F);
                        _soundRegisters[0x20 - 0x10] = value;
                        break;

                    case 0xFF21:           // Sound channel 4, volume envelope
                        soundChip.channel4.SetEnvelope(
                        (value & 0xF0) >> 4,
                        (value & 0x07),
                        (value & 0x08) == 8);
                        _soundRegisters[0x21 - 0x10] = value;
                        break;

                    case 0xFF22:           // Sound channel 4, polynomial parameters
                        soundChip.channel4.SetParameters(
                        (value & 0x07),
                        (value & 0x08) == 8,
                        (value & 0xF0) >> 4);
                        _soundRegisters[0x22 - 0x10] = value;
                        break;

                    case 0xFF23:          // Sound channel 4, initial/consecutive
                        _soundRegisters[0x23 - 0x10] = value;
                        if ((value & 0x80) != 0)
                        {
                            soundChip.channel4.SetLength(_soundRegisters[0x20 - 0x10] & 0x3F);
                        }
                        else if (((value & 0x80) & 0x40) == 0)
                        {
                            soundChip.channel4.SetLength(-1);
                        }
                        break;
                    case 0xFF24:
                        // TODO volume
                        break;
                    case 0xFF25:           // Stereo select
                        int chanData;
                        _soundRegisters[0x25 - 0x10] = value;

                        chanData = 0;
                        if ((value & 0x01) != 0)
                        {
                            chanData |= SquareWave.ChannelLeft;
                        }
                        if ((value & 0x10) != 0)
                        {
                            chanData |= SquareWave.ChannelRight;
                        }
                        soundChip.channel1.SetChannel(chanData);

                        chanData = 0;
                        if ((value & 0x02) != 0)
                        {
                            chanData |= SquareWave.ChannelLeft;
                        }
                        if ((value & 0x20) != 0)
                        {
                            chanData |= SquareWave.ChannelRight;
                        }
                        soundChip.channel2.SetChannel(chanData);

                        chanData = 0;
                        if ((value & 0x04) != 0)
                        {
                            chanData |= VoluntaryWave.ChannelLeft;
                        }
                        if ((value & 0x40) != 0)
                        {
                            chanData |= VoluntaryWave.ChannelRight;
                        }
                        soundChip.channel3.SetChannel(chanData);

                        chanData = 0;
                        if ((value & 0x08) != 0)
                        {
                            chanData |= Noise.ChannelLeft;
                        }
                        if ((value & 0x80) != 0)
                        {
                            chanData |= Noise.ChannelRight;
                        }
                        soundChip.channel4.SetChannel(chanData);

                        break;

                    case 0xFF26:
                        soundChip.soundEnabled = (value & 0x80) == 1;
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
                        soundChip.channel3.SetSamplePair(address - 0xFF30, value);
                        _soundRegisters[address - 0xFF10] = value;
                        break;
                    /*
                    case 0xFF40:
                    case 0xFF41:
                    case 0xFF42:
                    case 0xFF43:
                    case 0xFF44:
                    case 0xFF45:
                    case 0xFF47:
                    case 0xFF48:
                    case 0xFF49:
                    case 0xFF4A:
                    case 0xFF4B:
                    case 0xFF4F:
                    case 0xFF68:
                    case 0xFF69:
                    case 0xFF6A:
                    case 0xFF6B:
                        ppu.WriteRegister((byte)(address & 0xFF), value);
                        break;
                        */
                    default:
                        _io[address - 0xFF00] = value;
                        break;
                }
            }
        }
        

		public void WriteW(ushort address, ushort data) {
			Write(address, (byte)(data & 0x00FF));
			Write((ushort)(address + 1), (byte)((data & 0xFF00) >> 8));
		}


		public void WriteJoypadInfo(byte buttons, byte directions)
		{
			_joypadButtons = buttons;
			_joypadDirections = directions;
		}


		#region Interrupts

		private byte IF {
			get => Read(0xFF0F);
            set => Write(0xFF0F, value);
        }

		private byte IE {
			get => Read(0xFFFF);
            set => Write(0xFFFF, value);
        }

		public bool HasInterrupts() {
			return (IE & IF) != 0;
		}


		public bool CheckInterruptEnabled(InterruptType interruptType)
		{
			var result = false;
			switch (interruptType)
            {
			    case InterruptType.VBlank:
				    result = (IE & 0x01) != 0x00;
				    break;
			    case InterruptType.LCDCStatus:
				    result = (IE & 0x02) != 0x00;
				    break;
			    case InterruptType.TimerOverflow:
				    result = (IE & 0x04) != 0x00;
				    break;
			    case InterruptType.SerialTransferCompletion:
				    result = (IE & 0x08) != 0x00;
				    break;
			    case InterruptType.HighToLowP10P13:
				    result = (IE & 0x10) != 0x00;
				    break;
			}
			return result;
		}


		public void EnableInterrupt(InterruptType interruptType)
		{
			switch (interruptType)
            {
			    case InterruptType.VBlank:
				    IE |= 0x01;
				    break;
			    case InterruptType.LCDCStatus:
				    IE |= 0x02;
				    break;
			    case InterruptType.TimerOverflow:
				    IE |= 0x04;
				    break;
			    case InterruptType.SerialTransferCompletion:
				    IE |= 0x08;
				    break;
			    case InterruptType.HighToLowP10P13:
				    IE |= 0x10;
				    break;
			}
		}


		public void DisableInterrupt(InterruptType interruptType)
		{
			switch (interruptType)
            {
			    case InterruptType.VBlank:
				    IE = (byte)(IE & (~0x01));
				    break;
			    case InterruptType.LCDCStatus:
				    IE = (byte)(IE & (~0x02));
				    break;
			    case InterruptType.TimerOverflow:
				    IE = (byte)(IE & (~0x04));
				    break;
			    case InterruptType.SerialTransferCompletion:
				    IE = (byte)(IE & (~0x08));
				    break;
			    case InterruptType.HighToLowP10P13:
				    IE = (byte)(IE & (~0x10));
				    break;
			}
		}



		public bool CheckInterrupt(InterruptType interruptType)
		{
			var result = CheckInterruptEnabled(interruptType);
			if (result)
            {
				switch (interruptType)
                {
				    case InterruptType.VBlank:
					    result = (IF & 0x01) != 0x00;
					    break;
				    case InterruptType.LCDCStatus:
					    result = (IF & 0x02) != 0x00;
					    break;
				    case InterruptType.TimerOverflow:
					    result = (IF & 0x04) != 0x00;
					    break;
				    case InterruptType.SerialTransferCompletion:
					    result = (IF & 0x08) != 0x00;
					    break;
				    case InterruptType.HighToLowP10P13:
					    result = (IF & 0x10) != 0x00;
					    break;
				}
			}
			return result;
		}


		public void SetInterrupt(InterruptType interruptType)
		{
			switch (interruptType)
            {
			    case InterruptType.VBlank:
                    IF |= 0x01;
				    break;
			    case InterruptType.LCDCStatus:
				    IF |= 0x02;
				    break;
			    case InterruptType.TimerOverflow:
				    IF |= 0x04;
				    break;
			    case InterruptType.SerialTransferCompletion:
				    IF |= 0x08;
				    break;
			    case InterruptType.HighToLowP10P13:
				    IF |= 0x10;
				    break;
			}
		}


		public void ClearInterrupt(InterruptType interruptType)
		{
			switch (interruptType)
            {
			    case InterruptType.VBlank:
				    IF = (byte)(IF & (~0x01));
				    break;
			    case InterruptType.LCDCStatus:
				    IF = (byte)(IF & (~0x02));
				    break;
			    case InterruptType.TimerOverflow:
				    IF = (byte)(IF & (~0x04));
				    break;
			    case InterruptType.SerialTransferCompletion:
				    IF = (byte)(IF & (~0x08));
				    break;
			    case InterruptType.HighToLowP10P13:
				    IF = (byte)(IF & (~0x10));
				    break;
			}
		}
        #endregion
	}
}