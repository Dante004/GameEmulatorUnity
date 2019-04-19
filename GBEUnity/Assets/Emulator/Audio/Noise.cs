
using System;

namespace Emulator.Audio
{
    public class Noise
    {
        // Indicates sound is to be played on the left channel of a stereo sound
        public const int ChannelLeft = 1;
        // Indictaes sound is to be played on the right channel of a stereo sound
        public const int ChannelRight = 2;
        // Indicates that sound is mono
        public const int ChannelMono = 4;
        // Indicates the length of the sound in frames
        private int _totalLength;
        private int _cyclePosition;
        // The length of one cycle, in samples
        private int _cycleLength;
        // Amplitude of the wave function
        private int _amplitude;
        // Channel being played on.  Combination of ChannelLeft and ChannelRight, or ChannelMono
        private int _channel;
        // Sampling rate of the output channel
        private int _sampleRate;
        // Initial value of the envelope
        private int _initialEnvelope;
        private int _numStepsEnvelope;
        // Whether the envelope is an increase/decrease in amplitude
        private bool _increaseEnvelope;
        private int counterEnvelope;
        // Stores the random values emulating the polynomial generator (badly!)
        private bool[] _randomValues;
        private int _dividingRatio;
        private int _polynomialSteps;
        private int _shiftClockFreq;
        private int _finalFreq;
        private int _cycleOffset;
        Random random = new Random();

        // Creates a white noise generator with the specified wavelength, amplitude, channel, and sample rate
        public Noise(int waveLength, int amplitude, int channel, int sampleRate)
        {
            _cycleLength = waveLength;
            _amplitude = amplitude;
            _cyclePosition = 0;
            _sampleRate = sampleRate;
            _cycleOffset = 0;

            _randomValues = new bool[32767];

            for (var i = 0; i < 32767; ++i)
            {
                _randomValues[i] = random.NextDouble() < 0.5;
            }
        }

        // Creates a white noise generator with the specified sample rate
        public Noise(int sampleRate)
        {
            _cyclePosition = 0;
            _channel = ChannelLeft | ChannelRight;
            _cycleLength = 2;
            _totalLength = 0;
            _sampleRate = sampleRate;
            _amplitude = 32;
            _cycleOffset = 0;

            _randomValues = new bool[32767];

            for (var i = 0; i < 32767; ++i)
            {
                _randomValues[i] = random.NextDouble() < 0.5;
            }
        }

        public void SetSampleRate(int sampleRate)
        {
            _sampleRate = sampleRate;
        }

        public void SetChannel(int channel)
        {
            _channel = channel;
        }

        public void SetEnvelope(int initialValue, int numSteps, bool increase)
        {
            _initialEnvelope = initialValue;
            _numStepsEnvelope = numSteps;
            _increaseEnvelope = increase;
            _amplitude = initialValue * 2;
        }

        public void SetLength(int gbLength)
        {
            if (gbLength == -1)
            {
                _totalLength = -1;
            }
            else
            {
                _totalLength = (64 - gbLength) / 4;
            }
        }

        public void SetParameters(float dividingRatio, bool polynomialSteps, int shiftClockFreq)
        {
            _dividingRatio = (int)dividingRatio;
            if (!polynomialSteps)
            {
                _polynomialSteps = 32767;
                _cycleLength = 32767 << 8;
                _cycleOffset = 0;
            }
            else
            {
                _polynomialSteps = 63;
                _cycleLength = 63 << 8;

                _cycleOffset = (int)(random.NextDouble() * 1000);
            }
            _shiftClockFreq = shiftClockFreq;

            if (dividingRatio == 0)
                dividingRatio = 0.5f;

            _finalFreq = ((int)(4194304 / 8 / dividingRatio)) >> (shiftClockFreq + 1);
        }

        public void Play(byte[] b, int numSamples, int numChannels)
        {
            int val;

            if (_totalLength != 0)
            {
                _totalLength--;

                counterEnvelope++;
                if (_numStepsEnvelope != 0)
                {
                    if (((counterEnvelope % _numStepsEnvelope) == 0) && (_amplitude > 0))
                    {
                        if (!_increaseEnvelope)
                        {
                            if (_amplitude > 0)
                                _amplitude -= 2;
                        }
                        else
                        {
                            if (_amplitude < 16)
                                _amplitude += 2;
                        }
                    }
                }


                var step = ((_finalFreq) / (_sampleRate >> 8));

                for (var r = 0; r < numSamples; ++r)
                {
                    var value = _randomValues[((_cycleOffset) + (_cyclePosition >> 8)) & 0x7FFF];
                    val = value ? (_amplitude / 2) : (-_amplitude / 2);

                    if ((_channel & ChannelLeft) != 0)
                        b[r * numChannels] += (byte)val;
                    if ((_channel & ChannelRight) != 0)
                        b[r * numChannels + 1] += (byte)val;
                    if ((_channel & ChannelMono) != 0)
                        b[r * numChannels] = b[r * numChannels + 1] += (byte)val;

                    _cyclePosition = (_cyclePosition + step) % _cycleLength;
                }
            }
        }
    }
}
