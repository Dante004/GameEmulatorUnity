using Emulator.Memory;

namespace Emulator.GPU
{
    public enum LcdcModeType
    {
        HBlank = 0,
        VBlank = 1,
        SearchingOamRam = 2,
        TransferingData = 3
    }

    public class GameBoyGPU
    {
        public const uint White = 0xFFFFFFFF;
        public const uint LightGray = 0xFFAAAAAA;
        public const uint DarkGray = 0xFF555555;
        public const uint Black = 0xFF000000;
        public static uint[] BackgroundPalette = { White, LightGray, DarkGray, Black };
        public static uint[] ObjectPallete0 = { White, LightGray, DarkGray, Black };
        public static uint[] ObjectPallete1 = { White, LightGray, DarkGray, Black };
        public static bool BackgroundAndWindowTileDataSelect;
        public static bool BackgroundTileMapDisplaySelect;
        public uint[,] BackgroundBuffer = new uint[256, 256]; //buffor tła
        public static bool[,] BackgroundTileInvalidated = new bool[32, 32]; // tablica dla tła
        public static bool InvalidateAllBackgroundTilesRequest;
        public uint[,,,] SpriteTile = new uint[256, 8, 8, 2]; //tablica dla tilsetów
        public static bool[] SpriteTileInvalidated = new bool[256];
        public static bool InvalidateAllSpriteTilesRequest;
        public uint[,] WindowBuffer = new uint[144, 168]; //buffor ekranu
        public static bool lcdcLycLyCoincidenceInterruptEnabled;
        public static bool lcdcOamInterruptEnabled;
        public static bool lcdcVBlankInterruptEnabled;
        public static bool lcdcHBlankInterruptEnabled;
        public static bool WindowTileMapDisplaySelect;
        public static bool windowDisplayed;
        public static bool largeSprites;
        public static bool spritesDisplayed;
        public static bool backgroundDisplayed;
        public static byte scrollX, scrollY; //rejestry przesuwanie tła
        public static byte windowX, windowY;
        public static byte lyCompare, ly;
        public static LcdcModeType LcdcMode; //tryb wyświetlania obrazu

        private  readonly GameBoyMemory _memory;

        public GameBoyGPU(GameBoyMemory memory)
        {
            this._memory = memory;
        }

        //aktualizacja tilestów
        public void UpdateSpriteTiles()
        {
            for (int i = 0; i < 256; i++) //może być maksymalnie 256 tilesetów ustawionych na mapie
            {
                if (SpriteTileInvalidated[i] || InvalidateAllSpriteTilesRequest)
                {
                    SpriteTileInvalidated[i] = false;
                    int address = i << 4;
                    for (int y = 0; y < 8; y++) //maksymalny rozmiar tilesetu 8x8
                    {
                        int lowByte = _memory.VideoRam[address++];
                        int highByte = _memory.VideoRam[address++] << 1;
                        for (int x = 7; x >= 0; x--)
                        {
                            int palletteIndex = (0x02 & highByte) | (0x01 & lowByte); //sprawdzenie z pamięci jaki kolor powinien mieć sprite
                            lowByte >>= 1;
                            highByte >>= 1;
                            if (palletteIndex > 0)
                            {
                                //nadawanie spraitom danego koloru
                                SpriteTile[i, y, x, 0] = ObjectPallete0[palletteIndex];
                                SpriteTile[i, y, x, 1] = ObjectPallete1[palletteIndex];
                            }
                            else
                            {
                                //nadawanie spraitom przezroczystości
                                SpriteTile[i, y, x, 0] = 0;
                                SpriteTile[i, y, x, 1] = 1;
                            }
                        }
                    }
                }
            }
            InvalidateAllSpriteTilesRequest = false;
        }

