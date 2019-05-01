namespace Emulator.Audio
{
    public abstract class WaveGenerator
    {
        public const int ChannelLeft = 1;
        public const int ChannelRight = 2;
        public const int ChannelMono = 4;
        protected int totalLength;
        protected int cyclePosition;
        protected int cycleLength;
        protected int amplitude;
        protected int channel;
        protected int sampleRate;
        protected int initialEnvelope;
        protected bool increaseEnvelope;
        protected int numberStepsEnvelope;
        protected int counterEnvelope;

        public void SetSampleRate(int sampleRate)
        {
            this.sampleRate = sampleRate;
        }

        public void SetChannel(int channel)
        {
            this.channel = channel;
        }

        public virtual void SetLength(int gbLength)
        {
            if (gbLength == -1)
            {
                totalLength = -1;
            }
            else
            {
                totalLength = (64 - gbLength) / 4;
            }
        }
    }
}
