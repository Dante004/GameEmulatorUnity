//Disable assignement to the same variable warning
#pragma warning disable 1717
//Disable comparission to the same variable warning
#pragma warning disable 1718

using UnityEngine;
using System;
using Emulator.Memories;

namespace Emulator.Processor
{

	public class CPU
    {

		public float clockSpeed = 4.16f * 1000000f; //4.16Mhz
		public Memory memory;

		public bool Halt { get; private set; }
		public bool Stop { get; private set; }

		public bool Ime { get; private set; }

		public struct Timers {
			public uint t;
		}

        public Timers timers;
		public Registers registers;
			
		public byte[] opCodeCycles = {
			4,  12, 8,  8,  4,  4,  8,  4,  20, 8,  8,  8, 4,  4,  8, 4,
			4,  12, 8,  8,  4,  4,  8,  4,  12, 8,  8,  8, 4,  4,  8, 4,
			8,  12, 8,  8,  4,  4,  8,  4,  8,  8,  8,  8, 4,  4,  8, 4,
			8,  12, 8,  8,  12, 12, 12, 4,  8,  8,  8,  8, 4,  4,  8, 4,
			4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4, 4,  4,  8, 4,
			4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4, 4,  4,  8, 4,
			4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4, 4,  4,  8, 4,
			8,  8,  8,  8,  8,  8,  4,  8,  4,  4,  4,  4, 4,  4,  8, 4,
			4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4, 4,  4,  8, 4,
			4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4, 4,  4,  8, 4,
			4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4, 4,  4,  8, 4,
			4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4, 4,  4,  8, 4,
			8,  12, 12, 16, 12, 16, 8,  16, 8,  16, 12, 4, 12, 24, 8, 16,
			8,  12, 12, 0,  12, 16, 8,  16, 8,  16, 12, 0, 12, 0,  8, 16,
			12, 12, 8,  0,  0,  16, 8,  16, 16, 4,  16, 0, 0,  0,  8, 16,
			12, 12, 8,  4,  0,  16, 8,  16, 12, 8,  16, 4, 0,  0,  8, 16,	
		};

		public byte[] opCodeCyclesCB = {
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8,
			8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8
		};


		public Action[] operations;
		public Action[] cbOperations;

		public CPU(Memory memory) {
			this.memory = memory;
			timers.t = 0;
			Halt = false;

            registers = new Registers();

			operations = new Action[] {
				OP_00,OP_01,OP_02,OP_03,OP_04,OP_05,OP_06,OP_07,OP_08,OP_09,OP_0A,OP_0B,OP_0C,OP_0D,OP_0E,OP_0F,
				OP_10,OP_11,OP_12,OP_13,OP_14,OP_15,OP_16,OP_17,OP_18,OP_19,OP_1A,OP_1B,OP_1C,OP_1D,OP_1E,OP_1F,
				OP_20,OP_21,OP_22,OP_23,OP_24,OP_25,OP_26,OP_27,OP_28,OP_29,OP_2A,OP_2B,OP_2C,OP_2D,OP_2E,OP_2F,
				OP_30,OP_31,OP_32,OP_33,OP_34,OP_35,OP_36,OP_37,OP_38,OP_39,OP_3A,OP_3B,OP_3C,OP_3D,OP_3E,OP_3F,
				OP_40,OP_41,OP_42,OP_43,OP_44,OP_45,OP_46,OP_47,OP_48,OP_49,OP_4A,OP_4B,OP_4C,OP_4D,OP_4E,OP_4F,
				OP_50,OP_51,OP_52,OP_53,OP_54,OP_55,OP_56,OP_57,OP_58,OP_59,OP_5A,OP_5B,OP_5C,OP_5D,OP_5E,OP_5F,
				OP_60,OP_61,OP_62,OP_63,OP_64,OP_65,OP_66,OP_67,OP_68,OP_69,OP_6A,OP_6B,OP_6C,OP_6D,OP_6E,OP_6F,
				OP_70,OP_71,OP_72,OP_73,OP_74,OP_75,OP_76,OP_77,OP_78,OP_79,OP_7A,OP_7B,OP_7C,OP_7D,OP_7E,OP_7F,
				OP_80,OP_81,OP_82,OP_83,OP_84,OP_85,OP_86,OP_87,OP_88,OP_89,OP_8A,OP_8B,OP_8C,OP_8D,OP_8E,OP_8F,
				OP_90,OP_91,OP_92,OP_93,OP_94,OP_95,OP_96,OP_97,OP_98,OP_99,OP_9A,OP_9B,OP_9C,OP_9D,OP_9E,OP_9F,
				OP_A0,OP_A1,OP_A2,OP_A3,OP_A4,OP_A5,OP_A6,OP_A7,OP_A8,OP_A9,OP_AA,OP_AB,OP_AC,OP_AD,OP_AE,OP_AF,
				OP_B0,OP_B1,OP_B2,OP_B3,OP_B4,OP_B5,OP_B6,OP_B7,OP_B8,OP_B9,OP_BA,OP_BB,OP_BC,OP_BD,OP_BE,OP_BF,
				OP_C0,OP_C1,OP_C2,OP_C3,OP_C4,OP_C5,OP_C6,OP_C7,OP_C8,OP_C9,OP_CA,OP_CB,OP_CC,OP_CD,OP_CE,OP_CF,
				OP_D0,OP_D1,OP_D2,OP_XX,OP_D4,OP_D5,OP_D6,OP_D7,OP_D8,OP_D9,OP_DA,OP_XX,OP_DC,OP_XX,OP_DE,OP_DF,
				OP_E0,OP_E1,OP_E2,OP_XX,OP_XX,OP_E5,OP_E6,OP_E7,OP_E8,OP_E9,OP_EA,OP_XX,OP_XX,OP_XX,OP_EE,OP_EF,
				OP_F0,OP_F1,OP_F2,OP_F3,OP_XX,OP_F5,OP_F6,OP_F7,OP_F8,OP_F9,OP_FA,OP_FB,OP_XX,OP_XX,OP_FE,OP_FF
			};

			cbOperations = new Action[] {
				CB_00,CB_01,CB_02,CB_03,CB_04,CB_05,CB_06,CB_07,CB_08,CB_09,CB_0A,CB_0B,CB_0C,CB_0D,CB_0E,CB_0F,
				CB_10,CB_11,CB_12,CB_13,CB_14,CB_15,CB_16,CB_17,CB_18,CB_19,CB_1A,CB_1B,CB_1C,CB_1D,CB_1E,CB_1F,
				CB_20,CB_21,CB_22,CB_23,CB_24,CB_25,CB_26,CB_27,CB_28,CB_29,CB_2A,CB_2B,CB_2C,CB_2D,CB_2E,CB_2F,
				CB_30,CB_31,CB_32,CB_33,CB_34,CB_35,CB_36,CB_37,CB_38,CB_39,CB_3A,CB_3B,CB_3C,CB_3D,CB_3E,CB_3F,
				CB_40,CB_41,CB_42,CB_43,CB_44,CB_45,CB_46,CB_47,CB_48,CB_49,CB_4A,CB_4B,CB_4C,CB_4D,CB_4E,CB_4F,
				CB_50,CB_51,CB_52,CB_53,CB_54,CB_55,CB_56,CB_57,CB_58,CB_59,CB_5A,CB_5B,CB_5C,CB_5D,CB_5E,CB_5F,
				CB_60,CB_61,CB_62,CB_63,CB_64,CB_65,CB_66,CB_67,CB_68,CB_69,CB_6A,CB_6B,CB_6C,CB_6D,CB_6E,CB_6F,
				CB_70,CB_71,CB_72,CB_73,CB_74,CB_75,CB_76,CB_77,CB_78,CB_79,CB_7A,CB_7B,CB_7C,CB_7D,CB_7E,CB_7F,
				CB_80,CB_81,CB_82,CB_83,CB_84,CB_85,CB_86,CB_87,CB_88,CB_89,CB_8A,CB_8B,CB_8C,CB_8D,CB_8E,CB_8F,
				CB_90,CB_91,CB_92,CB_93,CB_94,CB_95,CB_96,CB_97,CB_98,CB_99,CB_9A,CB_9B,CB_9C,CB_9D,CB_9E,CB_9F,
				CB_A0,CB_A1,CB_A2,CB_A3,CB_A4,CB_A5,CB_A6,CB_A7,CB_A8,CB_A9,CB_AA,CB_AB,CB_AC,CB_AD,CB_AE,CB_AF,
				CB_B0,CB_B1,CB_B2,CB_B3,CB_B4,CB_B5,CB_B6,CB_B7,CB_B8,CB_B9,CB_BA,CB_BB,CB_BC,CB_BD,CB_BE,CB_BF,
				CB_C0,CB_C1,CB_C2,CB_C3,CB_C4,CB_C5,CB_C6,CB_C7,CB_C8,CB_C9,CB_CA,CB_CB,CB_CC,CB_CD,CB_CE,CB_CF,
				CB_D0,CB_D1,CB_D2,CB_D3,CB_D4,CB_D5,CB_D6,CB_D7,CB_D8,CB_D9,CB_DA,CB_DB,CB_DC,CB_DD,CB_DE,CB_DF,
				CB_E0,CB_E1,CB_E2,CB_E3,CB_E4,CB_E5,CB_E6,CB_E7,CB_E8,CB_E9,CB_EA,CB_EB,CB_EC,CB_ED,CB_EE,CB_EF,
				CB_F0,CB_F1,CB_F2,CB_F3,CB_F4,CB_F5,CB_F6,CB_F7,CB_F8,CB_F9,CB_FA,CB_FB,CB_FC,CB_FD,CB_FE,CB_FF
			};
		}

