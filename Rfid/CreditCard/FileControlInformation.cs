using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Iot.Device.Rfid.CreditCardProcessing
{
    public class FileControlInformation
    {
        private const byte FileControlId = 0x6F;
        private const byte DedicatedFileId = 0x84;
        private const byte ProprietaryId = 0xA5;

        //private const byte IssuerDiscretianaryId2 = 0x0C;
        private const byte IssuerPublicKeyId = 0x90;

        public byte[] DedicatedFileName { get; set; }
        public ProprietaryTemplate ProprietaryTemplate { get; set; }        

        public FileControlInformation(Span<byte> span)
        {
            var rootSplitter = new BerSplitter(span);
            Debug.WriteLine($"{BitConverter.ToString(span.ToArray())}");
            var root = rootSplitter.BerElements.Where(m => m.Tag == FileControlId).FirstOrDefault();
            if (root == null)
                throw new Exception($"Not a valid file Control Information Template");
            var dedicatedfciSplitter = new BerSplitter(root.Data);
            var dedicated = dedicatedfciSplitter.BerElements.Where(m => m.Tag == DedicatedFileId).FirstOrDefault();
            if (dedicated == null)
                throw new Exception($"Missing Dedicated File Name");
            DedicatedFileName = dedicated.Data;
            dedicated = dedicatedfciSplitter.BerElements.Where(m => m.Tag == ProprietaryId).FirstOrDefault();
            if (dedicated == null)
                throw new Exception($"Missing File Control Information (FCI) Proprietary Template");            

            ProprietaryTemplate = new ProprietaryTemplate(dedicated.Data);

        }

        public void Update(FileControlInformation fs)
        {
            // Rule is not to replace something existing that is not null or if a string 
            if ((fs.DedicatedFileName != null) && (DedicatedFileName == null))
                DedicatedFileName = fs.DedicatedFileName;
            if ((fs.ProprietaryTemplate != null) && (ProprietaryTemplate == null))
                ProprietaryTemplate = fs.ProprietaryTemplate;


        }
    }
}
