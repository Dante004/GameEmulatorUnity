namespace Emulator
{
    public interface IAudioOutput
    {
        int GetOutputSampleRate();

        int GetSamplesAvailable();

        void Play(byte[] data);
    }
}
