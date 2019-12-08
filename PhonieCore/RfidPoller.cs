using System;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Mfrc522;
using Iot.Device.Rfid;

namespace DotNetCoreTest
{
    public class RfidPoller
    {
        public delegate void NewCardDetectedHandler(string uid);
        public static event NewCardDetectedHandler OnNewCardDetected;

        public delegate void CardDetectedHandler(string uid);
        public static event CardDetectedHandler OnCardDetected;

        public RfidPoller(NewCardDetectedHandler onNewCardDetected, CardDetectedHandler onCardDetected)
        {
            var connection = new SpiConnectionSettings(0, 0);
            connection.ClockFrequency = 500000;

            var spi = new UnixSpiDevice(connection);
            using (var mfrc522Controller = new Mfrc522Controller(spi))
            {
                mfrc522Controller.LogLevel = LogLevel.Info;
                mfrc522Controller.LogTo = LogTo.Console;
                mfrc522Controller.RxGain = RxGain.G48dB;

                OnNewCardDetected += new NewCardDetectedHandler(onNewCardDetected);
                OnCardDetected += new CardDetectedHandler(onCardDetected);
                Task.Factory.StartNew(ReadCardUidLoop(mfrc522Controller, OnNewCardDetected, OnCardDetected));
            }

            while (true) ;
        }

        private static Action ReadCardUidLoop(Mfrc522Controller mfrc522Controller, NewCardDetectedHandler onNewCardDetected, CardDetectedHandler onCardDetected)
        {
            string lastUid = String.Empty;
            while (true)
            {
                Task.Delay(1000);
                var (status, _) = mfrc522Controller.Request(RequestMode.RequestIdle);
                if (status != Status.OK)
                    continue;

                var (status2, uidBytes) = mfrc522Controller.AntiCollision();
                string uid = BytesToString(uidBytes);

                if (uid.Length == 12)
                {
                    onCardDetected.Invoke(uid);
                    if (uid != lastUid)
                    {
                        onNewCardDetected.Invoke(uid);
                        lastUid = uid;
                    }
                }
            }
        }

        private static string BytesToString(byte[] data)
        {
            // Minimum length 1.
            if (data.Length == 0) return "0";

            // length <= digits.Length.
            var digits = new byte[(data.Length * 0x00026882/* (int)(Math.Log(2, 10) * 0x80000) */ + 0xFFFF) >> 16];
            int length = 1;

            // For each byte:
            for (int j = 0; j != data.Length; ++j)
            {
                // digits = digits * 256 + data[j].
                int i, carry = data[j];
                for (i = 0; i < length || carry != 0; ++i)
                {
                    int value = digits[i] * 256 + carry;
                    carry = Math.DivRem(value, 10, out value);
                    digits[i] = (byte)value;
                }
                // digits got longer.
                if (i > length) length = i;
            }

            // Return string.
            var result = new StringBuilder(length);
            while (0 != length) result.Append((char)('0' + digits[--length]));
            return result.ToString();
        }
    }
}
