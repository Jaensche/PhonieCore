// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Device.Gpio;
using System.Device.Spi;
using System.Collections.Generic;

using static Iot.Device.Mfrc522.Status;
using static Iot.Device.Mfrc522.Register;
using static Iot.Device.Mfrc522.RequestMode;
using static Iot.Device.Mfrc522.Command;
using Iot.Device.Rfid;

namespace Iot.Device.Mfrc522
{
    public class Mfrc522Controller : RfidWriteRead, IDisposable
    {
        const byte NRSTPD = 22;

        const byte MAX_LEN = 16;

        private readonly SpiDevice _spiDevice;
        private GpioController _controller;

        public LogLevel LogLevel { get { return LogInfo.LogLevel; } set { LogInfo.LogLevel = value; } }
        public LogTo LogTo { get { return LogInfo.LogTo; } set { LogInfo.LogTo = value; } }

        public int PinReset { get; internal set; }

        public RxGain RxGain
        {
            get
            {
                return (RxGain)(ReadSpi(RFCfgReg) & (byte)RxGain.G48dB);
            }

            set
            {
                WriteSpi(RFCfgReg, (byte)value);
            }
        }

        public Mfrc522Controller(SpiDevice spiDevice, int pinReset = -1)
        {
            _spiDevice = spiDevice;

            _controller = new GpioController();

            if (pinReset >= 0)
                PinReset = pinReset;
            else
                PinReset = NRSTPD;
            _controller.OpenPin(PinReset, PinMode.Output);
            _controller.Write(PinReset, PinValue.High);

            Init();
        }

        public static readonly byte[] DefaultAuthKey = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        public Version Version
        {
            get
            {
                Version version;
                var rev = ReadSpi(Register.VersionReg);
                if (rev == 0x91)
                    version = new Version(1, 0);
                else if (rev == 0x92)
                    version = new Version(2, 0);
                else
                    version = new Version();
                return version; ;

            }
        }

        private void Init()
        {
            _controller.Write(PinReset, PinValue.High);

            SoftReset();

            // Reset baud rates
            WriteSpi(TxModeReg, (byte)0x00);
            WriteSpi(RxModeReg, (byte)0x00);
            // Reset ModWidthReg
            WriteSpi(ModWidthReg, 0x26);

            WriteSpi(TModeReg, 0x8D);
            WriteSpi(TPrescalerReg, 0x3E);
            WriteSpi(TReloadRegL, 30);
            WriteSpi(TReloadRegH, (byte)0);

            WriteSpi(TxAutoReg, 0x40);
            WriteSpi(ModeReg, 0x3D);

            AntennaOn();
        }

        private void SoftReset()
        {
            WriteSpi(CommandReg, (byte)ResetPhase);
        }

        private void WriteSpi(byte address, byte value)
        {
            Span<byte> buffer = stackalloc byte[2] {
                (byte)((address << 1) & 0x7E),
                value
            };
            _spiDevice.Write(buffer);
        }
        private void WriteSpi(Register register, byte value)
        {
            WriteSpi((byte)register, value);
        }

        private void WriteSpi(Register register, Command command)
        {
            WriteSpi((byte)register, (byte)command);
        }

        private byte ReadSpi(byte address)
        {
            Span<byte> buffer = stackalloc byte[2] {
                (byte)(((address << 1) & 0x7E) | 0x80),
                0
            };
            _spiDevice.TransferFullDuplex(buffer, buffer);
            return buffer[1];
        }

        private byte ReadSpi(Register register)
        {
            return ReadSpi((byte)register);
        }

        public (Status status, byte[] data) ReadCardData(byte blockAddress)
        {
            List<byte> buff = new List<byte>
            {
              (byte)RequestMode.Read,
              blockAddress
            };
            var crc = CalulateCRC(buff.ToArray());
            buff.Add(crc[0]);
            buff.Add(crc[1]);

            var (status, data, _) = SendCommand(Command.Transceive, buff.ToArray());
            return (status, data);
        }

