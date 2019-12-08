using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Mfrc522
{
    public enum RxGain
    {
        G18dBa = 0b000_0000,
        G23dBa = 0b0001_0000,
        G18dBb = 0b0010_0000,
        G23dBb = 0b0011_0000,
        G33dB = 0b0100_0000,
        G38dB = 0b0101_0000,
        G43dB = 0b0110_0000,
        G48dB = 0b0111_0000,
    }
}
