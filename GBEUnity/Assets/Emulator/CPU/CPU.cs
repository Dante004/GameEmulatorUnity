using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;


namespace Emulator.CPU
{
    public class CPU
    {
        private readonly Register _register;
        private readonly AluInstructions _aluInstructions;
        private readonly BitInstructions _bitInstructions;
        private readonly JumpInstructions _jumpInstructions;
        private readonly RotateInstructions _rotateInstructions;
        private readonly SaveInstructions _saveInstructions;
        private readonly WriteInstructions _writeInstructions;
        private readonly LoadInstructions _loadInstructions;

        public bool halted;
        public bool stopped;
        public TimeFreqencyType TimeFreqency;
        public int TimerCounter;
        public int TimerModulo;
        public int ticks;
        public ushort vBlankInteruptAddress = 0x0040;
        public ushort LcdcInteruptAddress = 0x0048;
        public ushort TimeOverflowInteruptAddress = 0x0050;
        public ushort SerialIOTInteruptAddress = 0x0058;
        public ushort KeyPressedInteruptAddress = 0x0060;
        public bool interruptsEnabled;
        public static bool vBlankInterruptRequested;
        public static bool vBlankInterruptEnabled;
        public static bool lcdControlOperationEnabled;
        public static bool lcdcInterruptEnabled, lcdcInterruptRequested;
        public static bool timerOverflowInterruptEnabled, timerOverflowInterruptRequested;
        public static bool serialIOTransferCompleteInterruptEnabled, serialIOTransferCompleteInterruptRequested;
        public static bool keyPressedInterruptEnabled, keyPressedInterruptRequested;
        public bool stopCounting;


        public CPU(Register register)
        {
            _register = register;
            _loadInstructions = new LoadInstructions(_register);
            _writeInstructions = new WriteInstructions(_register);
            _aluInstructions = new AluInstructions(_register);
            _bitInstructions = new BitInstructions(_register);
            _rotateInstructions = new RotateInstructions(_register);
            _saveInstructions = new SaveInstructions(_register);
            _jumpInstructions = new JumpInstructions(_register);
        }

        public void PowerUp()
        {
            _register.SetupRegister();
        }

        public void Step()
        {

        }
    }
}

