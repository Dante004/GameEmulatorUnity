namespace Emulator.Cartridge
{
    public interface ICartridge
    {
        byte ReadByte(ushort address);
        void WriteByte(ushort address, byte value);
    }
}