        private void SetBitMask(Register register, byte mask)
        {
            var tmp = ReadSpi(register);
            WriteSpi(register, (byte)(tmp | mask));
        }

        private void ClearBitMask(Register register, byte mask)
        {
            var tmp = ReadSpi(register);
            WriteSpi(register, (byte)(tmp & (~mask)));
        }

        private void AntennaOn()
        {
            var temp = ReadSpi(TxControlReg);
            SetBitMask(TxControlReg, 0x03);
        }

        private void AntennaOff()
        {
            ClearBitMask(TxControlReg, 0x03);
        }

        public byte[] ListPassiveTarget()
        {
            var (status, atqa) = Request(RequestMode.RequestIdle);
            byte[] uid;
            byte sak;
            byte[] targetInfo = new byte[2 + 4 + 1];
            if (status == Status.OK)
            {

                (status, uid) = AntiCollision();
                if (status == Status.OK)
                {
                    // Try up to 32 times to select the tag
                    int idx = 32;
                    while (idx > 0)
                    {
                        (status, sak) = SelectTag(uid);
                        if (status == Status.OK)
                        {
                            atqa.CopyTo(targetInfo, 0);
                            targetInfo[2] = sak;
                            uid.AsSpan().Slice(0, 4).CopyTo(targetInfo.AsSpan().Slice(3));
                            return targetInfo;
                        }
                        idx--;
                    }
                }
            }
            return null;
        }

        private (Status status, byte[] backData, byte backLen) SendCommand(Command command, byte[] sendData)
        {
            const long TimeOutMilliseconds = 36;
            var backData = new List<byte>();
            byte backLen = 0;
            var status = Error;
            byte irqEn = 0x00;
            byte waitIRq = 0x00;
            byte n = 0;
            var i = 0;

            //LogInfo.Log($"{nameof(SendCommand)} - {command} {BitConverter.ToString(sendData)}", LogLevel.Debug);
            WriteSpi(CommandReg, Idle);

            switch (command)
            {
                case Command.Authenticate:
                    irqEn = (byte)(ComIEnReg.Idel | ComIEnReg.Error); //  0x12;
                    waitIRq = (byte)(ComIrReq.Idlel); //0x10;
                    break;

                case Command.Transceive:
                    irqEn = (byte)(ComIEnReg.Tx | ComIEnReg.Rx | ComIEnReg.Idel | ComIEnReg.Timer | ComIEnReg.LoAlert | ComIEnReg.Error); //0x77;
                    waitIRq = (byte)(ComIrReq.Idlel | ComIrReq.Rx); //0x30;
                    break;
            }
            WriteSpi(CommIrqReg, (byte)ComIrReq.All); //0x7F);
            WriteSpi(FIFOLevelReg, 0x80);
            WriteSpi(CommIEnReg, (byte)(irqEn | 0x80));

            while (i < sendData.Length)
            {
                WriteSpi(FIFODataReg, sendData[i]);
                i++;
            }

            WriteSpi(CommandReg, (byte)command);

            if (command == Transceive)
            {
                SetBitMask(BitFramingReg, 0x80);                
            }            

            Stopwatch stopwatch = new Stopwatch();
            var initial = stopwatch.ElapsedMilliseconds + TimeOutMilliseconds;
            long current;
            stopwatch.Start();
            do
            {
                n = ReadSpi((byte)CommIrqReg);
                current = stopwatch.ElapsedMilliseconds;
                if ((n & (byte)ComIrReq.Timer) == (byte)ComIrReq.Timer)
                {
                    LogInfo.Log($"{nameof(SendCommand)} - Timeout by timer", LogLevel.Debug);
                    return (status, backData.ToArray(), backLen);
                }
            }
            while ((current < initial) && (n & waitIRq) == (byte)ComIrReq.None);
            stopwatch.Stop();
            //ClearBitMask(BitFramingReg, 0x80);

            //if (i == 0)
            if (current > initial)
            {
                LogInfo.Log($"{nameof(SendCommand)} - Timeout by software", LogLevel.Debug);
                return (status, backData.ToArray(), backLen);
            }

            if ((ReadSpi(ErrorReg) & 0x1B) == 0x00)
            {
                status = OK;

                if (Convert.ToBoolean(n & irqEn & 0x01))
                {
                    status = NoTag;
                }

                if (command == Transceive)
                {
                    n = ReadSpi(FIFOLevelReg);
                    byte? lastBits = (byte)(ReadSpi(ControlReg) & 0x07);
                    if (lastBits != 0)
                    {
                        backLen = (byte)(((n - 1) * 8) + (byte)lastBits);
                    }
                    else
                    {
                        backLen = (byte)(n * 8);
                    }

                    if (n == 0)
                    {
                        n = 1;
                    }

                    if (n > MAX_LEN)
                    {
                        n = MAX_LEN;
                    }

                    i = 0;
                    while (i < n)
                    {
                        backData.Add(ReadSpi(FIFODataReg));
                        i++;
                    }
                }
            }
            else
            {
                status = Error;
            }

            LogInfo.Log($"{nameof(SendCommand)} - {status} {BitConverter.ToString(backData?.ToArray())} BackLen: {backLen}", LogLevel.Debug);

            return (status, backData.ToArray(), backLen);
        }

