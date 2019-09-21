using Emulator.Cartridge;
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

        public byte ReadByte(ushort address)
        {
            return _fileData[0x7FFF & address];
        }

        public void WriteByte(ushort address, byte value)
        {
            Debug.LogError("This cartridge cannot write data");
        }
    }
}