		public uint Step()
		{
			uint opCycles = 0;
			uint interruptCycles = 0;

			if (Halt) {
				opCycles = 4;	
			} else {
				var op = memory.Read(registers.PC++);
				opCycles = opCodeCycles[op];
				operations[op]();
			}

			Halt = Halt && !memory.HasInterrupts();

			if (Ime && memory.HasInterrupts()) {
				Ime = false;
				interruptCycles = 12;

				if (memory.CheckInterrupt(InterruptType.VBlank)) {
					memory.ClearInterrupt(InterruptType.VBlank);
					RST_40();
				} else if (memory.CheckInterrupt(InterruptType.LCDCStatus)) {
					memory.ClearInterrupt(InterruptType.LCDCStatus);
					RST_48();
				} else if (memory.CheckInterrupt(InterruptType.TimerOverflow)) {
					memory.ClearInterrupt(InterruptType.TimerOverflow);
					RST_50();
				} else if (memory.CheckInterrupt(InterruptType.SerialTransferCompletion)) {
					memory.ClearInterrupt(InterruptType.SerialTransferCompletion);
					RST_58();
				} else if (memory.CheckInterrupt(InterruptType.HighToLowP10P13)) {
					memory.ClearInterrupt(InterruptType.HighToLowP10P13);
					RST_60();
				} else {
					Debug.Log("PC: unknown interrupt");
					Ime = true;
					interruptCycles = 0;
				}
			}

			timers.t += opCycles;

			return opCycles + interruptCycles;
		}


		#region 8bit loads

		//ld-nn-n
		private void OP_06() { registers.B=memory.Read(registers.PC++); } //LD B n
        private void OP_0E() { registers.C=memory.Read(registers.PC++); } //LD C n
        private void OP_16() { registers.D=memory.Read(registers.PC++); } //LD D n
        private void OP_1E() { registers.E=memory.Read(registers.PC++); } //LD E n
        private void OP_26() { registers.H=memory.Read(registers.PC++); } //LD H n
        private void OP_2E() { registers.L=memory.Read(registers.PC++); } //LD L

        //ld-r1-r2
        private void OP_7F() { registers.A=registers.A; } //LD A A
        private void OP_78() { registers.A=registers.B; } //LD A B
        private void OP_79() { registers.A=registers.C; } //LD A C
        private void OP_7A() { registers.A=registers.D; } //LD A D
        private void OP_7B() { registers.A=registers.E; } //LD A E
        private void OP_7C() { registers.A=registers.H; } //LD A H
        private void OP_7D() { registers.A=registers.L; } //LD A L
        private void OP_7E() { registers.A=memory.Read(registers.HL); } //LD A (HL)
        private void OP_40() { registers.B=registers.B; } //LD B B
        private void OP_41() { registers.B=registers.C; } //LD B C
        private void OP_42() { registers.B=registers.D; } //LD B D
        private void OP_43() { registers.B=registers.E; } //LD B E
        private void OP_44() { registers.B=registers.H; } //LD B H
        private void OP_45() { registers.B=registers.L; } //LD B L
        private void OP_46() { registers.B=memory.Read(registers.HL); } //LD B (HL)
        private void OP_48() { registers.C=registers.B; } //LD C B
        private void OP_49() { registers.C=registers.C; } //LD C C
        private void OP_4A() { registers.C=registers.D; } //LD C D
        private void OP_4B() { registers.C=registers.E; } //LD C E
        private void OP_4C() { registers.C=registers.H; } //LD C H
        private void OP_4D() { registers.C=registers.L; } //LD C L
        private void OP_4E() { registers.C=memory.Read(registers.HL); } //LD C (HL)
        private void OP_50() { registers.D=registers.B; } //LD D B
        private void OP_51() { registers.D=registers.C; } //LD D C
        private void OP_52() { registers.D=registers.D; } //LD D D
        private void OP_53() { registers.D=registers.E; } //LD D E
        private void OP_54() { registers.D=registers.H; } //LD D H
        private void OP_55() { registers.D=registers.L; } //LD D L
        private void OP_56() { registers.D=memory.Read(registers.HL); } //LD D (HL)
        private void OP_58() { registers.E=registers.B; } //LD E B
        private void OP_59() { registers.E=registers.C; } //LD E C
        private void OP_5A() { registers.E=registers.D; } //LD E D
        private void OP_5B() { registers.E=registers.E; } //LD E E
        private void OP_5C() { registers.E=registers.H; } //LD E H
        private void OP_5D() { registers.E=registers.L; } //LD E L
        private void OP_5E() { registers.E=memory.Read(registers.HL); } //LD E (HL)
        private void OP_60() { registers.H=registers.B; } //LD H B
        private void OP_61() { registers.H=registers.C; } //LD H C
        private void OP_62() { registers.H=registers.D; } //LD H D
        private void OP_63() { registers.H=registers.E; } //LD H E
        private void OP_64() { registers.H=registers.H; } //LD H H
        private void OP_65() { registers.H=registers.L; } //LD H L
        private void OP_66() { registers.H=memory.Read(registers.HL); } //LD H (HL)
        private void OP_68() { registers.L=registers.B; } //LD L B
        private void OP_69() { registers.L=registers.C; } //LD L C
        private void OP_6A() { registers.L=registers.D; } //LD L D
        private void OP_6B() { registers.L=registers.E; } //LD L E
        private void OP_6C() { registers.L=registers.H; } //LD L H
        private void OP_6D() { registers.L=registers.L; } //LD L L
        private void OP_6E() { registers.L=memory.Read(registers.HL); } //LD L (HL)
        private void OP_70() { memory.Write(registers.HL, registers.B); } //LD (HL) B
        private void OP_71() { memory.Write(registers.HL, registers.C); } //LD (HL) C
        private void OP_72() { memory.Write(registers.HL, registers.D); } //LD (HL) D
        private void OP_73() { memory.Write(registers.HL, registers.E); } //LD (HL) E
        private void OP_74() { memory.Write(registers.HL, registers.H); } //LD (HL) H
        private void OP_75() { memory.Write(registers.HL, registers.L); } //LD (HL) L
        private void OP_36() { memory.Write(registers.HL, memory.Read(registers.PC++)); } //LD (HL) n

        //ld-a-n
        private void OP_0A() { registers.A=memory.Read(registers.BC); } //LD A (BC)
        private void OP_1A() { registers.A=memory.Read(registers.DE); } //LD A (DE)
        private void OP_FA() { registers.A=memory.Read(memory.ReadW(registers.PC)); registers.PC+=2; } //LD A (nn)
        private void OP_3E() { registers.A=memory.Read(registers.PC++); } //LD A #

        //ld-n-a
        private void OP_47() { registers.B=registers.A; } //LD B A
        private void OP_4F() { registers.C=registers.A; } //LD C A
        private void OP_57() { registers.D=registers.A; } //LD D A
        private void OP_5F() { registers.E=registers.A; } //LD E A
        private void OP_67() { registers.H=registers.A; } //LD H A
        private void OP_6F() { registers.L=registers.A; } //LD L A
        private void OP_02() { memory.Write(registers.BC, registers.A); } //LD (BC) A
        private void OP_12() { memory.Write(registers.DE, registers.A); } //LD (DE) A
        private void OP_77() { memory.Write(registers.HL, registers.A); } //LD (HL) A
        private void OP_EA() { memory.Write(memory.ReadW(registers.PC), registers.A); registers.PC+=2; } //LD (nn) A

        //ld-a-(c)
        private void OP_F2() { registers.A=memory.Read((ushort)(0xFF00 + registers.C)); } //LD A,($FF00+C)

        //ld-(c)-a
        private void OP_E2() { memory.Write((ushort)(0xFF00 + registers.C), registers.A); } //LD ($FF00+C),A

        //ld-a-(hld)
        private void OP_3A() { registers.A=memory.Read(registers.HL); registers.HL--; } //LD A,(HL-)

		//ld-(hld)-a
        private void OP_32() { memory.Write(registers.HL, registers.A); registers.HL--; } //LD (HL-), A

		//ld-a-(hli)
        private void OP_2A() { registers.A=memory.Read(registers.HL); registers.HL++; } //LD A,(HL+)

		//ld-(hli)-a
        private void OP_22() { memory.Write(registers.HL, registers.A); registers.HL++; } //LD (HL+), A

		//ldh-(n)-a
        private void OP_E0() { memory.Write((ushort)(0xFF00 + memory.Read(registers.PC++)), registers.A); } //LD ($FF00+n),A 

		//ldh-a-(n)
        private void OP_F0() { registers.A = memory.Read((ushort)(0xFF00 + memory.Read(registers.PC++))); } //LD A,($FF00+n)

		#endregion

		#region 16bit loads

		//ld-n-nn
        private void OP_01() { registers.BC=memory.ReadW(registers.PC); registers.PC+=2; } //LD BC,nn
        private void OP_11() { registers.DE=memory.ReadW(registers.PC); registers.PC+=2; } //LD DE,nn
        private void OP_21() { registers.HL=memory.ReadW(registers.PC); registers.PC+=2; } //LD HL,nn
        private void OP_31() { registers.SP=memory.ReadW(registers.PC); registers.PC+=2; } //LD SP,nn

		//ld-sp-hl
        private void OP_F9() { registers.SP=registers.HL; } //LD SP,HL

		//ldhl-sp-n
        private void OP_F8() { 

			var m = DecodeSigned(memory.Read(registers.PC++));
			registers.HL = (ushort)(registers.SP + m); 

			registers.flagH = CheckHFlag((ushort)(registers.SP & 0xF), (ushort)m);
			registers.flagC = (registers.SP & 0xFF) > (registers.HL & 0xFF);
				
			registers.flagZ=false; 
			registers.flagN=false; 
		} //LDHL SP,n 

		//ld-nn-sp
        private void OP_08() { memory.WriteW(memory.ReadW(registers.PC), registers.SP); registers.PC+=2; } //LD (nn),SP

		//push-nn
        private void OP_F5() { registers.SP-=2; memory.WriteW(registers.SP,registers.AF); }// PUSH AF
        private void OP_C5() { registers.SP-=2; memory.WriteW(registers.SP,registers.BC); }// PUSH BC
        private void OP_D5() { registers.SP-=2; memory.WriteW(registers.SP,registers.DE); }// PUSH DE
        private void OP_E5() { registers.SP-=2; memory.WriteW(registers.SP,registers.HL); }// PUSH HL

