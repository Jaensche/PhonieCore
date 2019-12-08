using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Iot.Device.Rfid.CreditCardProcessing
{
    public class BerSplitter
    {
        public List<BerElement> BerElements { get; set; }

        public BerSplitter(Span<byte> toSplit)
        {
            BerElements = new List<BerElement>();
            int index = 0;
            while ((index < toSplit.Length) && (toSplit[index] != 0x00))
            {
                var elem = new BerElement();
                var resTag = DecodeTag(toSplit.Slice(index));
                elem.Tag = resTag.Item1;
                // Need to move index depending on how many has been read
                index += resTag.Item2;
                var resSize = DecodeSize(toSplit.Slice(index));
                elem.Data = new byte[resSize.Item1];
                index += resSize.Item2;
                toSplit.Slice(index, resSize.Item1).CopyTo(elem.Data);
                BerElements.Add(elem);
                index += resSize.Item1;
            }
        }

        private Tuple<ushort, byte> DecodeTag(Span<byte> toSplit)
        {
            //  check if single or double element
            if ((toSplit[0] & 0b0001_1111) == 0b0001_1111)
            {
                // Double element               
                return new Tuple<ushort, byte>(BinaryPrimitives.ReadUInt16BigEndian(toSplit), 2);
            }
            return new Tuple<ushort, byte>(toSplit[0], 1);
        }

        private Tuple<int, byte> DecodeSize(Span<byte> toSplit)
        {
            // Case infinite
            if (toSplit[0] == 0b1000_0000)
                return new Tuple<int, byte>(- 1, 1);
            // Check how many bytes 
            if ((toSplit[0] & 0b1000_0000) == 0b1000_0000)
            {
                // multiple bytes
                var numBytes = toSplit[0] & 0b0111_1111;
                int size = 0;
                for (int i = 0; i < numBytes; i++)
                    size = (size << 8) + toSplit[1 + i];
                return new Tuple<int, byte>(size, (byte)(numBytes + 1));
            }
            return new Tuple<int, byte>(toSplit[0], 1);
        }
    }
}
