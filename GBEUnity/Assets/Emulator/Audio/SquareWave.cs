namespace Emulator.Audio
{
    internal class SquareWave : WaveGenerator
    {
        private int _dutyCycle;
        private int _gbFrequency;
        private int _timeSweep;
        private int _numSweep;
        private bool _decreaseSweep;
        private int _counterSweep;



        public SquareWave(int waveLength, int amplitude, int duty, int channel, int rate)
        {
            cycleLength = waveLength;
            base.amplitude = amplitude;
            cyclePosition = 0;
            _dutyCycle = duty;
            base.channel = channel;
            sampleRate = rate;
        }

        // Create a square wave generator at the specified sample rate
        public SquareWave(int rate)
        {
            _dutyCycle = 4;
            cyclePosition = 0;
            channel = ChannelLeft | ChannelRight;
            cycleLength = 2;
            totalLength = 0;
            sampleRate = rate;
            amplitude = 32;
            _counterSweep = 0;
        }

        // Set the duty cycle
        public void SetDutyCycle(int duty)
        {
            switch (duty)
            {
                case 0:
                    _dutyCycle = 1;
                    break;
                case 1:
                    _dutyCycle = 2;
                    break;
                case 2:
                    _dutyCycle = 4;
                    break;
                case 3:
                    _dutyCycle = 6;
                    break;
            }
        }

        // Set the sound frequency, in internal GB format */
        public void SetFrequency(int gbFrequency)
        {
            try
            {
                var frequency = 131072 / 2048f;

                if (gbFrequency != 2048)
                {
                    frequency = (131072f / (2048 - gbFrequency));
                }

                _gbFrequency = gbFrequency;
                if (frequency != 0)
                {
                    cycleLength = (256 * sampleRate) / (int)frequency;
                }
                else
                {
                    cycleLength = 65535;
                }
                if (cycleLength == 0)
                    cycleLength = 1;
            }
            catch
            {
                // Skip ip
            }
        }

        // Set the envelope parameters
        public void SetEnvelope(int initialValue, int numSteps, bool increase)
        {
            initialEnvelope = initialValue;
            numberStepsEnvelope = numSteps;
            increaseEnvelope = increase;
            amplitude = initialValue * 2;
        }

        // Set the frequency sweep parameters
        public void SetSweep(int time, int num, bool decrease)
        {
            _timeSweep = (time + 1) / 2;
            _numSweep = num;
            _decreaseSweep = decrease;
            _counterSweep = 0;
        }

        public override void SetLength(int gbLength)
        {
            base.SetLength(gbLength);
        }

        public void SetLength3(int gbLength)
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

        public void SetVolume3(int volume)
        {
            switch (volume)
            {
                case 0:
                    amplitude = 0;
                    break;
                case 1:
                    amplitude = 32;
                    break;
                case 2:
                    amplitude = 16;
                    break;
                case 3:
                    amplitude = 8;
                    break;
            }
        }

        // Output a frame of sound data into the buffer using the supplied frame length and array offset.
        public void Play(byte[] b, int numSamples, int numChannels)
        {
            int val = 0;

            if (totalLength == 0) return;
            totalLength--;

            if (_timeSweep != 0)
            {
                _counterSweep++;
                if (_counterSweep > _timeSweep)
                {
                    if (_decreaseSweep)
                    {
                        SetFrequency(_gbFrequency - (_gbFrequency >> _numSweep));
                    }
                    else
                    {
                        SetFrequency(_gbFrequency + (_gbFrequency >> _numSweep));
                    }
                    _counterSweep = 0;
                }
            }

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

            for (var r = 0; r < numSamples; r++)
            {
                if (cycleLength != 0)
                {
                    if (((8 * cyclePosition) / cycleLength) >= _dutyCycle)
                    {
                        val = amplitude;
                    }
                    else
                    {
                        val = -amplitude;
                    }
                }

                if ((channel & ChannelLeft) != 0)
                    b[r * numChannels] += (byte)val;
                if ((channel & ChannelRight) != 0)
                    b[r * numChannels + 1] += (byte)val;
                if ((channel & ChannelMono) != 0)
                    b[r * numChannels] = b[r * numChannels + 1] = (byte)val;

                cyclePosition = (cyclePosition + 256) % cycleLength;
            }
        }
    }
}