        public (Status status, byte[] atqa) Request(RequestMode requestMode)
        {
            var tagType = new List<byte> { (byte)requestMode };
            // Reset baud rates
            WriteSpi(TxModeReg, (byte)0x00);
            WriteSpi(RxModeReg, (byte)0x00);
            // Reset ModWidthReg
            WriteSpi(ModWidthReg, 0x26);
            WriteSpi(BitFramingReg, 0x07);

            var (status, backData, backBits) = SendCommand(Transceive, tagType.ToArray());
            LogInfo.Log($"{nameof(Request)} - {BitConverter.ToString(backData)} BackBits: {backBits.ToString()}", LogLevel.Debug);
            if ((status != OK) | (backBits != 0x10))
            {
                status = Error;
            }

            return (status, backData);
        }

        public (Status status, byte[] data) AntiCollision()
        {
            byte serNumCheck = 0;

            var serNum = new List<byte>();

            WriteSpi(BitFramingReg, (byte)0x00);

            serNum.Add((byte)RequestMode.AntiCollision);
            serNum.Add(0x20);

            var (status, backData, _) = SendCommand(Transceive, serNum.ToArray());

            var i = 0;
            if (status == OK)
            {
                i = 0;
            }

            if (backData.Length == 5)
            {
                while (i < 4)
                {
                    serNumCheck = (byte)(serNumCheck ^ backData[i]);
                    i = i + 1;
                }

                if (serNumCheck != backData[i])
                {
                    status = Error;
                }
            }
            else
            {
                status = Error;
            }

            return (status, backData);
        }

        private byte[] CalulateCRC(byte[] pIndata)
        {
            WriteSpi(CommandReg, Idle);
            WriteSpi(DivIrqReg, 0x04);
            WriteSpi(FIFOLevelReg, 0x80);
            byte i = 0;
            while (i < pIndata.Length)
            {
                WriteSpi(FIFODataReg, pIndata[i]);
                i++;
            }

            WriteSpi(CommandReg, (byte)CalculateCRC);
            i = 0xFF;
            while (true)
            {
                var n = ReadSpi(DivIrqReg);
                i--;
                if (!((i != 0) && !Convert.ToBoolean(n & 0x04)))
                {
                    break;
                }
            }

            var pOutData = new List<byte> { ReadSpi(CRCResultRegL), ReadSpi(CRCResultRegM) };
            return pOutData.ToArray();
        }

