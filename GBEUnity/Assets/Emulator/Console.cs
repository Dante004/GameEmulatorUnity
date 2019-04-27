using System;
using System.Diagnostics;
using Emulator.Cartridges;

namespace Emulator
{
	public class Console : ConsoleBase
	{
		private const float FramesPerSecond = 59.7f;
		private const int Width = 160;
		private const int Height = 144;
		public long ticksPerFrame = (long)(Stopwatch.Frequency / FramesPerSecond);
		private readonly Stopwatch _stopwatch = new Stopwatch();
        private CPU _cpu;
        private Memory _memory;
        private PPU _ppu;
		private double _scanLineTicks;
		private readonly uint[] _pixels = new uint[Width * Height];
		private Game _game;

		public Console(IVideoOutput video,IAudioOutput audio= null) : base(video, audio)
		{
			for (var i = 0; i < _pixels.Length; i++)
			{
				_pixels [i] = 0xFF000000;
			}

			Video.SetSize(Width, Height);
		}

		public override void RunNextStep()
		{
			if (_stopwatch.ElapsedTicks > ticksPerFrame)
			{
				UpdateModel(true);
				Video.SetPixels(_pixels);
				_stopwatch.Reset();
				_stopwatch.Start();
			} else
			{
				UpdateModel(false);
			}
		}

		public override void LoadRom(string name)
		{
			_game = RomLoader.Load(name);
			_memory = new Memory();
            _cpu = new CPU(_memory);
            _ppu = new PPU(_memory);
            if (Audio != null)
                _memory.SoundChip.SetSampleRate(Audio.GetOutputSampleRate());
            _memory.cartridge = _game.cartridge;
			_cpu.PowerUp();

			_stopwatch.Reset();
			_stopwatch.Start();
		}

		public override void SetInput(Button button, bool pressed)
		{
			var keyCode = ' ';
			switch (button)
			{
				case Button.Up:
					keyCode = 'u';
					break;
				case Button.Down:
					keyCode = 'd';
					break;
				case Button.Left:
					keyCode = 'l';
					break;
				case Button.Right:
					keyCode = 'r';
					break;
				case Button.A:
					keyCode = 'a';
					break;
				case Button.B:
					keyCode = 'b';
					break;
				case Button.Start:
					keyCode = 's';
					break;
				case Button.Select:
					keyCode = 'c';
					break;
			}

			_memory.KeyChanged(keyCode, pressed);
		}

