using System.Collections.Generic;
using Emulator.Memories;
using UnityEngine;

namespace Emulator.Graphics
{
	
	public class PPU
    {
		
		public const int ScreenPixelsWidth = 160;
		public const int ScreenPixelsHeight = 144;

		private const int HorizontalBlankCycles = 204;
		private const int VerticalBlankCycles = 456;
		private const int ScanLinesOamCycles = 80;
		private const int ScanLineVramCycles = 172;

		private const int TotalTitles = 512;

        private readonly Memory _memory;

		//FF40(LCDC)
		public int LcdcBGTileMap => (_memory.Read(0xFF40) & 0x08) == 0 ? 0 : 1;
        public int LcdcWindowTileMap => (_memory.Read(0xFF40) & 0x40) == 0 ? 0 : 1;
        public int LcdcBGWindowTileData => (_memory.Read(0xFF40) & 0x10) == 0 ? 0 : 1;

        public bool LcdcWindowDisplay => (_memory.Read(0xFF40) & 0x20) != 0;
        public bool LcdcSpriteDisplay => (_memory.Read(0xFF40) & 0x02) != 0;
        public bool Lcdc_BGWindowDisplay => (_memory.Read(0xFF40) & 0x01) != 0;

        public SpriteSize Lcdc_SpriteSize => (SpriteSize)(_memory.Read(0xFF40) & 0x04);

        //FF41(STAT)
        private bool StatInterruptLYCEnabled => (_memory.Read((ushort)0xFF41) & 0x40) != 0;
        private bool StatInterruptOAMEnabled => (_memory.Read((ushort)0xFF41) & 0x20) != 0;
        private bool StatInterruptVBlankEnabled => (_memory.Read((ushort)0xFF41) & 0x10) != 0;
        private bool StatInterruptHBlankEnabled => (_memory.Read((ushort)0xFF41) & 0x08) != 0;

        private bool StatCoincidenceFlag {
			get => (_memory.Read((ushort)0xFF41) & 0x04) != 0;
            set { 
				var data = _memory.Read((ushort)0xFF41);
				_memory.Write((ushort)0xFF41, (byte)((data & ~0x04) | (value ? 0x04 : 0x00)));
			}
		}
		private GPUMode StatMode {
			get => (GPUMode)(_memory.Read((ushort)0xFF41) & 0x03);
            set { 
				_memory.Write(0xFF41, (byte)((_memory.Read(0xFF41) & ~0x03) + (byte)value)); 
				CheckLcdInterrupts();
			}
		}

        //FF42(SCY)
        private byte SCY => _memory.Read(0xFF42);

        //FF43(SCX)
        private byte SCX => _memory.Read(0xFF43);

        //FF44(LY)
        private byte LY { 
			get => _memory.Read(0xFF44);
            set { 
				_memory.Write(0xFF44, value, true); 
				StatCoincidenceFlag = (value == LYC);
				if (StatCoincidenceFlag && StatInterruptLYCEnabled) {
					_memory.SetInterrupt(InterruptType.LCDCStatus);
				}
			} 
		}

        //FF45(LYC)
        private byte LYC => _memory.Read(0xFF45);

        //FF46(DMA)
        private byte DMA => _memory.Read(0xFF46);

        //FF47(BGP)
        private byte BGP => _memory.Read(0xFF47);

        //FF48(OBP0)
        private byte OBP0 => _memory.Read(0xFF48);

        //FF49(OBP1)
        private byte OBP1 => _memory.Read(0xFF49);

        //FF4A(WY)
        private byte WY => _memory.Read(0xFF4A);

        //FF4B(WX)
        private byte WX => _memory.Read(0xFF4B);

        private uint clock;

        readonly Color[] buffer;
		public Texture2D ScreenTexture { get; private set; }