		//pop-nn
        private void OP_F1() { registers.AF=memory.ReadW(registers.SP); registers.SP+=2; }  //POP AF
        private void OP_C1() { registers.BC=memory.ReadW(registers.SP); registers.SP+=2; }  //POP BC
        private void OP_D1() { registers.DE=memory.ReadW(registers.SP); registers.SP+=2; }  //POP DE
        private void OP_E1() { registers.HL=memory.ReadW(registers.SP); registers.SP+=2; }  //POP HL

		#endregion

		#region 8-bit ALU

		//add
        private void OP_87() { var a=registers.A; registers.A+=registers.A; registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=CheckHFlag(a, a); registers.flagC=(a>registers.A); } //LD A A
        private void OP_80() { var a=registers.A; registers.A+=registers.B; registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=CheckHFlag(a, registers.B); registers.flagC=(a>registers.A); } //LD A B
        private void OP_81() { var a=registers.A; registers.A+=registers.C; registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=CheckHFlag(a, registers.C); registers.flagC=(a>registers.A); } //LD A C
        private void OP_82() { var a=registers.A; registers.A+=registers.D; registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=CheckHFlag(a, registers.D); registers.flagC=(a>registers.A); } //LD A D
        private void OP_84() { var a=registers.A; registers.A+=registers.H; registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=CheckHFlag(a, registers.H); registers.flagC=(a>registers.A); } //LD A H
        private void OP_83() { var a=registers.A; registers.A+=registers.E; registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=CheckHFlag(a, registers.E); registers.flagC=(a>registers.A); } //LD A E
        private void OP_85() { var a=registers.A; registers.A+=registers.L; registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=CheckHFlag(a, registers.L); registers.flagC=(a>registers.A); } //LD A L
        private void OP_86() { var a=registers.A; var n = memory.Read(registers.HL); registers.A+=n; registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=CheckHFlag(a, n); registers.flagC=(a>registers.A); } //LD A (HL)
        private void OP_C6() { var a=registers.A; var n = memory.Read(registers.PC++); registers.A+=n; registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=CheckHFlag(a, n); registers.flagC=(a>registers.A); } //LD A #

		//adc
        private void OP_8F() { var n=registers.A; var a=registers.A; var result=a+n+(registers.flagC?1:0); registers.A = (byte)(result & 0xFF); registers.flagN = false; registers.flagH = ((a^n^registers.A) & 0x10) != 0; registers.flagC = (result > 0xFF); registers.flagZ = (registers.A == 0); } //ADC A #
        private void OP_88() { var n=registers.B; var a=registers.A; var result=a+n+(registers.flagC?1:0); registers.A = (byte)(result & 0xFF); registers.flagN = false; registers.flagH = ((a^n^registers.A) & 0x10) != 0; registers.flagC = (result > 0xFF); registers.flagZ = (registers.A == 0); } //ADC A #
        private void OP_89() { var n=registers.C; var a=registers.A; var result=a+n+(registers.flagC?1:0); registers.A = (byte)(result & 0xFF); registers.flagN = false; registers.flagH = ((a^n^registers.A) & 0x10) != 0; registers.flagC = (result > 0xFF); registers.flagZ = (registers.A == 0); } //ADC A #
        private void OP_8A() { var n=registers.D; var a=registers.A; var result=a+n+(registers.flagC?1:0); registers.A = (byte)(result & 0xFF); registers.flagN = false; registers.flagH = ((a^n^registers.A) & 0x10) != 0; registers.flagC = (result > 0xFF); registers.flagZ = (registers.A == 0); } //ADC A #
        private void OP_8B() { var n=registers.E; var a=registers.A; var result=a+n+(registers.flagC?1:0); registers.A = (byte)(result & 0xFF); registers.flagN = false; registers.flagH = ((a^n^registers.A) & 0x10) != 0; registers.flagC = (result > 0xFF); registers.flagZ = (registers.A == 0); } //ADC A #
        private void OP_8C() { var n=registers.H; var a=registers.A; var result=a+n+(registers.flagC?1:0); registers.A = (byte)(result & 0xFF); registers.flagN = false; registers.flagH = ((a^n^registers.A) & 0x10) != 0; registers.flagC = (result > 0xFF); registers.flagZ = (registers.A == 0); } //ADC A #
        private void OP_8D() { var n=registers.L; var a=registers.A; var result=a+n+(registers.flagC?1:0); registers.A = (byte)(result & 0xFF); registers.flagN = false; registers.flagH = ((a^n^registers.A) & 0x10) != 0; registers.flagC = (result > 0xFF); registers.flagZ = (registers.A == 0); } //ADC A #
        private void OP_8E() { var n=memory.Read(registers.HL); var a=registers.A; var result=a+n+(registers.flagC?1:0); registers.A = (byte)(result & 0xFF); registers.flagN = false; registers.flagH = ((a^n^registers.A) & 0x10) != 0; registers.flagC = (result > 0xFF); registers.flagZ = (registers.A == 0); } //ADC A #
        private void OP_CE() { var n=memory.Read(registers.PC++); var a=registers.A; var result=a+n+(registers.flagC?1:0); registers.A = (byte)(result & 0xFF); registers.flagN = false; registers.flagH = ((a^n^registers.A) & 0x10) != 0; registers.flagC = (result > 0xFF); registers.flagZ = (registers.A == 0); } //ADC A #

		//sub
        private void OP_97() { var a=registers.A; registers.A-=registers.A; registers.flagZ=(registers.A==0); registers.flagN=true; registers.flagH=CheckHFlag(a, registers.A, true); registers.flagC=(a<registers.A); } //SUB A
        private void OP_90() { var a=registers.A; registers.A-=registers.B; registers.flagZ=(registers.A==0); registers.flagN=true; registers.flagH=CheckHFlag(a, registers.B, true); registers.flagC=(a<registers.A); } //SUB B
        private void OP_91() { var a=registers.A; registers.A-=registers.C; registers.flagZ=(registers.A==0); registers.flagN=true; registers.flagH=CheckHFlag(a, registers.C, true); registers.flagC=(a<registers.A); } //SUB C
        private void OP_92() { var a=registers.A; registers.A-=registers.D; registers.flagZ=(registers.A==0); registers.flagN=true; registers.flagH=CheckHFlag(a, registers.D, true); registers.flagC=(a<registers.A); } //SUB D
        private void OP_93() { var a=registers.A; registers.A-=registers.E; registers.flagZ=(registers.A==0); registers.flagN=true; registers.flagH=CheckHFlag(a, registers.E, true); registers.flagC=(a<registers.A); } //SUB E
        private void OP_94() { var a=registers.A; registers.A-=registers.H; registers.flagZ=(registers.A==0); registers.flagN=true; registers.flagH=CheckHFlag(a, registers.H, true); registers.flagC=(a<registers.A); } //SUB H
        private void OP_95() { var a=registers.A; registers.A-=registers.L; registers.flagZ=(registers.A==0); registers.flagN=true; registers.flagH=CheckHFlag(a, registers.L, true); registers.flagC=(a<registers.A); } //SUB L
        private void OP_96() { var a=registers.A; var n = memory.Read(registers.HL); registers.A-=n; registers.flagZ=(registers.A==0); registers.flagN=true; registers.flagH=CheckHFlag(a, n, true); registers.flagC=(a<registers.A); } //SUB (HL)
        private void OP_D6() { var a=registers.A; var n = memory.Read(registers.PC++); registers.A-=n; registers.flagZ=(registers.A==0); registers.flagN=true; registers.flagH=CheckHFlag(a, n, true); registers.flagC=(a<registers.A); } //SUB #

		//sbc
        private void OP_9F() { var n=registers.A; var a=registers.A; var result=a-n-(registers.flagC?1:0); registers.A=(byte)(result & 0xFF); registers.flagN= true; registers.flagH=((a^n^registers.A)&0x10)!=0; registers.flagC=(result < 0); registers.flagZ=(registers.A == 0); }
        private void OP_98() { var n=registers.B; var a=registers.A; var result=a-n-(registers.flagC?1:0); registers.A=(byte)(result & 0xFF); registers.flagN= true; registers.flagH=((a^n^registers.A)&0x10)!=0; registers.flagC=(result < 0); registers.flagZ=(registers.A == 0); }
        private void OP_99() { var n=registers.C; var a=registers.A; var result=a-n-(registers.flagC?1:0); registers.A=(byte)(result & 0xFF); registers.flagN= true; registers.flagH=((a^n^registers.A)&0x10)!=0; registers.flagC=(result < 0); registers.flagZ=(registers.A == 0); }
        private void OP_9A() { var n=registers.D; var a=registers.A; var result=a-n-(registers.flagC?1:0); registers.A=(byte)(result & 0xFF); registers.flagN= true; registers.flagH=((a^n^registers.A)&0x10)!=0; registers.flagC=(result < 0); registers.flagZ=(registers.A == 0); }
        private void OP_9B() { var n=registers.E; var a=registers.A; var result=a-n-(registers.flagC?1:0); registers.A=(byte)(result & 0xFF); registers.flagN= true; registers.flagH=((a^n^registers.A)&0x10)!=0; registers.flagC=(result < 0); registers.flagZ=(registers.A == 0); }
        private void OP_9C() { var n=registers.H; var a=registers.A; var result=a-n-(registers.flagC?1:0); registers.A=(byte)(result & 0xFF); registers.flagN= true; registers.flagH=((a^n^registers.A)&0x10)!=0; registers.flagC=(result < 0); registers.flagZ=(registers.A == 0); }
        private void OP_9D() { var n=registers.L; var a=registers.A; var result=a-n-(registers.flagC?1:0); registers.A=(byte)(result & 0xFF); registers.flagN= true; registers.flagH=((a^n^registers.A)&0x10)!=0; registers.flagC=(result < 0); registers.flagZ=(registers.A == 0); }
        private void OP_9E() { var n=memory.Read(registers.HL); var a=registers.A; var result=a-n-(registers.flagC?1:0); registers.A=(byte)(result & 0xFF); registers.flagN= true; registers.flagH=((a^n^registers.A)&0x10)!=0; registers.flagC=(result < 0); registers.flagZ=(registers.A == 0); }
        private void OP_DE() { var n=memory.Read(registers.PC++); var a=registers.A; var result=a-n-(registers.flagC?1:0); registers.A=(byte)(result & 0xFF); registers.flagN= true; registers.flagH=((a^n^registers.A)&0x10)!=0; registers.flagC=(result < 0); registers.flagZ=(registers.A == 0); }

