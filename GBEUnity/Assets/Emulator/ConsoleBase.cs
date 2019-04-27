namespace Emulator
{
	public abstract class ConsoleBase
	{
        public enum Button
        {
            Up,
            Down,
            Left,
            Right,
            A,
            B,
            Start,
            Select
        }

        public IVideoOutput Video
		{
			get;
		}

        public IAudioOutput Audio
        {
            get;
        }

		protected ConsoleBase(IVideoOutput video,IAudioOutput audio = null)
		{
			Video = video;
            Audio = audio;
        }

		public abstract void LoadRom(string name);

		public abstract void RunNextStep();

		public abstract void SetInput(Button button, bool pressed);
	}
}
