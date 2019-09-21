using Emulator.Memories;

namespace Emulator.Timers {
	public class Timer
    {
        public byte DIV
        {
			get => _memory.Read(0xFF04);
            set => _memory.Write(0xFF04, value, true);
        }

		//Counter
		public byte TIMA
        {
			get => _memory.Read(0xFF05);
            set => _memory.Write(0xFF05, value);
        }

		//Modulo
		public byte TMA {
			get => _memory.Read(0xFF06);
            set { _memory.Write(0xFF06, value); }
		}

		//Control
		public byte TAC {
			get => _memory.Read(0xFF07);
            set => _memory.Write(0xFF07, value);
        }

		public Speed TimerSpeed {
			get => (Speed)(TAC & 0x03);
            set => TAC = (byte)((TAC & 0xFC) + value);
        }

		public bool IsRunning {
			get => (TAC & 0x04) != 0;
            set => TAC = (byte)((TAC & 0xFB) + (value?0x04:0x00));
        }


        private readonly Memory _memory;
		//Timer clock:  	 262144Hz (1/16 cpu speed)
		uint clock = 0;
		uint clockTmp = 0;

		//Divider clock:	  16384Hz (1/16 timer clock speed)
		uint dividerClockTmp = 0;

		//Counter clock 00:	   4096Hz (1/64 timer clock speed)
		//Counter clock 01:	 262144Hz (1 timer clock speed)
		//Counter clock 10:	  65536Hz (1/4 timer clock speed)
		//Counter clock 11:	  16384Hz (1/16 timer clock speed)


		public Timer(Memory memory)
		{
            _memory = memory;
			clock = 0;
			clockTmp = 0;
			dividerClockTmp = 0;
		}


		public void Step(uint opCycles)
		{
			clockTmp += opCycles;
			//1/16 cpu speed: increment main clock
			if (clockTmp >= 16) {
				clockTmp -= 16;
				clock++;

				//1/16 Increment divider
				dividerClockTmp++;
				if (dividerClockTmp == 16) {
					dividerClockTmp = 0;
					DIV++;
				}

			}

			if (IsRunning) {
				//1/x Increment counter
				if (clock >= (int)TimerSpeed) {
					clock = 0;
					TIMA++;
					if (TIMA == 0) {
						_memory.SetInterrupt(InterruptType.TimerOverflow);
						TIMA = TMA;
					}
				}
			}
		}
	}
}
