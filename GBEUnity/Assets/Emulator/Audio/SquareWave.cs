
namespace Emulator.Audio
{
    class SquareWave
    {
        // Sound is to be played on the left _channel of a stereo sound
        public const int ChannelLeft = 1;

        // Sound is to be played on the right _channel of a stereo sound
        public const int ChannelRight = 2;

        // Sound is to be played back in mono
        public const int ChannelMono = 4;

        // Length of the sound (in frames)
        private int _totalLength;

        // Current position in the waveform (in samples)
        private int _cyclePosition;

        // Length of the waveform (in samples)
        private int _cycleLength;

        // Amplitude of the waveform
        private int _amplitude;

        // Amount of time the sample stays high in a single waveform (in eighths)
        private int _dutyCycle;

        // The _channel that the sound is to be played back on
        private int _channel;

        // Sample rate of the sound buffer
        private int _sampleRate;

        // Initial _amplitude
        private int _initialEnvelope;

        // Number of envelope steps
        private int _numStepsEnvelope;

        // If true, envelope will increase _amplitude of sound, false indicates decrease
        private bool _increaseEnvelope;

        // Current position in the envelope
        private int _counterEnvelope;

        // Frequency of the sound in internal GB format
        private int _gbFrequency;

        // Amount of time between sweep steps.
        private int _timeSweep;

        // Number of sweep steps
        private int _numSweep;

        // If true, sweep will decrease the sound frequency, otherwise, it will increase
        private bool _decreaseSweep;

        // Current position in the sweep
        private int _counterSweep;

        // Create a square wave generator with the supplied parameters
        public SquareWave(int waveLength, int amplitude, int duty, int channel, int rate)
        {
            _cycleLength = waveLength;
            _amplitude = amplitude;
            _cyclePosition = 0;
            _dutyCycle = duty;
            _channel = channel;
            _sampleRate = rate;
        }

        // Create a square wave generator at the specified sample rate
        public SquareWave(int rate)
        {
            _dutyCycle = 4;
            _cyclePosition = 0;
            _channel = ChannelLeft | ChannelRight;
            _cycleLength = 2;
            _totalLength = 0;
            _sampleRate = rate;
            _amplitude = 32;
            _counterSweep = 0;
        }

        // Set the sound buffer sample rate
        public void SetSampleRate(int sampleRate)
        {
            _sampleRate = sampleRate;
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

                this._gbFrequency = gbFrequency;
                if (frequency != 0)
                {
                    _cycleLength = (256 * _sampleRate) / (int)frequency;
                }
                else
                {
                    _cycleLength = 65535;
                }
                if (_cycleLength == 0)
                    _cycleLength = 1;
            }
            catch
            {
                // Skip ip
            }
        }

        // Set the _channel for playback
        public void SetChannel(int channel)
        {
            _channel = channel;
        }

        // Set the envelope parameters
        public void SetEnvelope(int initialValue, int numSteps, bool increase)
        {
            _initialEnvelope = initialValue;
            _numStepsEnvelope = numSteps;
            _increaseEnvelope = increase;
            _amplitude = initialValue * 2;
        }

        // Set the frequency sweep parameters
        public void SetSweep(int time, int num, bool decrease)
        {
            _timeSweep = (time + 1) / 2;
            _numSweep = num;
            _decreaseSweep = decrease;
            _counterSweep = 0;
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

        public void SetLength3(int gbLength)
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

        public void SetVolume3(int volume)
        {
            switch (volume)
            {
                case 0:
                    _amplitude = 0;
                    break;
                case 1:
                    _amplitude = 32;
                    break;
                case 2:
                    _amplitude = 16;
                    break;
                case 3:
                    _amplitude = 8;
                    break;
            }
        }

        // Output a frame of sound data into the buffer using the supplied frame length and array offset.
        public void Play(byte[] b, int numSamples, int numChannels)
        {
            int val = 0;

            if (_totalLength != 0)
            {
                _totalLength--;

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

                _counterEnvelope++;
                if (_numStepsEnvelope != 0)
                {
                    if (((_counterEnvelope % _numStepsEnvelope) == 0) && (_amplitude > 0))
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

                for (var r = 0; r < numSamples; r++)
                {
                    if (_cycleLength != 0)
                    {
                        if (((8 * _cyclePosition) / _cycleLength) >= _dutyCycle)
                        {
                            val = _amplitude;
                        }
                        else
                        {
                            val = -_amplitude;
                        }
                    }

                    if ((_channel & ChannelLeft) != 0)
                        b[r * numChannels] += (byte)val;
                    if ((_channel & ChannelRight) != 0)
                        b[r * numChannels + 1] += (byte)val;
                    if ((_channel & ChannelMono) != 0)
                        b[r * numChannels] = b[r * numChannels + 1] = (byte)val;

                    _cyclePosition = (_cyclePosition + 256) % _cycleLength;
                }
            }
        }
    }
}
