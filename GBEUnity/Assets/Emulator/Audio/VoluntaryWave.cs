
namespace Emulator.Audio
{
    class VoluntaryWave
    {
        public const int ChannelLeft = 1;
        public const int ChannelRight = 2;
        public const int ChannelMono = 4;

        private int _totalLength;
        private int _cyclePos;
        private int _cycleLength;
        private int _amplitude;
        private int _channel;
        private int _sampleRate;
        private int _volumeShift;
        private readonly int[] _waveForm = new int[32];

        public VoluntaryWave(int waveLength, int amplitude, int duty, int chan, int rate)
        {
            _cycleLength = waveLength;
            _amplitude = amplitude;
            _cyclePos = 0;
            _channel = chan;
            _sampleRate = rate;
        }

        public VoluntaryWave(int rate)
        {
            _cyclePos = 0;
            _channel = ChannelLeft | ChannelRight;
            _cycleLength = 2;
            _totalLength = 0;
            _sampleRate = rate;
            _amplitude = 32;
        }

        public void SetSampleRate(int sampleRate)
        {
            _sampleRate = sampleRate;
        }

        public void SetFrequency(int gbFrequency)
        {
            float frequency = 65536f / (float)(2048 - gbFrequency);
            _cycleLength = (int)((float)(256f * _sampleRate) / frequency);
            if (_cycleLength == 0)
                _cycleLength = 1;
        }

        public void SetChannel(int channel)
        {
            _channel = channel;
        }

        public void SetLength(int gbLength)
        {
            if (gbLength == -1)
            {
                _totalLength = -1;
            }
            else
            {
                _totalLength = (256 - gbLength) / 4;
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

            if (_totalLength > 0)
            {
                _totalLength--;

                for (int r = 0; r < numSamples; r++)
                {
                    int samplePos = (31 * _cyclePos) / _cycleLength;
                    val = _waveForm[samplePos % 32] >> _volumeShift << 1;

                    if ((_channel & ChannelLeft) != 0)
                        b[r * numChannels] += (byte)val;
                    if ((_channel & ChannelRight) != 0)
                        b[r * numChannels + 1] += (byte)val;
                    //if ((_channel & ChannelMono) != 0)
                    //b [r * numChannels] = b [r * numChannels + 1] += (byte)val;

                    _cyclePos = (_cyclePos + 256) % _cycleLength;
                }
            }
        }
    }
}
