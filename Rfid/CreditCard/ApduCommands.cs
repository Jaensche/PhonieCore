using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Rfid.CreditCardProcessing
{
    public class ApduCommands
    {
        static public byte[] ApplicaitonBlock = { 0x80, 0x1E };
        static public byte[] ApplicaitonUnBlock = { 0x80, 0x18 };
        static public byte[] CardBlock = { 0x80, 0x16 };
        static public byte[] ExternalAuthenticate = { 0x00, 0x82 };
        static public byte[] GenerateApplicationCryptogram = { 0x80, 0xAE };
        static public byte[] GetChallenge = { 0x00, 0x84 };
        static public byte[] GetData = { 0x80, 0xCA };
        static public byte[] GetProcessingOptions = { 0x80, 0xA8 };
        static public byte[] InternalAuthenticate = { 0x00, 0x88 };
        static public byte[] PersonalIdentificationNumberChangeUnblock = { 0x80, 0x24 };
        static public byte[] ReadRecord = { 0x00, 0xB2 };
        static public byte[] Select = { 0x00, 0xA4 };
        static public byte[] Verify = { 0x80, 0x20 };
    }
}
