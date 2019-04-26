#pragma warning disable 0414

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Emulator;
using UnityEngineInternal;

namespace Emulator
{
	public class Console : ConsoleBase
	{
		const float FRAMES_PER_SECOND = 59.7f;
		const int WIDTH = 160;
		const int HEIGHT = 144;
		int MAX_FRAMES_SKIPPED = 10;
		public long FREQUENCY = Stopwatch.Frequency;
		public long TICKS_PER_FRAME = (long)(Stopwatch.Frequency / FRAMES_PER_SECOND);
		private Stopwatch stopwatch = new Stopwatch();
        private CPU cpu;
        private Memory memory;
        private PPU ppu;
		private double scanLineTicks;
		private uint[] pixels = new uint[WIDTH * HEIGHT];
		private Game game;

		public Console(IVideoOutput video,IAudioOutput audio= null) : base(video, audio)
		{
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels [i] = 0xFF000000;
			}

			Video.SetSize(WIDTH, HEIGHT);
		}

		public override void RunNextStep()
		{
			if (stopwatch.ElapsedTicks > TICKS_PER_FRAME)
			{
				UpdateModel(true);
				Video.SetPixels(pixels);
				stopwatch.Reset();
				stopwatch.Start();
			} else
			{
				UpdateModel(false);
			}
		}

		public override void LoadRom(string name)
		{
			game = ROMLoader.Load(name);
			memory = new Memory();
            cpu = new CPU(memory);
            ppu = new PPU(memory);
            if (Audio != null)
                memory.SoundChip.SetSampleRate(Audio.GetOutputSampleRate());
            memory.cartridge = game.cartridge;
			cpu.PowerUp();

			stopwatch.Reset();
			stopwatch.Start();
		}

		public override void SetInput(Button button, bool pressed)
		{
			char keyCode = ' ';
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

			memory.KeyChanged(keyCode, pressed);
		}