        public (Status status, byte Sak) SelectTag(byte[] serialNumber)
        {
            LogInfo.Log($"{nameof(SelectTag)}: {BitConverter.ToString(serialNumber)}", LogLevel.Debug);
            var buf = new List<byte> { (byte)RequestMode.SelectTag, 0x70 };
            var i = 0;
            while (i < serialNumber.Length)
            {
                buf.Add(serialNumber[i]);
                i++;
            }

            var pOut = CalulateCRC(buf.ToArray());
            buf.Add(pOut[0]);
            buf.Add(pOut[1]);
            var (status, backData, backBits) = SendCommand(Transceive, buf.ToArray());

            if (backData?.Length > 0)
                LogInfo.Log($"{nameof(SelectTag)} - SAK {backData[0].ToString("X2")}", LogLevel.Debug);
            else
                LogInfo.Log($"{nameof(SelectTag)} - {status}", LogLevel.Debug);

            if (status != OK || backBits != 0x18)
                return (Status.Error, 0);

            return (status, backData[0]);

        }

        public Status Authenticate(RequestMode authenticationMode, byte blockAddress, byte[] sectorKey, byte[] serialNumber)
        {
            // First byte should be the authMode (A or B) Second byte is the trailerBlock (usually 7)
            var buff = new List<byte> { (byte)authenticationMode, blockAddress };

            // Now we need to append the authKey which usually is 6 bytes of 0xFF
            var i = 0;
            while (i < sectorKey.Length)
            {
                buff.Add(sectorKey[i]);
                i++;
            }

            i = 0;

            // Next we append the first 4 bytes of the UID
            while (i < 4)
            {
                buff.Add(serialNumber[i]);
                i++;
            }

            WriteSpi(BitFramingReg, (byte)0x00);
            // Now we start the authentication itself
            var (status, _, _) = SendCommand(Command.Authenticate, buff.ToArray());

            LogInfo.Log($"{nameof(Authenticate)} - {status}", LogLevel.Debug);
            // Check if an error occurred
            if (status != OK)
            {
                return status;
                //throw new Exception("AUTH ERROR!!");
            }

            if ((ReadSpi(Status2Reg) & 0x08) == 0)
            {
                //throw new Exception("AUTH ERROR(status2reg & 0x08) != 0");
                return Status.Error;
            }

            // Return the status
            return status;
        }

        public void ClearSelection()
        {
            ClearBitMask(Status2Reg, 0x08);
        }

        public void WriteCardData(byte blockAddress, byte[] writeData)
        {
            var buff = new List<byte> { (byte)RequestMode.Write, blockAddress };
            var crc = CalulateCRC(buff.ToArray());
            buff.Add(crc[0]);
            buff.Add(crc[1]);
            var (status, backData, backLen) = SendCommand(Transceive, buff.ToArray());

            if (status != OK || backLen != 4 || (backData[0] & 0x0F) != 0x0A)
            {
                status = Error;
            }

            Debug.WriteLine($"{backLen} backdata &0x0F == 0x0A {backData[0] & 0x0F}");

            if (status != OK) return;

            var i = 0;
            var buf = new List<byte>();
            while (i < 16)
            {
                buf.Add(writeData[i]);
                i++;
            }

            crc = CalulateCRC(buf.ToArray());
            buf.Add(crc[0]);
            buf.Add(crc[1]);

            (status, backData, backLen) = SendCommand(Transceive, buf.ToArray());

            if (status != OK || backLen != 4 || (backData[0] & 0x0F) != 0x0A)
            {
                throw new Exception("Failed to write to");
            }
        }

        public void DumpClassic1K(byte[] key, byte[] uid)
        {
            byte i = 0;

            while (i < 64)
            {
                var status = Authenticate(Authenticate1A, i, key, uid);

                // Check if authenticated
                if (status == OK)
                {
                    ReadSpi(i);
                }
                else
                {
                    throw new Exception("Authentication error");
                }

                i++;
            }
        }

        public void Dispose()
        {
            _spiDevice?.Dispose();
            _controller?.Dispose();
        }

