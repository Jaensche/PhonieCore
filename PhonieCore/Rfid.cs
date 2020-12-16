using Unosquare.RaspberryIO.Peripherals;
using System.Text;
using System;
using System.Threading.Tasks;

namespace PhonieCore
{
    public class Rfid
    {
        public delegate void NewCardDetectedHandler(string uid);
        public event NewCardDetectedHandler NewCardDetected;

        public delegate void CardDetectedHandler(string uid);
        public event CardDetectedHandler CardDetected;

        public Rfid()
        {
            Task.Run(WatchRfid);
        }

        public void WatchRfid()
        {
            RFIDControllerMfrc522 _reader;
            try
            {
                _reader = new RFIDControllerMfrc522();           

                string lastUid = string.Empty;
                while (true)
                {
                    if (_reader.DetectCard() == RFIDControllerMfrc522.Status.AllOk)
                    {
                        var uidResponse = _reader.ReadCardUniqueId();
                        if (uidResponse.Status == RFIDControllerMfrc522.Status.AllOk)
                        {
                            var cardUid = uidResponse.Data;
                            _reader.SelectCardUniqueId(cardUid);

                            string currentUid = ByteArrayToString(cardUid);
                            if(currentUid != lastUid)
                            {
                                lastUid = currentUid;
                                NewCardDetected.Invoke(currentUid);
                            }
                            else
                            {
                                CardDetected.Invoke(currentUid);
                            }
                        
                            //try
                            //{
                            //    if (_reader.AuthenticateCard1A(cardUid, 7) == RFIDControllerMfrc522.Status.AllOk)
                            //    {
                            //        // Read or write data to sector
                            //    }
                            //}
                            //finally
                            //{
                            //    _reader.ClearCardSelection();
                            //}
                        }
                    }
            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
    }
}
