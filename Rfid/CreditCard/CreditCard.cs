using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Iot.Device.Rfid.CreditCardProcessing
{
    public class CreditCard
    {
        private const byte Cla = 0x00;
        private const byte Ins = 0x01;
        private const byte P1 = 0x02;
        private const byte P2 = 0x03;
        private const byte Lc = 0x04;

        private const int MaxBuffer = 260;

        private RfidWriteRead _nfc;
        private byte _target;

        // This is a string "2PAY.SYS.DDF01" (PPSE) to select the root directory
        public readonly byte[] RootDirectory = { 0x32, 0x50, 0x41, 0x59, 0x2e, 0x53, 0x59, 0x53, 0x2e, 0x44, 0x44, 0x46, 0x30, 0x31 };
        public FileControlInformation FileControlInformation { get; internal set; }

        public Dictionary<byte, EmvProprietaryTemplate> EmvInformation { get; internal set; }

        public CreditCard(RfidWriteRead nfc, byte target)
        {
            _nfc = nfc;
            _target = target;
            EmvInformation = new Dictionary<byte, EmvProprietaryTemplate>();
        }

        public ErrorType ExternalAuthentication(Span<byte> issuerAuthenticationData)
        {
            if ((issuerAuthenticationData.Length < 8) || (issuerAuthenticationData.Length > 16))
                throw new ArgumentException($"{nameof(issuerAuthenticationData)} needs to be more than 8 and less than 16 length");
            Span<byte> toSend = stackalloc byte[5 + issuerAuthenticationData.Length];
            ApduCommands.ExternalAuthenticate.CopyTo(toSend);
            toSend[P1] = 0x00;
            toSend[P2] = 0x00;
            toSend[Lc] = (byte)issuerAuthenticationData.Length;
            issuerAuthenticationData.CopyTo(toSend.Slice(Lc));
            Span<byte> received = stackalloc byte[MaxBuffer];
            return RunSimpleCommand(toSend);
        }

        private ErrorType RunSimpleCommand(Span<byte> toSend)
        {
            Span<byte> received = stackalloc byte[MaxBuffer];
            var ret = _nfc.WriteRead(_target, toSend, received);
            if (ret >= 0)
            {
                var err = new ProcessError(received.Slice(0, 2));
                return err.ErrorType;
            }
            return ErrorType.Unknown;

        }

        public ErrorType GetChallenge()
        {
            Span<byte> toSend = stackalloc byte[5];
            ApduCommands.GetChallenge.CopyTo(toSend);
            toSend[P1] = 0x00;
            toSend[P2] = 0x00;
            toSend[P2 + 1] = 0x00;
            return RunSimpleCommand(toSend);
        }


        public ErrorType Select(Span<byte> toSelect)
        {
            Span<byte> toSend = stackalloc byte[6 + toSelect.Length];
            ApduCommands.Select.CopyTo(toSend);
            toSend[P1] = 0x04;
            toSend[P2] = 0x00;
            toSend[Lc] = (byte)toSelect.Length;
            toSelect.CopyTo(toSend.Slice(Lc + 1));
            toSend[toSend.Length - 1] = 0x00;
            Span<byte> received = stackalloc byte[MaxBuffer];
            var ret = _nfc.WriteRead(_target, toSend, received);
            if (ret >= 0)
            {
                if (ret == 3)
                {
                    // It's an error, process it
                    var err = new ProcessError(received.Slice(0, 2));
                    return err.ErrorType;
                }
                var fs = new FileControlInformation(received.Slice(0, ret));
                if (FileControlInformation == null)
                    FileControlInformation = fs;
                else
                {
                    FileControlInformation.Update(fs);
                }
                return ErrorType.ProcessCompletedNormal;
            }
            return ErrorType.Unknown;
        }

        public void FillCreditCardInfo()
        {
            // This is a string "2PAY.SYS.DDF01" (PPSE) to select the root directory
            var ret = Select(RootDirectory);
            if (ret == ErrorType.ProcessCompletedNormal)
            {
                var fs = FileControlInformation;
                //Console.WriteLine($"Dedicated Name: {BitConverter.ToString(fs.DedicatedFileName)}");
                foreach (var app in fs.ProprietaryTemplate.ApplicationTemplates)
                {
                    //Console.WriteLine($"APPID: {BitConverter.ToString(app.ApplicationIdentifier)}, Priority: {app.ApplicationPriorityIndicator}, {app.ApplicationLabel}");
                    ret = Select(app.ApplicationIdentifier);
                    if (ret == ErrorType.ProcessCompletedNormal)
                    {
                        Span<byte> received = stackalloc byte[260];
                        for (byte record = 1; record < 5; record++)
                        {
                            for (byte SFI = 1; SFI < 5; SFI++)
                            {
                                ret = ReadRecord(SFI, record, app.ApplicationPriorityIndicator);                                
                            }
                        }
                    }
                }
            }
        }

        public ErrorType ReadRecord(byte sfi, byte record, byte opplicationPriorityIndicator)
        {
            if (sfi > 31)
                return ErrorType.WrongParameterP1P2FunctionNotSupported;
            Span<byte> toSend = stackalloc byte[5];
            ApduCommands.ReadRecord.CopyTo(toSend);
            toSend[P1] = record;
            toSend[P2] = (byte)((sfi << 3) | (0b0000_0100));
            toSend[P2 + 1] = 0x00;
            Span<byte> received = stackalloc byte[MaxBuffer];
            var ret = _nfc.WriteRead(_target, toSend, received);
            if (ret >= 3)
            {
                if (ret == 3)
                {
                    // It's an error, process it
                    return new ProcessError(received.Slice(0, 2)).ErrorType;
                }
                var emv = new EmvProprietaryTemplate(received.Slice(0, ret - 3));
                if (EmvInformation.ContainsKey(opplicationPriorityIndicator))
                {
                    var actualEmv = EmvInformation[opplicationPriorityIndicator];
                    actualEmv.Update(emv);
                }
                else
                {
                    EmvInformation.Add(opplicationPriorityIndicator, emv);
                }

                return new ProcessError(received.Slice(ret - 3)).ErrorType;
            }
            return ErrorType.Unknown;
        }

        public ErrorType GetData(DataType dataType)
        {
            Span<byte> toSend = stackalloc byte[5];
            ApduCommands.GetData.CopyTo(toSend);
            switch (dataType)
            {
                case DataType.ApplicationTransactionCounter:
                    // 9F36
                    toSend[P1] = 0x9F;
                    toSend[P2] = 0x36;
                    break;
                case DataType.PinTryCounter:
                    // 9F17
                    toSend[P1] = 0x9F;
                    toSend[P2] = 0x17;
                    break;
                case DataType.LastOnlineAtcRegister:
                    // 9F13
                    toSend[P1] = 0x9F;
                    toSend[P2] = 0x13;
                    break;
                case DataType.LogFormat:
                    //9F4F
                    toSend[P1] = 0x9F;
                    toSend[P2] = 0x4F;
                    break;
                default:
                    break;
            }
            toSend[P2 + 1] = 0x00;
            Span<byte> received = stackalloc byte[MaxBuffer];
            var ret = _nfc.WriteRead(_target, toSend, received);
            if (ret >= 3)
            {
                if (ret == 3)
                {
                    // It's an error, process it
                    return new ProcessError(received.Slice(0, 2)).ErrorType;
                }
                Console.WriteLine($"{BitConverter.ToString(received.Slice(0, ret).ToArray())}");
                var ber = new BerSplitter(received.Slice(0, ret - 3));
                foreach (var b in ber.BerElements)
                {
                    Console.WriteLine($"DataType: {dataType}, Tag: {b.Tag.ToString("X4")}, Data: {BitConverter.ToString(b.Data)}");
                }
                return new ProcessError(received.Slice(ret - 3)).ErrorType;
            }
            return ErrorType.Unknown;
        }

    }
}
