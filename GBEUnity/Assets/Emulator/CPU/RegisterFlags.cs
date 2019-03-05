using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.CPU
{
    [Flags]
    public enum RegisterFlags
    {
        Z = 1<<7,
        N = 1<<6,
        H = 1<<5,
        C = 1<<4,
        All = Z|N|H|C
    }
}