		private void UpdateModel(bool updateBitmap)
		{
			if (updateBitmap)
			{
				uint[,] backgroundBuffer = ppu.backgroundBuffer;
				uint[,] windowBuffer = ppu.windowBuffer;
				byte[] oam = memory.oam;

				for (int y = 0, pixelIndex = 0; y < HEIGHT; ++y)
				{
					ppu.ly = y;
                    ppu.lcdcMode = LcdcModeType.SearchingOamRam;
					if (cpu.lcdcInterruptEnabled
						&& (ppu.lcdcOamInterruptEnabled
						|| (ppu.lcdcLycLyCoincidenceInterruptEnabled && ppu.lyCompare == y)))
					{
						cpu.lcdcInterruptRequested = true;
					}
					ExecuteProcessor(800);
					ppu.lcdcMode = LcdcModeType.TransferingData;
					ExecuteProcessor(1720);

					ppu.UpdateWindow();
					ppu.UpdateBackground();
					ppu.UpdateSpriteTiles();

					bool backgroundDisplayed = ppu.backgroundDisplayed;
					int scrollX = ppu.scrollX;
					int scrollY = ppu.scrollY;
					bool windowDisplayed = ppu.windowDisplayed;
					int windowX = ppu.windowX - 7;
					int windowY = ppu.windowY;

					for (int x = 0; x < WIDTH; ++x, ++pixelIndex)
					{
						uint intensity = 0;

						if (backgroundDisplayed)
						{
							intensity = backgroundBuffer [0xFF & (scrollY + y), 0xFF & (scrollX + x)];
						}

						if (windowDisplayed && y >= windowY && y < windowY + HEIGHT && x >= windowX && x < windowX + WIDTH
							&& windowX >= -7 && windowX < WIDTH && windowY >= 0 && windowY < HEIGHT)
						{
							intensity = windowBuffer [y - windowY, x - windowX];
						}

						pixels [pixelIndex] = intensity;
					}

					if (ppu.spritesDisplayed)
					{
						uint[, , ,] spriteTile = ppu.spriteTile;
						if (ppu.largeSprites)
						{
							for (int address = 0; address < WIDTH; address += 4)
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

								int spriteTileIndex0 = 0xFE & oam [address + 2];
								int spriteTileIndex1 = spriteTileIndex0 | 0x01;
								int spriteFlags = oam [address + 3];
								bool spritePriority = (0x80 & spriteFlags) == 0x80;
								bool spriteYFlipped = (0x40 & spriteFlags) == 0x40;
								bool spriteXFlipped = (0x20 & spriteFlags) == 0x20;
								int spritePalette = (0x10 & spriteFlags) == 0x10 ? 1 : 0;

								if (spriteYFlipped)
								{
									int temp = spriteTileIndex0;
									spriteTileIndex0 = spriteTileIndex1;
									spriteTileIndex1 = temp;
								}

								int spriteRow = y - spriteY;
								if (spriteRow >= 0 && spriteRow < 8)
								{
									int screenAddress = (y << 7) + (y << 5) + spriteX;
									for (int x = 0; x < 8; ++x, ++screenAddress)
									{
										int screenX = spriteX + x;
										if (screenX >= 0 && screenX < WIDTH)
										{
											uint color = spriteTile [spriteTileIndex0,
          spriteYFlipped ? 7 - spriteRow : spriteRow,
          spriteXFlipped ? 7 - x : x, spritePalette];
											if (color > 0)
											{
												if (spritePriority)
												{
													if (pixels [screenAddress] == 0xFFFFFFFF)
													{
														pixels [screenAddress] = color;
													}
												} else
												{
													pixels [screenAddress] = color;
												}
											}
										}
									}
									continue;
								}

								spriteY += 8;

								spriteRow = y - spriteY;
								if (spriteRow >= 0 && spriteRow < 8)
								{
									int screenAddress = (y << 7) + (y << 5) + spriteX;
									for (int x = 0; x < 8; ++x, ++screenAddress)
									{
										int screenX = spriteX + x;
										if (screenX >= 0 && screenX < WIDTH)
										{
											uint color = spriteTile [spriteTileIndex1,
          spriteYFlipped ? 7 - spriteRow : spriteRow,
          spriteXFlipped ? 7 - x : x, spritePalette];
											if (color > 0)
											{
												if (spritePriority)
												{
													if (pixels [screenAddress] == 0xFFFFFFFF)
													{
														pixels [screenAddress] = color;
													}
												} else
												{
													pixels [screenAddress] = color;
												}
											}
										}
									}
								}
							}
						} else
						{
							for (int address = 0; address < WIDTH; address += 4)
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

								int spriteTileIndex = oam [address + 2];
								int spriteFlags = oam [address + 3];
								bool spritePriority = (0x80 & spriteFlags) == 0x80;
								bool spriteYFlipped = (0x40 & spriteFlags) == 0x40;
								bool spriteXFlipped = (0x20 & spriteFlags) == 0x20;
								int spritePalette = (0x10 & spriteFlags) == 0x10 ? 1 : 0;

								int spriteRow = y - spriteY;
								int screenAddress = (y << 7) + (y << 5) + spriteX;
								for (int x = 0; x < 8; ++x, ++screenAddress)
								{
									int screenX = spriteX + x;
									if (screenX >= 0 && screenX < WIDTH)
									{
										uint color = spriteTile [spriteTileIndex,
        spriteYFlipped ? 7 - spriteRow : spriteRow,
        spriteXFlipped ? 7 - x : x, spritePalette];
										if (color > 0)
										{
											if (spritePriority)
											{
												if (pixels [screenAddress] == 0xFFFFFFFF)
												{
													pixels [screenAddress] = color;
												}
											} else
											{
												pixels [screenAddress] = color;
											}
										}
									}
								}
							}
						}
					}

					ppu.lcdcMode = LcdcModeType.HBlank;
					if (cpu.lcdcInterruptEnabled && ppu.lcdcHBlankInterruptEnabled)
					{
						cpu.lcdcInterruptRequested = true;
					}
					ExecuteProcessor(2040);
					AddTicksPerScanLine();
				}
			} else
			{
				for (int y = 0; y < HEIGHT; ++y)
				{
					ppu.ly = y;
					ppu.lcdcMode = LcdcModeType.SearchingOamRam;
					if (cpu.lcdcInterruptEnabled
						&& (ppu.lcdcOamInterruptEnabled
						|| (ppu.lcdcLycLyCoincidenceInterruptEnabled && ppu.lyCompare == y)))
					{
						cpu.lcdcInterruptRequested = true;
					}
					ExecuteProcessor(800);
					ppu.lcdcMode = LcdcModeType.TransferingData;
					ExecuteProcessor(1720);
					ppu.lcdcMode = LcdcModeType.HBlank;
					if (cpu.lcdcInterruptEnabled && ppu.lcdcHBlankInterruptEnabled)
					{
						cpu.lcdcInterruptRequested = true;
					}
					ExecuteProcessor(2040);
					AddTicksPerScanLine();
				}
			}

			ppu.lcdcMode = LcdcModeType.VBlank;
			if (cpu.vBlankInterruptEnabled)
			{
				cpu.vBlankInterruptRequested = true;
			}
			if (cpu.lcdcInterruptEnabled && ppu.lcdcVBlankInterruptEnabled)
			{
				cpu.lcdcInterruptRequested = true;
			}
			for (int y = 144; y <= 153; ++y)
			{
				ppu.ly = y;
				if (cpu.lcdcInterruptEnabled && ppu.lcdcLycLyCoincidenceInterruptEnabled
					&& ppu.lyCompare == y)
				{
					cpu.lcdcInterruptRequested = true;
				}
				ExecuteProcessor(4560);
				AddTicksPerScanLine();
			}
            if (Audio != null)
                memory.SoundChip.OutputSound(Audio);
        }

		private void AddTicksPerScanLine()
		{
			switch (memory.timerFrequency)
			{
				case TimerFrequencyType.hz4096:
					scanLineTicks += 0.44329004329004329004329004329004;
					break;
				case TimerFrequencyType.hz16384:
					scanLineTicks += 1.7731601731601731601731601731602;
					break;
				case TimerFrequencyType.hz65536:
					scanLineTicks += 7.0926406926406926406926406926407;
					break;
				case TimerFrequencyType.hz262144:
					scanLineTicks += 28.370562770562770562770562770563;
					break;
			}
			while (scanLineTicks >= 1.0)
			{
				scanLineTicks -= 1.0;
				if (memory.timerCounter == 0xFF)
				{
					memory.timerCounter = memory.timerModulo;
					if (cpu.lcdcInterruptEnabled && cpu.timerOverflowInterruptEnabled)
					{
						cpu.timerOverflowInterruptRequested = true;
					}
				} else
				{
					memory.timerCounter++;
				}
			}
		}

		private void ExecuteProcessor(int maxTicks)
		{
			do
			{
				cpu.Step();
				if (cpu.halted)
				{
					cpu.ticks = ((maxTicks - cpu.ticks) & 0x03);
					return;
				}
			} while (cpu.ticks < maxTicks);
			cpu.ticks -= maxTicks;
		}
	}
}
