using UnityEngine;

namespace Emulator.Cartridges
{
    public class RomOnly: ICartridge
    {
        private readonly byte[] _fileData;

        public RomOnly(byte[] fileData)
        {
            _fileData = fileData;
        }

        public int ReadByte(int address)
        {
            return _fileData[0x7FFF & address];
        }

        public void WriteByte(int address, int value)
        {
            Debug.LogError("This cartridge cannot write data");
        }
    }
}
