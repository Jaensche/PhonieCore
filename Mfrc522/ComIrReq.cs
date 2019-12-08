using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Mfrc522
{
    [Flags]
    internal enum ComIrReq
    {
        Timer = 0b0000_0001,
        Error = 0b0000_0010,
        LoAlert = 0b0000_0100,
        HiAlert = 0b0000_1000,
        Idlel = 0b0001_0000,
        Rx = 0b0010_0000,
        Tx = 0b0100_0000,
        SetIrq = 0b1000_0000,
        All = Timer | Error | LoAlert | HiAlert | Idlel | Rx | Tx,
        None = 0b0000_0000
    }
}
