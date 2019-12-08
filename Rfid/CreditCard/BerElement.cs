using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Rfid.CreditCardProcessing
{
    public class BerElement
    {
        public ushort Tag { get; set; }
        public byte[] Data { get; set; }
    }
}
