using UnityEngine;
using System.Collections;

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
			Select}
		;

		public IVideoOutput Video
		{
			get;
			private set;
		}

        public IAudioOutput Audio
        {
            get;
            private set;
        }

		public ConsoleBase(IVideoOutput video,IAudioOutput audio = null)
		{
			Video = video;
            Audio = audio;
        }

		public abstract void LoadRom(string name);

		public abstract void RunNextStep();

		public abstract void SetInput(Button button, bool pressed);
	}
}
