using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Mfrc522
{
    [Flags]
    internal enum ComIEnReg
    {
        Timer = 0b0000_0001,
        Error = 0b0000_0010,
        LoAlert = 0B0000_0100,
        HiAlert = 0b0000_1000,
        Idel = 0b0001_0000,
        Rx = 0b0010_0000,
        Tx = 0b0100_0000,
        Irq = 0b1000_0000,
    }
}