		public PPU(Memory memory) 
		{
			_memory = memory;
			_memory.OnMemoryWritten += (Memory m, ushort address) => {
				if (address >= 0x8000 && address <= 0x97FF) {
					UpdateTile(address);
				} else if (address == 0xFF46) {
					OamTransfer();
				}
			};

			StatMode = GPUMode.HBlank;
			LY = 0;
			clock = 0;
			buffer = new Color[ScreenPixelsWidth * ScreenPixelsHeight];

            ScreenTexture = new Texture2D(ScreenPixelsWidth, ScreenPixelsHeight, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
        }

		public void Step(uint opCycles)
		{
			clock += opCycles;

			switch (StatMode) {

			//OAM Read
			    case GPUMode.OAMRead:
				    if (clock >= ScanLinesOamCycles)
                    {
					    clock -= ScanLinesOamCycles;
					    StatMode = GPUMode.VRAMRead;
				    }
				    break;

			//VRAM Read
			    case GPUMode.VRAMRead:
				    if (clock >= ScanLineVramCycles)
                    {
					    clock -= ScanLineVramCycles;
					    StatMode = GPUMode.HBlank;
					    DrawScanLine();
                    }
                    break;

			//HBlank
			    case GPUMode.HBlank:
				    if (clock >= HorizontalBlankCycles)
                    {
					    clock -= HorizontalBlankCycles;
					    LY++;

					    if (LY == (ScreenPixelsHeight - 1))
                        {
						    StatMode = GPUMode.VBlank;
						    _memory.SetInterrupt(InterruptType.VBlank);
						    DrawScreen();
					    } else
                        {
						    StatMode = GPUMode.OAMRead;

                        }                     }
				    break;
			
			//VBlank
			    case GPUMode.VBlank:
				    if (clock >= VerticalBlankCycles)
                    {
					    clock -= VerticalBlankCycles;
					    LY++;

					    if (LY > 153)
                        {
						    StatMode = GPUMode.OAMRead;
						    LY = 0;
					    }
				    }
				    break;
			}
		}

		private void CheckLcdInterrupts()
		{
			var setInterrupt = false;
			setInterrupt = setInterrupt || (StatMode == GPUMode.HBlank && StatInterruptHBlankEnabled);
			setInterrupt = setInterrupt || (StatMode == GPUMode.VBlank && StatInterruptVBlankEnabled);
			setInterrupt = setInterrupt || (StatMode == GPUMode.OAMRead && StatInterruptOAMEnabled);
			if (setInterrupt) {
				_memory.SetInterrupt(InterruptType.LCDCStatus);
			}
		}

		private void DrawScanLine()
		{
			//VRAM size is 32x32 tiles, 1 byte per tile
			var ly = LY;
			var lineY = ly + SCY;
			var lineX = SCX;
			var bufferY = (ScreenPixelsWidth * ScreenPixelsHeight - (ly * ScreenPixelsWidth)) - ScreenPixelsWidth;

			if (Lcdc_BGWindowDisplay) {
				var tileMapAddressOffset = LcdcBGTileMap == 0 ? 0x9800 : 0x9C00;
                int[] tile = null;

				for (var i = 0; i < ScreenPixelsWidth; ++i) {
					if (i == 0 || (lineX & 7) == 0) {
					
						var tileMapY = ((lineY >> 3) & 31);
						var tileMapX = ((lineX >> 3) & 31);

						int nTile = _memory.Read((ushort)(tileMapAddressOffset + (tileMapY << 5) + tileMapX));
						if (LcdcBGWindowTileData == 0) {
							if (nTile > 127) {
								nTile -= 0x100;
							}
							nTile = 256 + nTile;
						}

						if (!_tiles.ContainsKey((uint)(nTile))) {
							continue;
						}
						tile = _tiles[(uint)nTile];
					}

					if (tile == null) {
						continue;
					}

					var tileY = lineY & 7;
					var tileX = lineX & 7;

					buffer[bufferY + i] = ColorForPalette(BGP, tile[(tileY << 3) + tileX]);
					lineX++;
				}
			}

			if (LcdcSpriteDisplay) {
				const int oamAddress = 0xFE00;

                for (var i = 0; i < 40; ++i) {
					var yPosition = _memory.Read((ushort)(oamAddress + i * 4)) - 16;
					var xPosition = _memory.Read((ushort)(oamAddress + i * 4 + 1)) - 8;
					var n = _memory.Read((ushort)(oamAddress + i * 4 + 2));
					var flags = _memory.Read((ushort)(oamAddress + i * 4 + 3));

					var maxSprites = Lcdc_SpriteSize == SpriteSize.Size8x8 ? 1 : 2;
					for (var j = 0; j < maxSprites; ++j) {

						n = (byte)(n + j);
						yPosition += 8 * j;

						if (!_tiles.ContainsKey(n)) {
							continue;
						}

						if (ly >= yPosition && yPosition + 8 > ly) {
						
							var palette = (flags & 0x10) == 0 ? OBP0 : OBP1;
							var xFlip = (flags & 0x20) != 0;
							var yFlip = (flags & 0x40) != 0;
							var priority = (flags & 0x80) == 0 ? 0 : 1;

							var spriteRow = (ly - yPosition);
							if (yFlip) {
								spriteRow = 7 - spriteRow;
							}


							for (var x = 0; x < 8; ++x) {
								var xCoordinatesSprite = xFlip ? 7 - x : x;
								var xCoordinatesBuffer = bufferY + xPosition + x;
								var pixelColor = _tiles[n][spriteRow * 8 + xCoordinatesSprite];
								if (((xPosition + xCoordinatesSprite) >= 0) && ((xPosition + xCoordinatesSprite) < ScreenPixelsWidth)
								   && pixelColor != 0
								   && (priority == 0 || buffer[xCoordinatesBuffer] == _colors[0])) {
									buffer[xCoordinatesBuffer] = ColorForPalette(palette, pixelColor);
								}
							}
						}
					}

				}
			}

			var wx = WX - 7;
			var wy = WY;
			lineY = ly;
			lineX = 0;

			if (LcdcWindowDisplay && ly >= wy) {
				var tileMapAddressOffset = LcdcWindowTileMap == 0 ? 0x9800 : 0x9C00;
                int[] tile = null;

				for (var i = wx; i < ScreenPixelsWidth; ++i) {
					if (((i - wx) & 7) == 0) {
						var tileMapY = (((ly - wy) >> 3) & 31);
						var tileMapX = (((i - wx) >> 3) & 31);
						int nTile = _memory.Read((ushort)(tileMapAddressOffset + (tileMapY << 5) + tileMapX));
						if (LcdcBGWindowTileData == 0) {
							if (nTile > 127) {
								nTile -= 0x100;
							}
							nTile = 256 + nTile;
						}

						if (!_tiles.ContainsKey((uint)(nTile))) {
							continue;
						}
						tile = _tiles[(uint)nTile];
					}

					if (tile == null) {
						continue;
					}

					var tileY = (lineY & 7);
					var tileX = (lineX & 7);

					buffer[bufferY + i] = ColorForPalette(BGP, tile[(tileY << 3) + tileX]);
					lineX++;
				}
			}
		}

		private void DrawScreen()
		{
			ScreenTexture.SetPixels(0, 0, ScreenPixelsWidth, ScreenPixelsHeight, buffer);
			ScreenTexture.Apply();
		}


        readonly Color[] _colors = {
			new Color(224.0f / 255.0f, 248.0f / 255.0f, 208.0f / 255.0f),
			new Color(136.0f / 255.0f, 192.0f / 255.0f, 112.0f / 255.0f),
			new Color(52.0f / 255.0f, 104.0f / 255.0f, 86.0f / 255.0f),
			new Color(8.0f / 255.0f, 24.0f / 255.0f, 32.0f / 255.0f)
		};


        private Color ColorForPalette(byte palette, int colorIdx)
		{
			return _colors[((palette & (0x03 << (colorIdx * 2))) >> (colorIdx * 2))];
		}


        private readonly Dictionary<uint, int[]> _tiles = new Dictionary<uint, int[]>();
		private void UpdateTile(uint address)
		{
			var n = (address - 0x8000) / 16;
			var tileBaseAddress = 0x8000 + n * 16;
			var tileRow = (address - tileBaseAddress) / 2;
			var tileRowAddress = tileBaseAddress + tileRow * 2;
			
			if (!_tiles.ContainsKey(n)) {
				_tiles[n] = new int[8 * 8];
			}

			var b1 = _memory.Read((ushort)(tileRowAddress));
			var b2 = _memory.Read((ushort)(tileRowAddress + 1));

			var tile = _tiles[n];
			tile[tileRow * 8] = ((b1 & 0x80) >> 7) + ((b2 & 0x80) >> 6);
			tile[tileRow * 8 + 1] = ((b1 & 0x40) >> 6) + ((b2 & 0x40) >> 5);
			tile[tileRow * 8 + 2] = ((b1 & 0x20) >> 5) + ((b2 & 0x20) >> 4);
			tile[tileRow * 8 + 3] = ((b1 & 0x10) >> 4) + ((b2 & 0x10) >> 3);
			tile[tileRow * 8 + 4] = ((b1 & 0x08) >> 3) + ((b2 & 0x08) >> 2);
			tile[tileRow * 8 + 5] = ((b1 & 0x04) >> 2) + ((b2 & 0x04) >> 1);
			tile[tileRow * 8 + 6] = ((b1 & 0x02) >> 1) + ((b2 & 0x02));
			tile[tileRow * 8 + 7] = ((b1 & 0x01)) + ((b2 & 0x01) << 1);
		}

		private void OamTransfer()
		{
			var address = (ushort)(DMA << 8);
			for (var i = 0; i < 40 * 4; ++i) {
				_memory.Write((ushort)(0xFE00 + i), _memory.Read((ushort)(address + i)));
			}
		}
	}
}