        public override int WriteRead(byte targetNumber, ReadOnlySpan<byte> dataToSend, Span<byte> dataFromCard)
        {
            Status status;
            byte[] backData = null;
            byte backLen;
            // Use built in functions for authentication
            if ((dataToSend[0] == (byte)Authenticate1A) || (dataToSend[0] == (byte)Authenticate1B))
            {
                status = Authenticate((RequestMode)dataToSend[0],
                    dataToSend[1],
                    dataToSend.Slice(2, 6).ToArray(),
                    dataToSend.Slice(8, 4).ToArray()
                    );
                //return status == OK ? 0 : -1;
                //WriteSpi(BitFramingReg, (byte)0x00);
                //(status, backData, backLen) = SendCommand(Command.Authenticate, dataToSend.ToArray());
                //LogInfo.Log($"SendCommand {BitConverter.ToString(dataToSend.ToArray())}, Status: {status}, Retdata: {backLen}", LogLevel.Info);
                LogInfo.Log($"SendCommand {BitConverter.ToString(dataToSend.ToArray())}, Status: {status}", LogLevel.Info);
                return status == Status.OK ? 0 : -1;

            }
            else if ((dataToSend[0] == (byte)RequestMode.Increment) || (dataToSend[0] == (byte)RequestMode.Decrement)
                || (dataToSend[0] == (byte)RequestMode.Restore))
            {
                return TwoStepsIncDecRestore(dataToSend, dataFromCard);
            }
            else if(dataToSend[0] == (byte)RequestMode.Read)
            {
                Span<byte> toSend = stackalloc byte[4];
                var crc = CalulateCRC(dataToSend.Slice(0, 2).ToArray());
                dataToSend.Slice(0, 2).CopyTo(toSend);
                crc.CopyTo(toSend.Slice(2));
                LogInfo.Log($"Array to send: {BitConverter.ToString(toSend.ToArray())}", LogLevel.Debug);
                (status, backData, backLen) = SendCommand(Transceive, dataToSend.ToArray());
                if (status != Status.OK)
                {
                    LogInfo.Log($"Reading - Error {(RequestMode)dataToSend[0]}", LogLevel.Debug);
                    return -1;
                }
                if (backData?.Length > 0)
                    backData.CopyTo(dataFromCard);
                return backData == null ? -1 : backData.Length;
            }


            (status, backData, backLen) = SendCommand(Transceive, dataToSend.ToArray());

            LogInfo.Log($"SendCommand {BitConverter.ToString(dataToSend.ToArray())}, Status: {status}, Retdata: {backLen}", LogLevel.Info);
            if (status != OK)
            {
                return -1;
            }
            if (backData?.Length > 0)
                backData.CopyTo(dataFromCard);
            return backData == null ? -1 : backData.Length;


        }

        private int TwoStepsIncDecRestore(ReadOnlySpan<byte> dataToSend, Span<byte> dataFromCard)
        {
            Span<byte> toSendFirst = stackalloc byte[4];
            var crc = CalulateCRC(dataToSend.Slice(0, 2).ToArray());
            dataToSend.Slice(0, 2).CopyTo(toSendFirst);
            crc.CopyTo(toSendFirst.Slice(2));
            var (status, backData, backLen) = SendCommand(Transceive, dataToSend.ToArray());
            if (status != Status.OK)
            {
                LogInfo.Log($"{nameof(TwoStepsIncDecRestore)} - Error {(RequestMode)dataToSend[0]}", LogLevel.Debug);
                return -1;
            }
            Span<byte> toSendSecond = stackalloc byte[dataToSend.Length];
            crc = CalulateCRC(dataToSend.Slice(0, 2).ToArray());
            dataToSend.Slice(2).CopyTo(toSendSecond);
            crc.CopyTo(toSendSecond.Slice(2));
            (status, backData, backLen) = SendCommand(Transceive, dataToSend.ToArray());
            if (backData?.Length > 0)
                backData.CopyTo(dataFromCard);
            LogInfo.Log($"{nameof(TwoStepsIncDecRestore)} - Status {status} - {(RequestMode)dataToSend[0]}", LogLevel.Debug);
            return backData == null ? -1 : backData.Length;
        }
    }
}