		private void UpdateModel(bool updateBitmap)
		{
			if (updateBitmap)
			{
				var backgroundBuffer = _ppu.backgroundBuffer;
				var windowBuffer = _ppu.windowBuffer;
				var oam = _memory.oam;

				for (int y = 0, pixelIndex = 0; y < Height; ++y)
				{
					_ppu.ly = y;
                    _ppu.lcdcMode = LcdcModeType.SearchingOamRam;
					if (_cpu.lcdcInterruptEnabled
						&& (_ppu.lcdcOamInterruptEnabled
						|| (_ppu.lcdcLycLyCoincidenceInterruptEnabled && _ppu.lyCompare == y)))
					{
						_cpu.lcdcInterruptRequested = true;
					}
					ExecuteProcessor(800);
					_ppu.lcdcMode = LcdcModeType.TransferingData;
					ExecuteProcessor(1720);

					_ppu.UpdateWindow();
					_ppu.UpdateBackground();
					_ppu.UpdateSpriteTiles();

					var backgroundDisplayed = _ppu.backgroundDisplayed;
					var scrollX = _ppu.scrollX;
					var scrollY = _ppu.scrollY;
					var windowDisplayed = _ppu.windowDisplayed;
					var windowX = _ppu.windowX - 7;
					var windowY = _ppu.windowY;

					for (var x = 0; x < Width; ++x, ++pixelIndex)
					{
						uint intensity = 0;

						if (backgroundDisplayed)
						{
							intensity = backgroundBuffer [0xFF & (scrollY + y), 0xFF & (scrollX + x)];
						}

						if (windowDisplayed && y >= windowY && y < windowY + Height && x >= windowX && x < windowX + Width
							&& windowX >= -7 && windowX < Width && windowY >= 0 && windowY < Height)
						{
							intensity = windowBuffer [y - windowY, x - windowX];
						}

						_pixels [pixelIndex] = intensity;
					}

					if (_ppu.spritesDisplayed)
					{
						var spriteTile = _ppu.spriteTile;
						if (_ppu.largeSprites)
						{
							for (var address = 0; address < Width; address += 4)
							{
								int spriteY = oam [address];
								int spriteX = oam [address + 1];
								if (spriteY == 0 || spriteX == 0 || spriteY >= 160 || spriteX >= 168)
								{
									continue;
								}
								spriteY -= 16;
								if (spriteY > y || spriteY + 15 < y)
								{
									continue;
								}
								spriteX -= 8;

								var spriteTileIndex0 = 0xFE & oam [address + 2];
								var spriteTileIndex1 = spriteTileIndex0 | 0x01;
								var spriteFlags = oam [address + 3];
								var spritePriority = (0x80 & spriteFlags) == 0x80;
								var spriteYFlipped = (0x40 & spriteFlags) == 0x40;
								var spriteXFlipped = (0x20 & spriteFlags) == 0x20;
								var spritePalette = (0x10 & spriteFlags) == 0x10 ? 1 : 0;

								if (spriteYFlipped)
								{
									var temp = spriteTileIndex0;
									spriteTileIndex0 = spriteTileIndex1;
									spriteTileIndex1 = temp;
								}

								var spriteRow = y - spriteY;
								if (spriteRow >= 0 && spriteRow < 8)
								{
									var screenAddress = (y << 7) + (y << 5) + spriteX;
									for (var x = 0; x < 8; ++x, ++screenAddress)
									{
										var screenX = spriteX + x;
										if (screenX >= 0 && screenX < Width)
										{
											var color = spriteTile [spriteTileIndex0,
          spriteYFlipped ? 7 - spriteRow : spriteRow,
          spriteXFlipped ? 7 - x : x, spritePalette];
                                            if (color <= 0) continue;
                                            if (spritePriority)
                                            {
                                                if (_pixels [screenAddress] == 0xFFFFFFFF)
                                                {
                                                    _pixels [screenAddress] = color;
                                                }
                                            } else
                                            {
                                                _pixels [screenAddress] = color;
                                            }
                                        }
									}
									continue;
								}

								spriteY += 8;

								spriteRow = y - spriteY;
                                if (spriteRow < 0 || spriteRow >= 8) continue;
                                {
                                    var screenAddress = (y << 7) + (y << 5) + spriteX;
                                    for (var x = 0; x < 8; ++x, ++screenAddress)
                                    {
                                        var screenX = spriteX + x;
                                        if (screenX < 0 || screenX >= Width) continue;
                                        var color = spriteTile [spriteTileIndex1,
                                            spriteYFlipped ? 7 - spriteRow : spriteRow,
                                            spriteXFlipped ? 7 - x : x, spritePalette];
                                        if (color <= 0) continue;
                                        if (spritePriority)
                                        {
                                            if (_pixels [screenAddress] == 0xFFFFFFFF)
                                            {
                                                _pixels [screenAddress] = color;
                                            }
                                        } else
                                        {
                                            _pixels [screenAddress] = color;
                                        }
                                    }
                                }
                            }
						} else
						{
							for (var address = 0; address < Width; address += 4)
							{
								int spriteY = oam [address];
								int spriteX = oam [address + 1];
								if (spriteY == 0 || spriteX == 0 || spriteY >= 160 || spriteX >= 168)
								{
									continue;
								}
								spriteY -= 16;
								if (spriteY > y || spriteY + 7 < y)
								{
									continue;
								}
								spriteX -= 8;

								var spriteTileIndex = oam [address + 2];
								var spriteFlags = oam [address + 3];
								var spritePriority = (0x80 & spriteFlags) == 0x80;
								var spriteYFlipped = (0x40 & spriteFlags) == 0x40;
								var spriteXFlipped = (0x20 & spriteFlags) == 0x20;
								var spritePalette = (0x10 & spriteFlags) == 0x10 ? 1 : 0;

								var spriteRow = y - spriteY;
								var screenAddress = (y << 7) + (y << 5) + spriteX;
								for (var x = 0; x < 8; ++x, ++screenAddress)
								{
									var screenX = spriteX + x;
                                    if (screenX < 0 || screenX >= Width) continue;
                                    var color = spriteTile [spriteTileIndex,
                                        spriteYFlipped ? 7 - spriteRow : spriteRow,
                                        spriteXFlipped ? 7 - x : x, spritePalette];
                                    if (color <= 0) continue;
                                    if (spritePriority)
                                    {
                                        if (_pixels [screenAddress] == 0xFFFFFFFF)
                                        {
                                            _pixels [screenAddress] = color;
                                        }
                                    } else
                                    {
                                        _pixels [screenAddress] = color;
                                    }
                                }
							}
						}
					}

					_ppu.lcdcMode = LcdcModeType.HBlank;
					if (_cpu.lcdcInterruptEnabled && _ppu.lcdcHBlankInterruptEnabled)
					{
						_cpu.lcdcInterruptRequested = true;
					}
					ExecuteProcessor(2040);
					AddTicksPerScanLine();
				}
			} else
			{
				for (var y = 0; y < Height; ++y)
				{
					_ppu.ly = y;
					_ppu.lcdcMode = LcdcModeType.SearchingOamRam;
					if (_cpu.lcdcInterruptEnabled
						&& (_ppu.lcdcOamInterruptEnabled
						|| (_ppu.lcdcLycLyCoincidenceInterruptEnabled && _ppu.lyCompare == y)))
					{
						_cpu.lcdcInterruptRequested = true;
					}
					ExecuteProcessor(800);
					_ppu.lcdcMode = LcdcModeType.TransferingData;
					ExecuteProcessor(1720);
					_ppu.lcdcMode = LcdcModeType.HBlank;
					if (_cpu.lcdcInterruptEnabled && _ppu.lcdcHBlankInterruptEnabled)
					{
						_cpu.lcdcInterruptRequested = true;
					}
					ExecuteProcessor(2040);
					AddTicksPerScanLine();
				}
			}

			_ppu.lcdcMode = LcdcModeType.VBlank;
			if (_cpu.vBlankInterruptEnabled)
			{
				_cpu.vBlankInterruptRequested = true;
			}
			if (_cpu.lcdcInterruptEnabled && _ppu.lcdcVBlankInterruptEnabled)
			{
				_cpu.lcdcInterruptRequested = true;
			}
			for (var y = 144; y <= 153; ++y)
			{
				_ppu.ly = y;
				if (_cpu.lcdcInterruptEnabled && _ppu.lcdcLycLyCoincidenceInterruptEnabled
					&& _ppu.lyCompare == y)
				{
					_cpu.lcdcInterruptRequested = true;
				}
				ExecuteProcessor(4560);
				AddTicksPerScanLine();
			}
            if (Audio != null)
                _memory.SoundChip.OutputSound(Audio);
        }