		//and-n
        private void OP_A7() { registers.A=(byte)(registers.A&registers.A); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=true; registers.flagC=false; } //AND A
        private void OP_A0() { registers.A=(byte)(registers.A&registers.B); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=true; registers.flagC=false; } //AND B
        private void OP_A1() { registers.A=(byte)(registers.A&registers.C); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=true; registers.flagC=false; } //AND C
        private void OP_A2() { registers.A=(byte)(registers.A&registers.D); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=true; registers.flagC=false; } //AND D
        private void OP_A3() { registers.A=(byte)(registers.A&registers.E); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=true; registers.flagC=false; } //AND E
        private void OP_A4() { registers.A=(byte)(registers.A&registers.H); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=true; registers.flagC=false; } //AND H
        private void OP_A5() { registers.A=(byte)(registers.A&registers.L); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=true; registers.flagC=false; } //AND L
        private void OP_A6() { registers.A=(byte)(registers.A&memory.Read(registers.HL)); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=true; registers.flagC=false; } //AND (HL)
        private void OP_E6() { registers.A=(byte)(registers.A&memory.Read(registers.PC++)); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=true; registers.flagC=false; } //AND #

		//or-n
        private void OP_B7() { registers.A=(byte)(registers.A|registers.A); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //OR A
        private void OP_B0() { registers.A=(byte)(registers.A|registers.B); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //OR B
        private void OP_B1() { registers.A=(byte)(registers.A|registers.C); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //OR C
        private void OP_B2() { registers.A=(byte)(registers.A|registers.D); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //OR D
        private void OP_B3() { registers.A=(byte)(registers.A|registers.E); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //OR E
        private void OP_B4() { registers.A=(byte)(registers.A|registers.H); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //OR H
        private void OP_B5() { registers.A=(byte)(registers.A|registers.L); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //OR L
        private void OP_B6() { registers.A=(byte)(registers.A|memory.Read(registers.HL)); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //OR (HL)
        private void OP_F6() { registers.A=(byte)(registers.A|memory.Read(registers.PC++)); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //OR #

		//xor-n
        private void OP_AF() { registers.A=(byte)(registers.A^registers.A); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //XOR A
        private void OP_A8() { registers.A=(byte)(registers.A^registers.B); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //XOR B
        private void OP_A9() { registers.A=(byte)(registers.A^registers.C); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //XOR C
        private void OP_AA() { registers.A=(byte)(registers.A^registers.D); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //XOR D
        private void OP_AB() { registers.A=(byte)(registers.A^registers.E); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //XOR E
        private void OP_AC() { registers.A=(byte)(registers.A^registers.H); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //XOR H
        private void OP_AD() { registers.A=(byte)(registers.A^registers.L); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //XOR L
        private void OP_AE() { registers.A=(byte)(registers.A^memory.Read(registers.HL)); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //XOR (HL)
        private void OP_EE() { registers.A=(byte)(registers.A^memory.Read(registers.PC++)); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; } //XOR #

		//cp-n
        private void OP_BF() { registers.flagZ=(registers.A==registers.A); registers.flagN=true; registers.flagH=CheckHFlag(registers.A, registers.A, true); registers.flagC=(registers.A<registers.A); } //CP A
        private void OP_B8() { registers.flagZ=(registers.A==registers.B); registers.flagN=true; registers.flagH=CheckHFlag(registers.A, registers.B, true); registers.flagC=(registers.A<registers.B); } //CP B
        private void OP_B9() { registers.flagZ=(registers.A==registers.C); registers.flagN=true; registers.flagH=CheckHFlag(registers.A, registers.C, true); registers.flagC=(registers.A<registers.C); } //CP C
        private void OP_BA() { registers.flagZ=(registers.A==registers.D); registers.flagN=true; registers.flagH=CheckHFlag(registers.A, registers.D, true); registers.flagC=(registers.A<registers.D); } //CP D
        private void OP_BB() { registers.flagZ=(registers.A==registers.E); registers.flagN=true; registers.flagH=CheckHFlag(registers.A, registers.E, true); registers.flagC=(registers.A<registers.E); } //CP E
        private void OP_BC() { registers.flagZ=(registers.A==registers.H); registers.flagN=true; registers.flagH=CheckHFlag(registers.A, registers.H, true); registers.flagC=(registers.A<registers.H); } //CP H
        private void OP_BD() { registers.flagZ=(registers.A==registers.L); registers.flagN=true; registers.flagH=CheckHFlag(registers.A, registers.L, true); registers.flagC=(registers.A<registers.L); } //CP L
        private void OP_BE() { byte m=memory.Read(registers.HL); registers.flagZ=(registers.A==m); registers.flagN=true; registers.flagH=CheckHFlag(registers.A, m, true); registers.flagC=(registers.A<m); } //CP (HL)
        private void OP_FE() { byte m=memory.Read(registers.PC++); registers.flagZ=(registers.A==m); registers.flagN=true; registers.flagH=CheckHFlag(registers.A, m, true); registers.flagC=(registers.A<m); } //CP #

		//inc-n
        private void OP_3C() { var a=registers.A; registers.A++; registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=CheckHFlag(a, 1); } //INC A
        private void OP_04() { var b = registers.B; registers.B++; registers.flagZ=(registers.B==0); registers.flagN=false; registers.flagH=CheckHFlag(b, 1); } //INC B
        private void OP_0C() { var c = registers.C; registers.C++; registers.flagZ=(registers.C==0); registers.flagN=false; registers.flagH=CheckHFlag(c, 1); } //INC C
        private void OP_14() { var d = registers.D; registers.D++; registers.flagZ=(registers.D==0); registers.flagN=false; registers.flagH=CheckHFlag(d, 1); } //INC D
        private void OP_1C() { var e = registers.E; registers.E++; registers.flagZ=(registers.E==0); registers.flagN=false; registers.flagH=CheckHFlag(e, 1); } //INC E
        private void OP_24() { var h = registers.H; registers.H++; registers.flagZ=(registers.H==0); registers.flagN=false; registers.flagH=CheckHFlag(h, 1); } //INC H
        private void OP_2C() { var l = registers.L; registers.L++; registers.flagZ=(registers.L==0); registers.flagN=false; registers.flagH=CheckHFlag(l, 1); } //INC L
        private void OP_34() { byte m = (byte)(memory.Read(registers.HL)+1); memory.Write(registers.HL,m); registers.flagZ=(m==0); registers.flagN=false; registers.flagH=CheckHFlag((byte)(m - 1), 1); } //INC (HL)

		//dec-n
        private void OP_3D() { var a=registers.A; registers.A--; registers.flagZ=(registers.A==0); registers.flagN=true; registers.flagH=CheckHFlag(a, 1, true); } //DEC A
        private void OP_05() { var b = registers.B; registers.B--; registers.flagZ=(registers.B==0); registers.flagN=true; registers.flagH=CheckHFlag(b, 1, true); } //DEC B
        private void OP_0D() { var c = registers.C; registers.C--; registers.flagZ=(registers.C==0); registers.flagN=true; registers.flagH=CheckHFlag(c, 1, true); } //DEC C
        private void OP_15() { var d = registers.D; registers.D--; registers.flagZ=(registers.D==0); registers.flagN=true; registers.flagH=CheckHFlag(d, 1, true); } //DEC D
        private void OP_1D() { var e = registers.E; registers.E--; registers.flagZ=(registers.E==0); registers.flagN=true; registers.flagH=CheckHFlag(e, 1, true); } //DEC E
        private void OP_25() { var h = registers.H; registers.H--; registers.flagZ=(registers.H==0); registers.flagN=true; registers.flagH=CheckHFlag(h, 1, true); } //DEC H
        private void OP_2D() { var l = registers.L; registers.L--; registers.flagZ=(registers.L==0); registers.flagN=true; registers.flagH=CheckHFlag(l, 1, true); } //DEC L
        private void OP_35() { var m=(byte)(memory.Read(registers.HL)-1); memory.Write(registers.HL,m); registers.flagZ=(m==0); registers.flagN=true; registers.flagH=CheckHFlag((byte)(m + 1), 1, true); } //DEC (HL)


		#endregion

		#region 16-bit ALU

		//add-hl-n
        private void OP_09() { var hl=registers.HL; registers.HL+=registers.BC; registers.flagN=false; registers.flagH=CheckHFlag(hl, registers.BC, is16bit:true); registers.flagC=(hl>registers.HL); } //ADD HL BC
        private void OP_19() { var hl=registers.HL; registers.HL+=registers.DE; registers.flagN=false; registers.flagH=CheckHFlag(hl, registers.DE, is16bit:true); registers.flagC=(hl>registers.HL); } //ADD HL DE
        private void OP_29() { var hl=registers.HL; registers.HL+=registers.HL; registers.flagN=false; registers.flagH=CheckHFlag(hl, registers.HL, is16bit:true); registers.flagC=(hl>registers.HL); } //ADD HL HL
        private void OP_39() { var hl=registers.HL; registers.HL+=registers.SP; registers.flagN=false; registers.flagH=CheckHFlag(hl, registers.SP, is16bit:true); registers.flagC=(hl>registers.HL); } //ADD HL SP

		//add-sp-n
        private void OP_E8() { 

			var sp=registers.SP;
			var m = DecodeSigned(memory.Read(registers.PC++));
			registers.SP = (ushort)(registers.SP + m);

			registers.flagH = CheckHFlag((ushort)(sp & 0xF), (ushort)m);
			registers.flagC = (sp & 0xFF) > (registers.SP & 0xFF);	

			registers.flagZ=false; 
			registers.flagN=false;
		}

		//inc-nn
        private void OP_03() { registers.BC++; } //INC BC
        private void OP_13() { registers.DE++; } //INC DE
        private void OP_23() { registers.HL++; } //INC HL
        private void OP_33() { registers.SP++; } //INC SP

		//dec-nn
        private void OP_0B() { registers.BC--; } //DEC BC
        private void OP_1B() { registers.DE--; } //DEC DE
        private void OP_2B() { registers.HL--; } //DEC HL
        private void OP_3B() { registers.SP--; } //DEC SP

		#endregion

		#region misc functions

		//DAA
        private void OP_27()
        {
            var a=registers.A;

			if (!registers.flagN) {
				if (registers.flagC || a > 0x99) { a += 0x60; registers.flagC = true; }
				if (registers.flagH || (a & 0x0F) > 0x09) { a += 0x06; }
			} else {
				if (registers.flagC) { a -= 0x60; }
				if (registers.flagH) { a -= 0x06; }
			}

			registers.A = a;
			registers.flagZ = (registers.A == 0);
			registers.flagH = false;
		}

		//cpl
        private void OP_2F() { registers.A=(byte)(~registers.A); registers.flagN=true; registers.flagH=true; }

		//ccf
        private void OP_3F() { registers.flagC=!registers.flagC; registers.flagN=false; registers.flagH=false; }

		//scf
        private void OP_37() { registers.flagC=true; registers.flagN=false; registers.flagH=false; }

		//nop
        private void OP_00() {}

		//halt
        private void OP_76() { Halt = true; }

		//stop
        private void OP_10() { Stop = true; }

		//di
        private void OP_F3() { Ime = false; }

		//ei
        private void OP_FB() { Ime = true; }

		#endregion

		#region Rotates & shifts

		//rlca
        private void OP_07() { 
			registers.flagC = ((registers.A >> 7) != 0); 
			registers.A = (byte)((registers.A << 1) | (registers.flagC?0x01:0x00)); 
			registers.flagZ = false; 
			registers.flagH = false; 
			registers.flagN = false; 
		}

		//rla
        private void OP_17() {
			var result = (byte)((registers.A << 1) | (registers.flagC?0x01:0x00));
			registers.flagC = ((registers.A >> 7) != 0); 
			registers.A = result;
			registers.flagZ = false; 
			registers.flagH = false; 
			registers.flagN = false; 
		}


		//rrca
        private void OP_0F() { 
			registers.flagC = ((registers.A & 0x01) != 0); 
			registers.A = (byte)((registers.A >> 1) | ((registers.flagC?0x01:0x00) << 7)); 
			registers.flagZ = false; 
			registers.flagH = false; 
			registers.flagN = false;
		}

		//rra
        private void OP_1F() {
			var result = (byte)((registers.A >> 1) | ((registers.flagC ? 0x01 : 0x00) << 7));
			registers.flagC = ((registers.A&0x01) != 0); 
			registers.A = result;
			registers.flagZ = false; 
			registers.flagH = false; 
			registers.flagN = false; 
		}
		
		#endregion

		#region Jumps

		//jp nn
        private void OP_C3() { registers.PC = memory.ReadW(registers.PC); }

		//jp cc,nn
        private void OP_C2() { if (!registers.flagZ) { registers.PC=memory.ReadW(registers.PC); } else { registers.PC+=2; } }
        private void OP_CA() { if (registers.flagZ) { registers.PC=memory.ReadW(registers.PC); } else { registers.PC+=2; } }
        private void OP_D2() { if (!registers.flagC) { registers.PC=memory.ReadW(registers.PC); } else { registers.PC+=2; } }
        private void OP_DA() { if (registers.flagC) { registers.PC=memory.ReadW(registers.PC); } else { registers.PC+=2; } }

		//jp hl
        private void OP_E9() { registers.PC = registers.HL; }

		//jr n
        private void OP_18() { var val = DecodeSigned(memory.Read(registers.PC++)); registers.PC = (ushort)(registers.PC+val); }

		//jr cc,n
        private void OP_20() { if (!registers.flagZ) { var val = DecodeSigned(memory.Read(registers.PC++)); registers.PC = (ushort)(registers.PC+val); } else { registers.PC++; } }
        private void OP_28() { if (registers.flagZ) { var val = DecodeSigned(memory.Read(registers.PC++)); registers.PC = (ushort)(registers.PC+val); } else { registers.PC++; } }
        private void OP_30() { if (!registers.flagC) { var val = DecodeSigned(memory.Read(registers.PC++)); registers.PC = (ushort)(registers.PC+val); } else { registers.PC++; } }
        private void OP_38() { if (registers.flagC) { var val = DecodeSigned(memory.Read(registers.PC++)); registers.PC = (ushort)(registers.PC+val); } else { registers.PC++; } }

		#endregion

		#region Calls

		//call nn
        private void OP_CD() { registers.SP -= 2; memory.WriteW(registers.SP, (ushort)(registers.PC+2)); registers.PC=memory.ReadW(registers.PC); }

		//call cc,nn
        private void OP_C4() { if (!registers.flagZ) { registers.SP -= 2; memory.WriteW(registers.SP, (ushort)(registers.PC+2)); registers.PC=memory.ReadW(registers.PC); } else { registers.PC+=2; } }
        private void OP_CC() { if (registers.flagZ) { registers.SP -= 2; memory.WriteW(registers.SP, (ushort)(registers.PC+2)); registers.PC=memory.ReadW(registers.PC); } else { registers.PC+=2; } }
        private void OP_D4() { if (!registers.flagC) { registers.SP -= 2; memory.WriteW(registers.SP, (ushort)(registers.PC+2)); registers.PC=memory.ReadW(registers.PC); } else { registers.PC+=2; } }
        private void OP_DC() { if (registers.flagC) { registers.SP -= 2; memory.WriteW(registers.SP, (ushort)(registers.PC+2)); registers.PC=memory.ReadW(registers.PC); } else { registers.PC+=2; } }

		#endregion

		#region Restarts & returns

		//rst
        private void OP_C7() { registers.SP -= 2; memory.WriteW(registers.SP, registers.PC); registers.PC=0x00; } //RST 00H
        private void OP_CF() { registers.SP -= 2; memory.WriteW(registers.SP, registers.PC); registers.PC=0x08; } //RST 08H
		void OP_D7() { registers.SP -= 2; memory.WriteW(registers.SP, registers.PC); registers.PC=0x10; } //RST 10H
		void OP_DF() { registers.SP -= 2; memory.WriteW(registers.SP, registers.PC); registers.PC=0x18; } //RST 18H
		void OP_E7() { registers.SP -= 2; memory.WriteW(registers.SP, registers.PC); registers.PC=0x20; } //RST 20H
		void OP_EF() { registers.SP -= 2; memory.WriteW(registers.SP, registers.PC); registers.PC=0x28; } //RST 28H
		void OP_F7() { registers.SP -= 2; memory.WriteW(registers.SP, registers.PC); registers.PC=0x30; } //RST 30H
		void OP_FF() { registers.SP -= 2; memory.WriteW(registers.SP, registers.PC); registers.PC=0x38; } //RST 38H

		void RST_40() { registers.SP -= 2; memory.WriteW(registers.SP, registers.PC); registers.PC=0x40; }
		void RST_48() { registers.SP -= 2; memory.WriteW(registers.SP, registers.PC); registers.PC=0x48; }
		void RST_50() { registers.SP -= 2; memory.WriteW(registers.SP, registers.PC); registers.PC=0x50; }
		void RST_58() { registers.SP -= 2; memory.WriteW(registers.SP, registers.PC); registers.PC=0x58; }
		void RST_60() { registers.SP -= 2; memory.WriteW(registers.SP, registers.PC); registers.PC=0x60; }

		//ret
		void OP_C9() { registers.PC=memory.ReadW(registers.SP); registers.SP+=2; }

		//ret cc
		void OP_C0() { if (!registers.flagZ) { registers.PC=memory.ReadW(registers.SP); registers.SP+=2; } }
		void OP_C8() { if (registers.flagZ) { registers.PC=memory.ReadW(registers.SP); registers.SP+=2; } }
		void OP_D0() { if (!registers.flagC) { registers.PC=memory.ReadW(registers.SP); registers.SP+=2; } }
		void OP_D8() { if (registers.flagC) { registers.PC=memory.ReadW(registers.SP); registers.SP+=2; } }

		//reti
		void OP_D9() { registers.PC=memory.ReadW(registers.SP); registers.SP+=2; Ime = true; }

		#endregion

		#region CB operations

        private void OP_CB() {
			var op = memory.Read(registers.PC++);
			cbOperations[op]();
			timers.t += opCodeCyclesCB[op];
		}

		//swap
		void CB_37() { byte tmp=registers.A; registers.A = (byte)(((tmp&0x0F)<<4)|((tmp&0xF0)>>4)); registers.flagZ=(registers.A==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; }
		void CB_30() { byte tmp=registers.B; registers.B = (byte)(((tmp&0x0F)<<4)|((tmp&0xF0)>>4)); registers.flagZ=(registers.B==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; }
		void CB_31() { byte tmp=registers.C; registers.C = (byte)(((tmp&0x0F)<<4)|((tmp&0xF0)>>4)); registers.flagZ=(registers.C==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; }
		void CB_32() { byte tmp=registers.D; registers.D = (byte)(((tmp&0x0F)<<4)|((tmp&0xF0)>>4)); registers.flagZ=(registers.D==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; }
		void CB_33() { byte tmp=registers.E; registers.E = (byte)(((tmp&0x0F)<<4)|((tmp&0xF0)>>4)); registers.flagZ=(registers.E==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; }
		void CB_34() { byte tmp=registers.H; registers.H = (byte)(((tmp&0x0F)<<4)|((tmp&0xF0)>>4)); registers.flagZ=(registers.H==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; }
		void CB_35() { byte tmp=registers.L; registers.L = (byte)(((tmp&0x0F)<<4)|((tmp&0xF0)>>4)); registers.flagZ=(registers.L==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; }
		void CB_36() { byte tmp=memory.Read(registers.HL); memory.Write(registers.HL,(byte)(((tmp&0x0F)<<4)|((tmp&0xF0)>>4))); registers.flagZ=(memory.Read(registers.HL)==0); registers.flagN=false; registers.flagH=false; registers.flagC=false; }

		//rlc
		void CB_07() { registers.flagC=((registers.A & 0x80)!=0);  registers.A=(byte)((registers.A<<1)|(registers.flagC?0x01:0x00));  registers.flagZ=(registers.A==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_00() { registers.flagC=((registers.B & 0x80)!=0);  registers.B=(byte)((registers.B<<1)|(registers.flagC?0x01:0x00));  registers.flagZ=(registers.B==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_01() { registers.flagC=((registers.C & 0x80)!=0);  registers.C=(byte)((registers.C<<1)|(registers.flagC?0x01:0x00));  registers.flagZ=(registers.C==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_02() { registers.flagC=((registers.D & 0x80)!=0);  registers.D=(byte)((registers.D<<1)|(registers.flagC?0x01:0x00));  registers.flagZ=(registers.D==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_03() { registers.flagC=((registers.E & 0x80)!=0);  registers.E=(byte)((registers.E<<1)|(registers.flagC?0x01:0x00));  registers.flagZ=(registers.E==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_04() { registers.flagC=((registers.H & 0x80)!=0);  registers.H=(byte)((registers.H<<1)|(registers.flagC?0x01:0x00));  registers.flagZ=(registers.H==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_05() { registers.flagC=((registers.L & 0x80)!=0);  registers.L=(byte)((registers.L<<1)|(registers.flagC?0x01:0x00));  registers.flagZ=(registers.L==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_06() { registers.flagC=((memory.Read(registers.HL) & 0x80)!=0);  memory.Write(registers.HL, (byte)((memory.Read(registers.HL)<<1)|(registers.flagC?0x01:0x00)));  registers.flagZ=(memory.Read(registers.HL)==0);  registers.flagH=false;  registers.flagN=false; }


		//rl
		void CB_17() { bool flagC=registers.flagC; registers.flagC=((registers.A>>7)!= 0);  registers.A=(byte)((registers.A << 1)|(flagC?0x01:0x00)); registers.flagZ=(registers.A==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_10() { bool flagC=registers.flagC; registers.flagC=((registers.B>>7)!= 0);  registers.B=(byte)((registers.B << 1)|(flagC?0x01:0x00)); registers.flagZ=(registers.B==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_11() { bool flagC=registers.flagC; registers.flagC=((registers.C>>7)!= 0);  registers.C=(byte)((registers.C << 1)|(flagC?0x01:0x00)); registers.flagZ=(registers.C==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_12() { bool flagC=registers.flagC; registers.flagC=((registers.D>>7)!= 0);  registers.D=(byte)((registers.D << 1)|(flagC?0x01:0x00)); registers.flagZ=(registers.D==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_13() { bool flagC=registers.flagC; registers.flagC=((registers.E>>7)!= 0);  registers.E=(byte)((registers.E << 1)|(flagC?0x01:0x00)); registers.flagZ=(registers.E==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_14() { bool flagC=registers.flagC; registers.flagC=((registers.H>>7)!= 0);  registers.H=(byte)((registers.H << 1)|(flagC?0x01:0x00)); registers.flagZ=(registers.H==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_15() { bool flagC=registers.flagC; registers.flagC=((registers.L>>7)!= 0);  registers.L=(byte)((registers.L << 1)|(flagC?0x01:0x00)); registers.flagZ=(registers.L==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_16() { bool flagC=registers.flagC; registers.flagC=((memory.Read(registers.HL)>>7)!= 0);  memory.Write(registers.HL, (byte)((memory.Read(registers.HL) << 1)|(flagC?0x01:0x00))); registers.flagZ=(memory.Read(registers.HL)==0);  registers.flagH=false;  registers.flagN=false; }


		//rrc
		void CB_0F() { registers.flagC=((registers.A & 0x01)!=0);  registers.A=(byte)((registers.A>>1)|((registers.flagC?0x01:0x00)<<7));  registers.flagZ=(registers.A==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_08() { registers.flagC=((registers.B & 0x01)!=0);  registers.B=(byte)((registers.B>>1)|((registers.flagC?0x01:0x00)<<7));  registers.flagZ=(registers.B==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_09() { registers.flagC=((registers.C & 0x01)!=0);  registers.C=(byte)((registers.C>>1)|((registers.flagC?0x01:0x00)<<7));  registers.flagZ=(registers.C==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_0A() { registers.flagC=((registers.D & 0x01)!=0);  registers.D=(byte)((registers.D>>1)|((registers.flagC?0x01:0x00)<<7));  registers.flagZ=(registers.D==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_0B() { registers.flagC=((registers.E & 0x01)!=0);  registers.E=(byte)((registers.E>>1)|((registers.flagC?0x01:0x00)<<7));  registers.flagZ=(registers.E==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_0C() { registers.flagC=((registers.H & 0x01)!=0);  registers.H=(byte)((registers.H>>1)|((registers.flagC?0x01:0x00)<<7));  registers.flagZ=(registers.H==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_0D() { registers.flagC=((registers.L & 0x01)!=0);  registers.L=(byte)((registers.L>>1)|((registers.flagC?0x01:0x00)<<7));  registers.flagZ=(registers.L==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_0E() { registers.flagC=((memory.Read(registers.HL) & 0x01)!=0);  memory.Write(registers.HL, (byte)((memory.Read(registers.HL)>>1)|((registers.flagC?0x01:0x00)<<7)));  registers.flagZ=(memory.Read(registers.HL)==0);  registers.flagH=false;  registers.flagN=false; }


		//rr
		void CB_1F() { bool flagC=registers.flagC; registers.flagC=((registers.A&0x01)!=0);  registers.A=(byte)((registers.A>>1)|(flagC?0x80:0x00)); registers.flagZ=(registers.A==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_18() { bool flagC=registers.flagC; registers.flagC=((registers.B&0x01)!=0);  registers.B=(byte)((registers.B>>1)|(flagC?0x80:0x00)); registers.flagZ=(registers.B==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_19() { bool flagC=registers.flagC; registers.flagC=((registers.C&0x01)!=0);  registers.C=(byte)((registers.C>>1)|(flagC?0x80:0x00)); registers.flagZ=(registers.C==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_1A() { bool flagC=registers.flagC; registers.flagC=((registers.D&0x01)!=0);  registers.D=(byte)((registers.D>>1)|(flagC?0x80:0x00)); registers.flagZ=(registers.D==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_1B() { bool flagC=registers.flagC; registers.flagC=((registers.E&0x01)!=0);  registers.E=(byte)((registers.E>>1)|(flagC?0x80:0x00)); registers.flagZ=(registers.E==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_1C() { bool flagC=registers.flagC; registers.flagC=((registers.H&0x01)!=0);  registers.H=(byte)((registers.H>>1)|(flagC?0x80:0x00)); registers.flagZ=(registers.H==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_1D() { bool flagC=registers.flagC; registers.flagC=((registers.L&0x01)!=0);  registers.L=(byte)((registers.L>>1)|(flagC?0x80:0x00)); registers.flagZ=(registers.L==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_1E() { bool flagC=registers.flagC; registers.flagC=((memory.Read(registers.HL)&0x01)!=0);  memory.Write(registers.HL,(byte)((memory.Read(registers.HL)>>1)|(flagC?0x80:0x00))); registers.flagZ=(memory.Read(registers.HL)==0);  registers.flagH=false;  registers.flagN=false; }

		//sla
		void CB_27() { registers.flagC=((registers.A & 0x80)!=0);  registers.A=(byte)(registers.A<<1);  registers.flagZ=(registers.A==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_20() { registers.flagC=((registers.B & 0x80)!=0);  registers.B=(byte)(registers.B<<1);  registers.flagZ=(registers.B==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_21() { registers.flagC=((registers.C & 0x80)!=0);  registers.C=(byte)(registers.C<<1);  registers.flagZ=(registers.C==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_22() { registers.flagC=((registers.D & 0x80)!=0);  registers.D=(byte)(registers.D<<1);  registers.flagZ=(registers.D==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_23() { registers.flagC=((registers.E & 0x80)!=0);  registers.E=(byte)(registers.E<<1);  registers.flagZ=(registers.E==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_24() { registers.flagC=((registers.H & 0x80)!=0);  registers.H=(byte)(registers.H<<1);  registers.flagZ=(registers.H==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_25() { registers.flagC=((registers.L & 0x80)!=0);  registers.L=(byte)(registers.L<<1);  registers.flagZ=(registers.L==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_26() { registers.flagC=((memory.Read(registers.HL) & 0x80)!=0);  memory.Write(registers.HL,(byte)(memory.Read(registers.HL)<<1));  registers.flagZ=(memory.Read(registers.HL)==0);  registers.flagH=false;  registers.flagN=false; }


		//sra
		void CB_2F() { byte tmp=(byte)(registers.A&0x80); registers.flagC=((registers.A&0x01)!=0);  registers.A=(byte)((registers.A>>1)+tmp);  registers.flagZ=(registers.A==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_28() { byte tmp=(byte)(registers.B&0x80); registers.flagC=((registers.B&0x01)!=0);  registers.B=(byte)((registers.B>>1)+tmp);  registers.flagZ=(registers.B==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_29() { byte tmp=(byte)(registers.C&0x80); registers.flagC=((registers.C&0x01)!=0);  registers.C=(byte)((registers.C>>1)+tmp);  registers.flagZ=(registers.C==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_2A() { byte tmp=(byte)(registers.D&0x80); registers.flagC=((registers.D&0x01)!=0);  registers.D=(byte)((registers.D>>1)+tmp);  registers.flagZ=(registers.D==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_2B() { byte tmp=(byte)(registers.E&0x80); registers.flagC=((registers.E&0x01)!=0);  registers.E=(byte)((registers.E>>1)+tmp);  registers.flagZ=(registers.E==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_2C() { byte tmp=(byte)(registers.H&0x80); registers.flagC=((registers.H&0x01)!=0);  registers.H=(byte)((registers.H>>1)+tmp);  registers.flagZ=(registers.H==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_2D() { byte tmp=(byte)(registers.L&0x80); registers.flagC=((registers.L&0x01)!=0);  registers.L=(byte)((registers.L>>1)+tmp);  registers.flagZ=(registers.L==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_2E() { byte tmp=(byte)(memory.Read(registers.HL)&0x80); registers.flagC=((memory.Read(registers.HL)&0x01)!=0);  memory.Write(registers.HL,(byte)((memory.Read(registers.HL)>>1)+tmp));  registers.flagZ=(memory.Read(registers.HL)==0);  registers.flagH=false;  registers.flagN=false; }


		//srl
		void CB_3F() { registers.flagC=((registers.A&0x01)!=0);  registers.A=(byte)(registers.A>>1);  registers.flagZ=(registers.A==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_38() { registers.flagC=((registers.B&0x01)!=0);  registers.B=(byte)(registers.B>>1);  registers.flagZ=(registers.B==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_39() { registers.flagC=((registers.C&0x01)!=0);  registers.C=(byte)(registers.C>>1);  registers.flagZ=(registers.C==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_3A() { registers.flagC=((registers.D&0x01)!=0);  registers.D=(byte)(registers.D>>1);  registers.flagZ=(registers.D==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_3B() { registers.flagC=((registers.E&0x01)!=0);  registers.E=(byte)(registers.E>>1);  registers.flagZ=(registers.E==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_3C() { registers.flagC=((registers.H&0x01)!=0);  registers.H=(byte)(registers.H>>1);  registers.flagZ=(registers.H==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_3D() { registers.flagC=((registers.L&0x01)!=0);  registers.L=(byte)(registers.L>>1);  registers.flagZ=(registers.L==0);  registers.flagH=false;  registers.flagN=false; }
		void CB_3E() { registers.flagC=((memory.Read(registers.HL)&0x01)!=0);  memory.Write(registers.HL, (byte)(memory.Read(registers.HL)>>1));  registers.flagZ=(memory.Read(registers.HL)==0);  registers.flagH=false;  registers.flagN=false; }

		//bit-n-r
		//bit A
		void CB_47() { registers.flagZ=((registers.A&0x01)==0); registers.flagN=false; registers.flagH=true; }
		void CB_4F() { registers.flagZ=((registers.A&0x02)==0); registers.flagN=false; registers.flagH=true; }
		void CB_57() { registers.flagZ=((registers.A&0x04)==0); registers.flagN=false; registers.flagH=true; }
		void CB_5F() { registers.flagZ=((registers.A&0x08)==0); registers.flagN=false; registers.flagH=true; }
		void CB_67() { registers.flagZ=((registers.A&0x10)==0); registers.flagN=false; registers.flagH=true; }
		void CB_6F() { registers.flagZ=((registers.A&0x20)==0); registers.flagN=false; registers.flagH=true; }
		void CB_77() { registers.flagZ=((registers.A&0x40)==0); registers.flagN=false; registers.flagH=true; }
		void CB_7F() { registers.flagZ=((registers.A&0x80)==0); registers.flagN=false; registers.flagH=true; }

		//bit B
		void CB_40() { registers.flagZ=((registers.B&0x01)==0); registers.flagN=false; registers.flagH=true; }
		void CB_48() { registers.flagZ=((registers.B&0x02)==0); registers.flagN=false; registers.flagH=true; }
		void CB_50() { registers.flagZ=((registers.B&0x04)==0); registers.flagN=false; registers.flagH=true; }
		void CB_58() { registers.flagZ=((registers.B&0x08)==0); registers.flagN=false; registers.flagH=true; }
		void CB_60() { registers.flagZ=((registers.B&0x10)==0); registers.flagN=false; registers.flagH=true; }
		void CB_68() { registers.flagZ=((registers.B&0x20)==0); registers.flagN=false; registers.flagH=true; }
		void CB_70() { registers.flagZ=((registers.B&0x40)==0); registers.flagN=false; registers.flagH=true; }
		void CB_78() { registers.flagZ=((registers.B&0x80)==0); registers.flagN=false; registers.flagH=true; }

		//bit C
		void CB_41() { registers.flagZ=((registers.C&0x01)==0); registers.flagN=false; registers.flagH=true; }
		void CB_49() { registers.flagZ=((registers.C&0x02)==0); registers.flagN=false; registers.flagH=true; }
		void CB_51() { registers.flagZ=((registers.C&0x04)==0); registers.flagN=false; registers.flagH=true; }
		void CB_59() { registers.flagZ=((registers.C&0x08)==0); registers.flagN=false; registers.flagH=true; }
		void CB_61() { registers.flagZ=((registers.C&0x10)==0); registers.flagN=false; registers.flagH=true; }
		void CB_69() { registers.flagZ=((registers.C&0x20)==0); registers.flagN=false; registers.flagH=true; }
		void CB_71() { registers.flagZ=((registers.C&0x40)==0); registers.flagN=false; registers.flagH=true; }
		void CB_79() { registers.flagZ=((registers.C&0x80)==0); registers.flagN=false; registers.flagH=true; }

		//bit D
		void CB_42() { registers.flagZ=((registers.D&0x01)==0); registers.flagN=false; registers.flagH=true; }
		void CB_4A() { registers.flagZ=((registers.D&0x02)==0); registers.flagN=false; registers.flagH=true; }
		void CB_52() { registers.flagZ=((registers.D&0x04)==0); registers.flagN=false; registers.flagH=true; }
		void CB_5A() { registers.flagZ=((registers.D&0x08)==0); registers.flagN=false; registers.flagH=true; }
		void CB_62() { registers.flagZ=((registers.D&0x10)==0); registers.flagN=false; registers.flagH=true; }
		void CB_6A() { registers.flagZ=((registers.D&0x20)==0); registers.flagN=false; registers.flagH=true; }
		void CB_72() { registers.flagZ=((registers.D&0x40)==0); registers.flagN=false; registers.flagH=true; }
		void CB_7A() { registers.flagZ=((registers.D&0x80)==0); registers.flagN=false; registers.flagH=true; }

		//bit E
		void CB_43() { registers.flagZ=((registers.E&0x01)==0); registers.flagN=false; registers.flagH=true; }
		void CB_4B() { registers.flagZ=((registers.E&0x02)==0); registers.flagN=false; registers.flagH=true; }
		void CB_53() { registers.flagZ=((registers.E&0x04)==0); registers.flagN=false; registers.flagH=true; }
		void CB_5B() { registers.flagZ=((registers.E&0x08)==0); registers.flagN=false; registers.flagH=true; }
		void CB_63() { registers.flagZ=((registers.E&0x10)==0); registers.flagN=false; registers.flagH=true; }
		void CB_6B() { registers.flagZ=((registers.E&0x20)==0); registers.flagN=false; registers.flagH=true; }
		void CB_73() { registers.flagZ=((registers.E&0x40)==0); registers.flagN=false; registers.flagH=true; }
		void CB_7B() { registers.flagZ=((registers.E&0x80)==0); registers.flagN=false; registers.flagH=true; }

		//bit H
		void CB_44() { registers.flagZ=((registers.H&0x01)==0); registers.flagN=false; registers.flagH=true; }
		void CB_4C() { registers.flagZ=((registers.H&0x02)==0); registers.flagN=false; registers.flagH=true; }
		void CB_54() { registers.flagZ=((registers.H&0x04)==0); registers.flagN=false; registers.flagH=true; }
		void CB_5C() { registers.flagZ=((registers.H&0x08)==0); registers.flagN=false; registers.flagH=true; }
		void CB_64() { registers.flagZ=((registers.H&0x10)==0); registers.flagN=false; registers.flagH=true; }
		void CB_6C() { registers.flagZ=((registers.H&0x20)==0); registers.flagN=false; registers.flagH=true; }
		void CB_74() { registers.flagZ=((registers.H&0x40)==0); registers.flagN=false; registers.flagH=true; }
		void CB_7C() { registers.flagZ=((registers.H&0x80)==0); registers.flagN=false; registers.flagH=true; }

		//bit L
		void CB_45() { registers.flagZ=((registers.L&0x01)==0); registers.flagN=false; registers.flagH=true; }
		void CB_4D() { registers.flagZ=((registers.L&0x02)==0); registers.flagN=false; registers.flagH=true; }
		void CB_55() { registers.flagZ=((registers.L&0x04)==0); registers.flagN=false; registers.flagH=true; }
		void CB_5D() { registers.flagZ=((registers.L&0x08)==0); registers.flagN=false; registers.flagH=true; }
		void CB_65() { registers.flagZ=((registers.L&0x10)==0); registers.flagN=false; registers.flagH=true; }
		void CB_6D() { registers.flagZ=((registers.L&0x20)==0); registers.flagN=false; registers.flagH=true; }
		void CB_75() { registers.flagZ=((registers.L&0x40)==0); registers.flagN=false; registers.flagH=true; }
		void CB_7D() { registers.flagZ=((registers.L&0x80)==0); registers.flagN=false; registers.flagH=true; }

		//bit (HL)
		void CB_46() { registers.flagZ=((memory.Read(registers.HL)&0x01)==0); registers.flagN=false; registers.flagH=true; }
		void CB_4E() { registers.flagZ=((memory.Read(registers.HL)&0x02)==0); registers.flagN=false; registers.flagH=true; }
		void CB_56() { registers.flagZ=((memory.Read(registers.HL)&0x04)==0); registers.flagN=false; registers.flagH=true; }
		void CB_5E() { registers.flagZ=((memory.Read(registers.HL)&0x08)==0); registers.flagN=false; registers.flagH=true; }
		void CB_66() { registers.flagZ=((memory.Read(registers.HL)&0x10)==0); registers.flagN=false; registers.flagH=true; }
		void CB_6E() { registers.flagZ=((memory.Read(registers.HL)&0x20)==0); registers.flagN=false; registers.flagH=true; }
		void CB_76() { registers.flagZ=((memory.Read(registers.HL)&0x40)==0); registers.flagN=false; registers.flagH=true; }
		void CB_7E() { registers.flagZ=((memory.Read(registers.HL)&0x80)==0); registers.flagN=false; registers.flagH=true; }

		//set A
		void CB_C7() { registers.A=(byte)(registers.A|0x01); }
		void CB_CF() { registers.A=(byte)(registers.A|0x02); }
		void CB_D7() { registers.A=(byte)(registers.A|0x04); }
		void CB_DF() { registers.A=(byte)(registers.A|0x08); }
		void CB_E7() { registers.A=(byte)(registers.A|0x10); }
		void CB_EF() { registers.A=(byte)(registers.A|0x20); }
		void CB_F7() { registers.A=(byte)(registers.A|0x40); }
		void CB_FF() { registers.A=(byte)(registers.A|0x80); }

		//set B
		void CB_C0() { registers.B=(byte)(registers.B|0x01); }
		void CB_C8() { registers.B=(byte)(registers.B|0x02); }
		void CB_D0() { registers.B=(byte)(registers.B|0x04); }
		void CB_D8() { registers.B=(byte)(registers.B|0x08); }
		void CB_E0() { registers.B=(byte)(registers.B|0x10); }
		void CB_E8() { registers.B=(byte)(registers.B|0x20); }
		void CB_F0() { registers.B=(byte)(registers.B|0x40); }
		void CB_F8() { registers.B=(byte)(registers.B|0x80); }

		//set C
		void CB_C1() { registers.C=(byte)(registers.C|0x01); }
		void CB_C9() { registers.C=(byte)(registers.C|0x02); }
		void CB_D1() { registers.C=(byte)(registers.C|0x04); }
		void CB_D9() { registers.C=(byte)(registers.C|0x08); }
		void CB_E1() { registers.C=(byte)(registers.C|0x10); }
		void CB_E9() { registers.C=(byte)(registers.C|0x20); }
		void CB_F1() { registers.C=(byte)(registers.C|0x40); }
		void CB_F9() { registers.C=(byte)(registers.C|0x80); }

		//set D
		void CB_C2() { registers.D=(byte)(registers.D|0x01); }
		void CB_CA() { registers.D=(byte)(registers.D|0x02); }
		void CB_D2() { registers.D=(byte)(registers.D|0x04); }
		void CB_DA() { registers.D=(byte)(registers.D|0x08); }
		void CB_E2() { registers.D=(byte)(registers.D|0x10); }
		void CB_EA() { registers.D=(byte)(registers.D|0x20); }
		void CB_F2() { registers.D=(byte)(registers.D|0x40); }
		void CB_FA() { registers.D=(byte)(registers.D|0x80); }

		//set E
		void CB_C3() { registers.E=(byte)(registers.E|0x01); }
		void CB_CB() { registers.E=(byte)(registers.E|0x02); }
		void CB_D3() { registers.E=(byte)(registers.E|0x04); }
		void CB_DB() { registers.E=(byte)(registers.E|0x08); }
		void CB_E3() { registers.E=(byte)(registers.E|0x10); }
		void CB_EB() { registers.E=(byte)(registers.E|0x20); }
		void CB_F3() { registers.E=(byte)(registers.E|0x40); }
		void CB_FB() { registers.E=(byte)(registers.E|0x80); }

		//set H
		void CB_C4() { registers.H=(byte)(registers.H|0x01); }
		void CB_CC() { registers.H=(byte)(registers.H|0x02); }
		void CB_D4() { registers.H=(byte)(registers.H|0x04); }
		void CB_DC() { registers.H=(byte)(registers.H|0x08); }
		void CB_E4() { registers.H=(byte)(registers.H|0x10); }
		void CB_EC() { registers.H=(byte)(registers.H|0x20); }
		void CB_F4() { registers.H=(byte)(registers.H|0x40); }
		void CB_FC() { registers.H=(byte)(registers.H|0x80); }

		//set L
		void CB_C5() { registers.L=(byte)(registers.L|0x01); }
		void CB_CD() { registers.L=(byte)(registers.L|0x02); }
		void CB_D5() { registers.L=(byte)(registers.L|0x04); }
		void CB_DD() { registers.L=(byte)(registers.L|0x08); }
		void CB_E5() { registers.L=(byte)(registers.L|0x10); }
		void CB_ED() { registers.L=(byte)(registers.L|0x20); }
		void CB_F5() { registers.L=(byte)(registers.L|0x40); }
		void CB_FD() { registers.L=(byte)(registers.L|0x80); }

		//set (HL)
		void CB_C6() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)|0x01)); }
		void CB_CE() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)|0x02)); }
		void CB_D6() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)|0x04)); }
		void CB_DE() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)|0x08)); }
		void CB_E6() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)|0x10)); }
		void CB_EE() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)|0x20)); }
		void CB_F6() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)|0x40)); }
		void CB_FE() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)|0x80)); }

		//res A
		void CB_87() { registers.A=(byte)(registers.A&0xFE); }
		void CB_8F() { registers.A=(byte)(registers.A&0xFD); }
		void CB_97() { registers.A=(byte)(registers.A&0xFB); }
		void CB_9F() { registers.A=(byte)(registers.A&0xF7); }
		void CB_A7() { registers.A=(byte)(registers.A&0xEF); }
		void CB_AF() { registers.A=(byte)(registers.A&0xDF); }
		void CB_B7() { registers.A=(byte)(registers.A&0xBF); }
		void CB_BF() { registers.A=(byte)(registers.A&0x7F); }

		//res B
		void CB_80() { registers.B=(byte)(registers.B&0xFE); }
		void CB_88() { registers.B=(byte)(registers.B&0xFD); }
		void CB_90() { registers.B=(byte)(registers.B&0xFB); }
		void CB_98() { registers.B=(byte)(registers.B&0xF7); }
		void CB_A0() { registers.B=(byte)(registers.B&0xEF); }
		void CB_A8() { registers.B=(byte)(registers.B&0xDF); }
		void CB_B0() { registers.B=(byte)(registers.B&0xBF); }
		void CB_B8() { registers.B=(byte)(registers.B&0x7F); }

		//res C
		void CB_81() { registers.C=(byte)(registers.C&0xFE); }
		void CB_89() { registers.C=(byte)(registers.C&0xFD); }
		void CB_91() { registers.C=(byte)(registers.C&0xFB); }
		void CB_99() { registers.C=(byte)(registers.C&0xF7); }
		void CB_A1() { registers.C=(byte)(registers.C&0xEF); }
		void CB_A9() { registers.C=(byte)(registers.C&0xDF); }
		void CB_B1() { registers.C=(byte)(registers.C&0xBF); }
		void CB_B9() { registers.C=(byte)(registers.C&0x7F); }

		//res D
		void CB_82() { registers.D=(byte)(registers.D&0xFE); }
		void CB_8A() { registers.D=(byte)(registers.D&0xFD); }
		void CB_92() { registers.D=(byte)(registers.D&0xFB); }
		void CB_9A() { registers.D=(byte)(registers.D&0xF7); }
		void CB_A2() { registers.D=(byte)(registers.D&0xEF); }
		void CB_AA() { registers.D=(byte)(registers.D&0xDF); }
		void CB_B2() { registers.D=(byte)(registers.D&0xBF); }
		void CB_BA() { registers.D=(byte)(registers.D&0x7F); }

		//res E
		void CB_83() { registers.E=(byte)(registers.E&0xFE); }
		void CB_8B() { registers.E=(byte)(registers.E&0xFD); }
		void CB_93() { registers.E=(byte)(registers.E&0xFB); }
		void CB_9B() { registers.E=(byte)(registers.E&0xF7); }
		void CB_A3() { registers.E=(byte)(registers.E&0xEF); }
		void CB_AB() { registers.E=(byte)(registers.E&0xDF); }
		void CB_B3() { registers.E=(byte)(registers.E&0xBF); }
		void CB_BB() { registers.E=(byte)(registers.E&0x7F); }

		//res H
		void CB_84() { registers.H=(byte)(registers.H&0xFE); }
		void CB_8C() { registers.H=(byte)(registers.H&0xFD); }
		void CB_94() { registers.H=(byte)(registers.H&0xFB); }
		void CB_9C() { registers.H=(byte)(registers.H&0xF7); }
		void CB_A4() { registers.H=(byte)(registers.H&0xEF); }
		void CB_AC() { registers.H=(byte)(registers.H&0xDF); }
		void CB_B4() { registers.H=(byte)(registers.H&0xBF); }
		void CB_BC() { registers.H=(byte)(registers.H&0x7F); }

		//res L
		void CB_85() { registers.L=(byte)(registers.L&0xFE); }
		void CB_8D() { registers.L=(byte)(registers.L&0xFD); }
		void CB_95() { registers.L=(byte)(registers.L&0xFB); }
		void CB_9D() { registers.L=(byte)(registers.L&0xF7); }
		void CB_A5() { registers.L=(byte)(registers.L&0xEF); }
		void CB_AD() { registers.L=(byte)(registers.L&0xDF); }
		void CB_B5() { registers.L=(byte)(registers.L&0xBF); }
		void CB_BD() { registers.L=(byte)(registers.L&0x7F); }

		//res (HL)
		void CB_86() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)&0xFE)); }
		void CB_8E() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)&0xFD)); }
		void CB_96() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)&0xFB)); }
		void CB_9E() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)&0xF7)); }
		void CB_A6() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)&0xEF)); }
		void CB_AE() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)&0xDF)); }
		void CB_B6() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)&0xBF)); }
		void CB_BE() { memory.Write(registers.HL,(byte)(memory.Read(registers.HL)&0x7F)); }


		#endregion





		#region Helpers & others

		int DecodeSigned(byte b)
		{
			int result = (int)b;
			if (b > 127) {
				result -= 0x100;
			}
			return result;
		}


		//https://www.reddit.com/r/EmuDev/comments/4ycoix/a_guide_to_the_gameboys_halfcarry_flag/
		bool CheckHFlag(ushort a, ushort b, bool isSubstraction = false, bool is16bit = false)
		{
			var result = false;
			if (!isSubstraction) {
				if (is16bit) {
					result = ((a & 0xFFF) + (b & 0xFFF)) > 0xFFF;
				} 
				else {
					result = ((a & 0xF) + (b & 0xF)) > 0xF;
				}
			} else {
				if (is16bit) {
					result = ((a & 0xFFF) - (b & 0xFFF)) < 0;
				} else {
					result = ((a & 0xF) - (b & 0xF)) < 0;
				}
			}
			return result;
		}


		void OP_XX() {
			Debug.LogError(string.Format("Invalid operation received: {0}", registers.PC));
			Stop = true;
		}

		#endregion
	}


}
