using System;

namespace Emulator.Audio
{
    internal class Noise : WaveGenerator
    {
        private readonly bool[] _randomValues;
        private int _dividingRatio;
        private int _polynomialSteps;
        private int _shiftClockFreq;
        private int _finalFreq;
        private int _cycleOffset;
        protected readonly Random random = new Random();

        public Noise(int waveLength, int amplitude, int channel, int sampleRate)
        {
            cycleLength = waveLength;
            base.amplitude = amplitude;
            cyclePosition = 0;
            base.sampleRate = sampleRate;
            _cycleOffset = 0;

            _randomValues = new bool[32767];

            for (var i = 0; i < 32767; ++i)
            {
                _randomValues[i] = random.NextDouble() < 0.5;
            }
        }

        public Noise(int sampleRate)
        {
            cyclePosition = 0;
            channel = ChannelLeft | ChannelRight;
            cycleLength = 2;
            totalLength = 0;
            sampleRate = sampleRate;
            amplitude = 32;
            _cycleOffset = 0;

            _randomValues = new bool[32767];

            for (var i = 0; i < 32767; ++i)
            {
                _randomValues[i] = random.NextDouble() < 0.5;
            }
        }

        public void SetEnvelope(int initialValue, int numSteps, bool increase)
        {
            initialEnvelope = initialValue;
            numberStepsEnvelope = numSteps;
            increaseEnvelope = increase;
            base.amplitude = initialValue * 2;
        }

        public override void SetLength(int gbLength)
        {
            base.SetLength(gbLength);
        }

        public void SetParameters(float dividingRatio, bool polynomialSteps, int shiftClockFreq)
        {
            _dividingRatio = (int)dividingRatio;
            if (!polynomialSteps)
            {
                _polynomialSteps = 32767;
                cycleLength = 32767 << 8;
                _cycleOffset = 0;
            }
            else
            {
                _polynomialSteps = 63;
                cycleLength = 63 << 8;

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

            if (totalLength == 0) return;
            totalLength--;

            counterEnvelope++;
            if (numberStepsEnvelope != 0)
            {
                if (((counterEnvelope % numberStepsEnvelope) == 0) && (amplitude > 0))
                {
                    if (!increaseEnvelope)
                    {
                        if (amplitude > 0)
                            amplitude -= 2;
                    }
                    else
                    {
                        if (amplitude < 16)
                            amplitude += 2;
                    }
                }
            }


            var step = ((_finalFreq) / (base.sampleRate >> 8));

            for (var r = 0; r < numSamples; ++r)
            {
                var value = _randomValues[((_cycleOffset) + (cyclePosition >> 8)) & 0x7FFF];
                val = value ? (base.amplitude / 2) : (-base.amplitude / 2);

                if ((channel & ChannelLeft) != 0)
                    b[r * numChannels] += (byte)val;
                if ((channel & ChannelRight) != 0)
                    b[r * numChannels + 1] += (byte)val;
                if ((channel & ChannelMono) != 0)
                    b[r * numChannels] = b[r * numChannels + 1] += (byte)val;

                cyclePosition = (cyclePosition + step) % cycleLength;
            }
        }
    }
}
