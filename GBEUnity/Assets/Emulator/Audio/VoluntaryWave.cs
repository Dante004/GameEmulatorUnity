namespace Emulator.Audio
{
    internal class VoluntaryWave : WaveGenerator
    { 
        private int _volumeShift;
        private readonly int[] _waveForm = new int[32];

        public VoluntaryWave(int waveLength, int amplitude, int duty, int chan, int rate)
        {
            cycleLength = waveLength;
            base.amplitude = amplitude;
            cyclePosition = 0;
            channel = chan;
            sampleRate = rate;
        }

        public VoluntaryWave(int rate)
        {
            cyclePosition = 0;
            channel = ChannelLeft | ChannelRight;
            cycleLength = 2;
            totalLength = 0;
            sampleRate = rate;
            amplitude = 32;
        }

        public void SetFrequency(int gbFrequency)
        {
            float frequency = 65536f / (float)(2048 - gbFrequency);
            cycleLength = (int)((float)(256f * sampleRate) / frequency);
            if (cycleLength == 0)
                cycleLength = 1;
        }

        public override void SetLength(int gbLength)
        {
            if (gbLength == -1)
            {
                totalLength = -1;
            }
            else
            {
                totalLength = (256 - gbLength) / 4;
            }
        }

        public void SetSamplePair(int address, int value)
        {
            _waveForm[address * 2] = (value & 0xF0) >> 4;
            _waveForm[address * 2 + 1] = (value & 0x0F);
        }

        public void SetVolume(int volume)
        {
            switch (volume)
            {
                case 0:
                    _volumeShift = 5;
                    break;
                case 1:
                    _volumeShift = 0;
                    break;
                case 2:
                    _volumeShift = 1;
                    break;
                case 3:
                    _volumeShift = 2;
                    break;
            }
        }

        public void Play(byte[] b, int numSamples, int numChannels)
        {
            int val;

            if (totalLength <= 0) return;
            totalLength--;

            for (var r = 0; r < numSamples; ++r)
            {
                var samplePos = (31 * cyclePosition) / cycleLength;
                val = _waveForm[samplePos % 32] >> _volumeShift << 1;

                if ((channel & ChannelLeft) != 0)
                    b[r * numChannels] += (byte)val;
                if ((channel & ChannelRight) != 0)
                    b[r * numChannels + 1] += (byte)val;
                //if ((_channel & ChannelMono) != 0)
                //b [r * numChannels] = b [r * numChannels + 1] += (byte)val;

                cyclePosition = (cyclePosition + 256) % cycleLength;
            }
        }
    }
}
