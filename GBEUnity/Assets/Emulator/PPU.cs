using System.Collections.Generic;

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
        public int colorIndex=0;

        public readonly uint[] white = new [] { 0xFFFFFFFF }; 
        public readonly uint[] lightGray = new[] { 0xFFAAAAAA };
        public readonly uint[] darkGray = new[] { 0xFF555555 };
        public readonly uint[] black = new[] { 0xFF000000 };

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

        private readonly Memory _memory;

        public PPU(Memory memory)
        {
            _memory = memory;
            memory.ppu = this;
            backgroundPalette = new [] { white[colorIndex], lightGray[colorIndex], darkGray[colorIndex], black[colorIndex] };
            objectPalette0 = new [] { white[colorIndex], lightGray[colorIndex], darkGray[colorIndex], black[colorIndex] };
            objectPalette1 = new [] { white[colorIndex], lightGray[colorIndex], darkGray[colorIndex], black[colorIndex] };
        }

        public void UpdateSpriteTiles()
        {

            for (var i = 0; i < 256; ++i)
            {
                if (!spriteTileInvalidated[i] && !invalidateAllSpriteTilesRequest) continue;
                spriteTileInvalidated[i] = false;
                var address = i << 4;
                for (var y = 0; y < 8; ++y)
                {
                    var lowByte = _memory.videoRam[address++];
                    var highByte = _memory.videoRam[address++] << 1;
                    for (var x = 7; x >= 0; --x)
                    {
                        var paletteIndex = (0x02 & highByte) | (0x01 & lowByte);
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

            invalidateAllSpriteTilesRequest = false;
        }

        public void UpdateWindow()
        {

            var tileMapAddress = windowTileMapDisplaySelect ? 0x1C00 : 0x1800;

            if (backgroundAndWindowTileDataSelect)
            {
                for (var i = 0; i < 18; ++i)
                {
                    for (var j = 0; j < 21; ++j)
                    {
                        if (!backgroundTileInvalidated[i, j] && !invalidateAllBackgroundTilesRequest) continue;
                        var tileDataAddress = _memory.videoRam[tileMapAddress + ((i << 5) | j)] << 4;
                        var y = i << 3;
                        var x = j << 3;
                        for (var k = 0; k < 8; ++k)
                        {
                            var lowByte = _memory.videoRam[tileDataAddress++];
                            var highByte = _memory.videoRam[tileDataAddress++] << 1;
                            for (var b = 7; b >= 0; --b)
                            {
                                windowBuffer[y + k, x + b] = backgroundPalette[(0x02 & highByte) | (0x01 & lowByte)];
                                lowByte >>= 1;
                                highByte >>= 1;
                            }
                        }
                    }
                }
            }
            else
            {
                for (var i = 0; i < 18; ++i)
                {
                    for (var j = 0; j < 21; ++j)
                    {
                        if (!backgroundTileInvalidated[i, j] && !invalidateAllBackgroundTilesRequest) continue;
                        int tileDataAddress = _memory.videoRam[tileMapAddress + ((i << 5) | j)];
                        if (tileDataAddress > 127)
                        {
                            tileDataAddress -= 256;
                        }
                        tileDataAddress = 0x1000 + (tileDataAddress << 4);
                        var y = i << 3;
                        var x = j << 3;
                        for (var k = 0; k < 8; ++k)
                        {
                            var lowByte = _memory.videoRam[tileDataAddress++];
                            var highByte = _memory.videoRam[tileDataAddress++] << 1;
                            for (var b = 7; b >= 0; --b)
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

        public void UpdateBackground()
        {

            var tileMapAddress = backgroundTileMapDisplaySelect ? 0x1C00 : 0x1800;

            if (backgroundAndWindowTileDataSelect)
            {
                for (var i = 0; i < 32; ++i)
                {
                    for (var j = 0; j < 32; ++j, ++tileMapAddress)
                    {
                        if (!backgroundTileInvalidated[i, j] && !invalidateAllBackgroundTilesRequest) continue;
                        backgroundTileInvalidated[i, j] = false;
                        var tileDataAddress = _memory.videoRam[tileMapAddress] << 4;
                        var y = i << 3;
                        var x = j << 3;
                        for (var k = 0; k < 8; ++k)
                        {
                            var lowByte = _memory.videoRam[tileDataAddress++];
                            var highByte = _memory.videoRam[tileDataAddress++] << 1;
                            for (var b = 7; b >= 0; --b)
                            {
                                backgroundBuffer[y + k, x + b] = backgroundPalette[(0x02 & highByte) | (0x01 & lowByte)];
                                lowByte >>= 1;
                                highByte >>= 1;
                            }
                        }
                    }
                }
            }
            else
            {
                for (var i = 0; i < 32; ++i)
                {
                    for (var j = 0; j < 32; ++j, ++tileMapAddress)
                    {
                        if (!backgroundTileInvalidated[i, j] && !invalidateAllBackgroundTilesRequest) continue;
                        backgroundTileInvalidated[i, j] = false;
                        int tileDataAddress = _memory.videoRam[tileMapAddress];
                        if (tileDataAddress > 127)
                        {
                            tileDataAddress -= 256;
                        }
                        tileDataAddress = 0x1000 + (tileDataAddress << 4);
                        var y = i << 3;
                        var x = j << 3;
                        for (var k = 0; k < 8; ++k)
                        {
                            var lowByte = _memory.videoRam[tileDataAddress++];
                            var highByte = _memory.videoRam[tileDataAddress++] << 1;
                            for (var b = 7; b >= 0; --b)
                            {
                                backgroundBuffer[y + k, x + b] = backgroundPalette[(0x02 & highByte) | (0x01 & lowByte)];
                                lowByte >>= 1;
                                highByte >>= 1;
                            }
                        }
                    }
                }
            }

            invalidateAllBackgroundTilesRequest = false;
        }
    }
}