		private void AddTicksPerScanLine()
		{
			switch (_memory.timerFrequency)
			{
				case TimerFrequencyType.hz4096:
					_scanLineTicks += 0.44329004329004329004329004329004;
					break;
				case TimerFrequencyType.hz16384:
					_scanLineTicks += 1.7731601731601731601731601731602;
					break;
				case TimerFrequencyType.hz65536:
					_scanLineTicks += 7.0926406926406926406926406926407;
					break;
				case TimerFrequencyType.hz262144:
					_scanLineTicks += 28.370562770562770562770562770563;
					break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
			while (_scanLineTicks >= 1.0)
			{
				_scanLineTicks -= 1.0;
				if (_memory.timerCounter == 0xFF)
				{
					_memory.timerCounter = _memory.timerModulo;
					if (_cpu.lcdcInterruptEnabled && _cpu.timerOverflowInterruptEnabled)
					{
						_cpu.timerOverflowInterruptRequested = true;
					}
				} else
				{
					_memory.timerCounter++;
				}
			}
		}

		private void ExecuteProcessor(int maxTicks)
		{
			do
			{
				_cpu.Step();
                if (!_cpu.halted) continue;
                _cpu.ticks = ((maxTicks - _cpu.ticks) & 0x03);
                return;
            } while (_cpu.ticks < maxTicks);
			_cpu.ticks -= maxTicks;
		}
	}
}
