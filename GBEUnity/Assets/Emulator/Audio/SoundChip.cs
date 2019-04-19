
namespace Emulator.Audio
{
    public class SoundChip
    {
        internal SquareWave channel1;
        internal SquareWave channel2;
        internal VoluntaryWave channel3;
        internal Noise channel4;
        internal bool soundEnabled = true;

        /** If true, channel is enabled */
        internal bool channel1Enable = true, channel2Enable = true,
            channel3Enable = true, channel4Enable = true;

        /** Current sampling rate that sound is output at */
        private int _sampleRate = 44100;

        /** Initialize sound emulation, and allocate sound hardware */
        public SoundChip()
        {
            channel1 = new SquareWave(_sampleRate);
            channel2 = new SquareWave(_sampleRate);
            channel3 = new VoluntaryWave(_sampleRate);
            channel4 = new Noise(_sampleRate);
        }

        /** Change the sample rate of the playback */
        public void SetSampleRate(int sampleRate)
        {
            _sampleRate = sampleRate;

            channel1.SetSampleRate(sampleRate);
            channel2.SetSampleRate(sampleRate);
            channel3.SetSampleRate(sampleRate);
            channel4.SetSampleRate(sampleRate);
        }

        /** Adds a single frame of sound data to the buffer */
        public void OutputSound(IAudioOutput audioOutput)
        {
            if (soundEnabled)
                return;

            int numChannels = 2; // Always stereo for Game Boy
            int numSamples = audioOutput.GetSamplesAvailable();

            byte[] b = new byte[numChannels * numSamples];

            if (channel1Enable)
                channel1.Play(b, numSamples, numChannels);
            if (channel2Enable)
                channel2.Play(b, numSamples, numChannels);
            if (channel3Enable)
                channel3.Play(b, numSamples, numChannels);
            if (channel4Enable)
                channel4.Play(b, numSamples, numChannels);

            audioOutput.Play(b);
        }
    }
}
