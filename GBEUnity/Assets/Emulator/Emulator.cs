using System.Collections.Generic;
using Emulator.Debugger;
using UnityEngine;
using Emulator.Graphics;
using Emulator.Memories;
using Emulator.Processor;
using Emulator.Timers;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Emulator
{

	public class Emulator : MonoBehaviour
    {

		public event System.Action<Emulator> OnEmulatorOn;
		public event System.Action<Emulator> OnEmulatorOff;
		public event System.Action<Emulator> OnEmulatorStep;

		public const float FPS = 59.7f;
        public string fileName;
		public Material outputMaterial;

		Coroutine emulatorStepCoroutine;
		public bool isOn { get; private set; }

		[HideInInspector] public bool paused = false;
		[HideInInspector] public CPU cpu;
		[HideInInspector] public Memory memory;
		[HideInInspector] public PPU ppu;
		[HideInInspector] public Timer timer;
		[HideInInspector] public Joypad joypad;
		bool skipBios = true;

        public IAudioOutput _audio;
        public GbVersion version;

        void Awake()
        {
            fileName = PlayerPrefs.GetString("file");
        }

		private void Init()
		{
			memory = new Memory(version);
			cpu = new CPU(memory);
			ppu = new PPU(memory);
			timer = new Timer(memory);
			joypad = new Joypad(memory);

            memory.soundChip.SetSampleRate(_audio.GetOutputSampleRate());

            if (outputMaterial != null) {
				outputMaterial.SetTexture("_MainTex", ppu.ScreenTexture);
			}

			InitKeyMap();
		}


		void Update()
		{
			if (isOn) {
				EmulatorFrame();
			}
		}


		#region Public

		public void TurnOn()
		{
			if (isOn) return;
			Init();

			if (skipBios) {
				SimulateBiosStartup();
			}

			if (fileName != null) {
				memory.LoadRom(fileName);
			}

            isOn = true;

            OnEmulatorOn?.Invoke(this);
        }


		public void TurnOff()
		{
			if (!isOn) return;
			isOn = false;

            OnEmulatorOff?.Invoke(this);
        }


		public void Reset()
		{
			TurnOff();
			TurnOn();
		}

		#endregion


		#region Private

		public void EmulatorFrame()
		{
			if (paused) return;

			CheckKeys();

			var cyclesPerFrame = cpu.clockSpeed / FPS;
			var fTime = cpu.timers.t + cyclesPerFrame;

			while (cpu.timers.t < fTime) {
                OnEmulatorStep?.Invoke(this);

                if (!paused) {
					EmulatorStep();
				} else {
					break;
				}
			}
            memory.soundChip.OutputSound(_audio);
        }


		public void EmulatorStep(bool frameskip = false)
		{
			var opCycles = cpu.Step();
			timer.Step(opCycles);
			ppu.Step(opCycles);
        }


		void SimulateBiosStartup()
		{
            cpu.registers.AF = (ushort) (version == GbVersion.Color ? 0x11B0 : 0x01B0); //0x01=GB/SGB, 0xFF=GBP, 0x11=GBC
			cpu.registers.BC = 0x0013;
			cpu.registers.DE = 0x00D8;
			cpu.registers.HL = 0x014D;
			cpu.registers.SP = 0xFFFE;
			cpu.registers.PC = 0x0100;

			//IO default values
			memory.Write((ushort)0xFF01, (byte)0x00);
			memory.Write((ushort)0xFF02, (byte)0x7E);
			memory.Write((ushort)0xFF04, (byte)0xAB);
			memory.Write((ushort)0xFF05, (byte)0x00);
			memory.Write((ushort)0xFF06, (byte)0x00);
			memory.Write((ushort)0xFF07, (byte)0x00);
			memory.Write((ushort)0xFF0F, (byte)0xE1);

			memory.Write((ushort)0xFF10, (byte)0x80);
			memory.Write((ushort)0xFF11, (byte)0xBF);
			memory.Write((ushort)0xFF12, (byte)0xF3);
			memory.Write((ushort)0xFF14, (byte)0xBF);
			memory.Write((ushort)0xFF16, (byte)0x3F);
			memory.Write((ushort)0xFF17, (byte)0x00);
			memory.Write((ushort)0xFF19, (byte)0xBF);
			memory.Write((ushort)0xFF1A, (byte)0x7F);
			memory.Write((ushort)0xFF1B, (byte)0xFF);
			memory.Write((ushort)0xFF1C, (byte)0x9F);
			memory.Write((ushort)0xFF1E, (byte)0xBF);

			memory.Write((ushort)0xFF21, (byte)0x00);
			memory.Write((ushort)0xFF22, (byte)0x00);
			memory.Write((ushort)0xFF23, (byte)0xBF);
			memory.Write((ushort)0xFF24, (byte)0x77);
			memory.Write((ushort)0xFF25, (byte)0xF3);
			memory.Write((ushort)0xFF26, (byte)(version == GbVersion.Super ? 0xF0 : 0xF1));

			memory.Write((ushort)0xFF40, (byte)0x91);
			memory.Write((ushort)0xFF41, (byte)0x85);
			memory.Write((ushort)0xFF42, (byte)0x00);
			memory.Write((ushort)0xFF43, (byte)0x00);
			memory.Write((ushort)0xFF44, (byte)0x00);
			memory.Write((ushort)0xFF45, (byte)0x00);
			memory.Write((ushort)0xFF47, (byte)0xFC);
			memory.Write((ushort)0xFF4A, (byte)0x00);
			memory.Write((ushort)0xFF4B, (byte)0x00);

			memory.Write((ushort)0xFF50, (byte)0x01);

			memory.Write((ushort)0xFFFF, (byte)0x00);
		}

		#endregion

		Dictionary<KeyCode, Button> keyMap;
		List<KeyCode> keys;

        private void InitKeyMap()
		{
            keyMap = new Dictionary<KeyCode, Button>
            {
                [KeyCode.LeftArrow] = Button.Left,
                [KeyCode.RightArrow] = Button.Right,
                [KeyCode.UpArrow] = Button.Up,
                [KeyCode.DownArrow] = Button.Down,
                [KeyCode.Z] = Button.A,
                [KeyCode.X] = Button.B,
                [KeyCode.Space] = Button.Start,
                [KeyCode.Delete] = Button.Select
            };

            keys = new List<KeyCode>();
			foreach (var kv in keyMap) {
				keys.Add(kv.Key);
			}
		}

        private void CheckKeys()
		{
			for (int i = 0; i < keys.Count; i++) {
				var key = keys[i];
				if (Input.GetKey(key)) {
					joypad.SetKey(keyMap[key], true);	
				} else {
					joypad.SetKey(keyMap[key], false);	
				}
			}
		}
	}
}
