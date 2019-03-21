using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.CPU
{
    [Flags]
    public enum InteruptFlags
    {
        None = 0,
        VBlank = 1<<0,
        LCDCStatus = 1<<1,
        TimeOverflow = 1<<2,
        SerialTransferCompletion = 1<<3,
        JoyPad = 1<<4
    }
}
