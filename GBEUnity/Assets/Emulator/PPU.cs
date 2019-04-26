namespace Emulator
{

    public enum LcdcModeType
    {
        HBlank = 0,
        VBlank = 1,
        SearchingOamRam = 2,
        TransferingData = 3
    }

    public class PPU
    {

        public readonly uint WHITE = 0xFFFFFFFF;
        public readonly uint LIGHT_GRAY = 0xFFAAAAAA;
        public readonly uint DARK_GRAY = 0xFF555555;
        public readonly uint BLACK = 0xFF000000;

        public bool lcdControlOperationEnabled;
        public bool windowTileMapDisplaySelect;
        public bool windowDisplayed;
        public bool backgroundAndWindowTileDataSelect;
        public bool backgroundTileMapDisplaySelect;
        public bool largeSprites;
        public bool spritesDisplayed;
        public bool backgroundDisplayed;
        public int scrollX, scrollY;
        public int windowX, windowY;
        public int lyCompare, ly;
        public uint[] backgroundPalette;
        public uint[] objectPalette0;
        public uint[] objectPalette1;
        public bool lcdcLycLyCoincidenceInterruptEnabled;
        public bool lcdcOamInterruptEnabled;
        public bool lcdcVBlankInterruptEnabled;
        public bool lcdcHBlankInterruptEnabled;
        public LcdcModeType lcdcMode;

        public uint[,] backgroundBuffer = new uint[256, 256];
        public bool[,] backgroundTileInvalidated = new bool[32, 32];
        public bool invalidateAllBackgroundTilesRequest;
        public uint[,,,] spriteTile = new uint[256, 8, 8, 2];
        public bool[] spriteTileInvalidated = new bool[256];
        public bool invalidateAllSpriteTilesRequest;
        public uint[,] windowBuffer = new uint[144, 168];

        private Memory memory;

        public PPU(Memory memory)
        {
            this.memory = memory;
            memory.ppu = this;
        backgroundPalette = new [] { WHITE, LIGHT_GRAY, DARK_GRAY, BLACK };
        objectPalette0 = new [] { WHITE, LIGHT_GRAY, DARK_GRAY, BLACK };
        objectPalette1 = new [] { WHITE, LIGHT_GRAY, DARK_GRAY, BLACK };
    }

        public void UpdateSpriteTiles()
        {

            for (int i = 0; i < 256; i++)
            {
                if (spriteTileInvalidated[i] || invalidateAllSpriteTilesRequest)
                {
                    spriteTileInvalidated[i] = false;
                    int address = i << 4;
                    for (int y = 0; y < 8; y++)
                    {
                        int lowByte = memory.videoRam[address++];
                        int highByte = memory.videoRam[address++] << 1;
                        for (int x = 7; x >= 0; x--)
                        {
                            int paletteIndex = (0x02 & highByte) | (0x01 & lowByte);
                            lowByte >>= 1;
                            highByte >>= 1;
                            if (paletteIndex > 0)
                            {
                                spriteTile[i, y, x, 0] = objectPalette0[paletteIndex];
                                spriteTile[i, y, x, 1] = objectPalette1[paletteIndex];
                            }
                            else
                            {
                                spriteTile[i, y, x, 0] = 0;
                                spriteTile[i, y, x, 1] = 0;
                            }
                        }
                    }
                }
            }

            invalidateAllSpriteTilesRequest = false;
        }

        public void UpdateWindow()
        {

            int tileMapAddress = windowTileMapDisplaySelect ? 0x1C00 : 0x1800;

            if (backgroundAndWindowTileDataSelect)
            {
                for (int i = 0; i < 18; i++)
                {
                    for (int j = 0; j < 21; j++)
                    {
                        if (backgroundTileInvalidated[i, j] || invalidateAllBackgroundTilesRequest)
                        {
                            int tileDataAddress = memory.videoRam[tileMapAddress + ((i << 5) | j)] << 4;
                            int y = i << 3;
                            int x = j << 3;
                            for (int k = 0; k < 8; k++)
                            {
                                int lowByte = memory.videoRam[tileDataAddress++];
                                int highByte = memory.videoRam[tileDataAddress++] << 1;
                                for (int b = 7; b >= 0; b--)
                                {
                                    windowBuffer[y + k, x + b] = backgroundPalette[(0x02 & highByte) | (0x01 & lowByte)];
                                    lowByte >>= 1;
                                    highByte >>= 1;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 18; i++)
                {
                    for (int j = 0; j < 21; j++)
                    {
                        if (backgroundTileInvalidated[i, j] || invalidateAllBackgroundTilesRequest)
                        {
                            int tileDataAddress = memory.videoRam[tileMapAddress + ((i << 5) | j)];
                            if (tileDataAddress > 127)
                            {
                                tileDataAddress -= 256;
                            }
                            tileDataAddress = 0x1000 + (tileDataAddress << 4);
                            int y = i << 3;
                            int x = j << 3;
                            for (int k = 0; k < 8; k++)
                            {
                                int lowByte = memory.videoRam[tileDataAddress++];
                                int highByte = memory.videoRam[tileDataAddress++] << 1;
                                for (int b = 7; b >= 0; b--)
                                {
                                    windowBuffer[y + k, x + b] = backgroundPalette[(0x02 & highByte) | (0x01 & lowByte)];
                                    lowByte >>= 1;
                                    highByte >>= 1;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void UpdateBackground()
        {

            int tileMapAddress = backgroundTileMapDisplaySelect ? 0x1C00 : 0x1800;

            if (backgroundAndWindowTileDataSelect)
            {
                for (int i = 0; i < 32; i++)
                {
                    for (int j = 0; j < 32; j++, tileMapAddress++)
                    {
                        if (backgroundTileInvalidated[i, j] || invalidateAllBackgroundTilesRequest)
                        {
                            backgroundTileInvalidated[i, j] = false;
                            int tileDataAddress = memory.videoRam[tileMapAddress] << 4;
                            int y = i << 3;
                            int x = j << 3;
                            for (int k = 0; k < 8; k++)
                            {
                                int lowByte = memory.videoRam[tileDataAddress++];
                                int highByte = memory.videoRam[tileDataAddress++] << 1;
                                for (int b = 7; b >= 0; b--)
                                {
                                    backgroundBuffer[y + k, x + b] = backgroundPalette[(0x02 & highByte) | (0x01 & lowByte)];
                                    lowByte >>= 1;
                                    highByte >>= 1;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 32; i++)
                {
                    for (int j = 0; j < 32; j++, tileMapAddress++)
                    {
                        if (backgroundTileInvalidated[i, j] || invalidateAllBackgroundTilesRequest)
                        {
                            backgroundTileInvalidated[i, j] = false;
                            int tileDataAddress = memory.videoRam[tileMapAddress];
                            if (tileDataAddress > 127)
                            {
                                tileDataAddress -= 256;
                            }
                            tileDataAddress = 0x1000 + (tileDataAddress << 4);
                            int y = i << 3;
                            int x = j << 3;
                            for (int k = 0; k < 8; k++)
                            {
                                int lowByte = memory.videoRam[tileDataAddress++];
                                int highByte = memory.videoRam[tileDataAddress++] << 1;
                                for (int b = 7; b >= 0; b--)
                                {
                                    backgroundBuffer[y + k, x + b] = backgroundPalette[(0x02 & highByte) | (0x01 & lowByte)];
                                    lowByte >>= 1;
                                    highByte >>= 1;
                                }
                            }
                        }
                    }
                }
            }

            invalidateAllBackgroundTilesRequest = false;
        }
    }
}