        //aktualizacja tła
        public void UpdateBackground()
        {
            int tileMapAddress = BackgroundTileMapDisplaySelect ? 0x1C00 : 0x1800;

            if (BackgroundTileMapDisplaySelect)
            {
                for (int i = 0; i < 32; i++) //tilsety tła wynosza 32x32
                {
                    for (int j = 0; j < 32; j++, tileMapAddress++)
                    {
                        if (BackgroundTileInvalidated[i, j] || InvalidateAllBackgroundTilesRequest)
                        {
                            BackgroundTileInvalidated[i, j] = false;
                            int tileDataAddress = _memory.VideoRam[tileMapAddress] << 4; //pobieranie tilsetów z pamięci 
                            int y = i << 3;
                            int x = j << 3;
                            for (int k = 0; k < 8; k++)
                            {
                                int lowByte = _memory.VideoRam[tileDataAddress++];
                                int highByte = _memory.VideoRam[tileDataAddress++] << 1;
                                for (int b = 7; b >= 0; b--)
                                {
                                    BackgroundBuffer[y + k, x + b] = BackgroundPalette[(0x02 & highByte) | (0x01 & lowByte)];
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
                        if (BackgroundTileInvalidated[i, j] || InvalidateAllBackgroundTilesRequest)
                        {
                            BackgroundTileInvalidated[i, j] = false;
                            int tileDataAddress = _memory.VideoRam[tileMapAddress];
                            if (tileDataAddress > 127)
                            {
                                tileDataAddress -= 256;
                            }
                            tileDataAddress = 0x1000 + (tileDataAddress << 4);
                            int y = i << 3;
                            int x = j << 3;
                            for (int k = 0; k < 8; k++)
                            {
                                int lowByte = _memory.VideoRam[tileDataAddress++];
                                int highByte = _memory.VideoRam[tileDataAddress++] << 1;
                                for (int b = 7; b >= 0; b--)
                                {
                                    BackgroundBuffer[y + k, x + b] = BackgroundPalette[(0x02 & highByte) | (0x01 & lowByte)];
                                    lowByte >>= 1;
                                    highByte >>= 1;
                                }
                            }
                        }
                    }
                }
            }
            InvalidateAllBackgroundTilesRequest = false;
        }

        //aktualizacja HUD
        public void UpdateWindow()
        {
            int tileMapAddress = WindowTileMapDisplaySelect ? 0x1C00 : 0x1800;

            if (BackgroundAndWindowTileDataSelect)
            {
                for (int i = 0; i < 18; i++)
                {
                    for (int j = 0; j < 21; j++)
                    {
                        if (BackgroundTileInvalidated[i, j] || InvalidateAllBackgroundTilesRequest)
                        {
                            int tileDataAddress = _memory.VideoRam[tileMapAddress + ((i << 5) | j)] << 4;
                            int y = i << 3;
                            int x = j << 3;
                            for (int k = 0; k < 8; k++)
                            {
                                int lowByte = _memory.VideoRam[tileDataAddress++];
                                int highByte = _memory.VideoRam[tileDataAddress++] << 1;
                                for (int b = 7; b >= 0; b--)
                                {
                                    WindowBuffer[y + k, x + b] = BackgroundPalette[(0x02 & highByte) | (0x01 & lowByte)];
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
                        if (BackgroundTileInvalidated[i, j] || InvalidateAllBackgroundTilesRequest)
                        {
                            int tileDataAddress = _memory.VideoRam[tileMapAddress + ((i << 5) | j)];
                            if (tileDataAddress > 127)
                            {
                                tileDataAddress -= 256;
                            }
                            tileDataAddress = 0x1000 + (tileDataAddress << 4);
                            int y = i << 3;
                            int x = j << 3;
                            for (int k = 0; k < 8; k++)
                            {
                                int lowByte = _memory.VideoRam[tileDataAddress++];
                                int highByte = _memory.VideoRam[tileDataAddress++] << 1;
                                for (int b = 7; b >= 0; b--)
                                {
                                    WindowBuffer[y + k, x + b] = BackgroundPalette[(0x02 & highByte) | (0x01 & lowByte)];
                                    lowByte >>= 1;
                                    highByte >>= 1;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
