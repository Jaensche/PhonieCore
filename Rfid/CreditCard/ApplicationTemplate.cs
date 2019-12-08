using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Iot.Device.Rfid.CreditCardProcessing
{
    public class ApplicationTemplate
    {
        private const byte ApplicationTemplateId = 0x61;
        private const byte ApplicationIdentifierId = 0x4F;
        private const byte ApplicationLabelId = 0x50;
        private const byte ApplicationPriorityId = 0x87;
        private const ushort IssuerCountryCodeId = 0x5F55;
        private const byte IssuerIdentificationNumberId = 0x42;
        private const ushort ContactlessKernelId = 0x9F2A;
        private const ushort AsrpdId = 0x9F0A;

        public byte[] ApplicationIdentifier { get; set; }
        public string ApplicationLabel { get; set; }
        public byte ApplicationPriorityIndicator { get; set; }
        public string IssuerCountryCode { get; set; }
        public byte[] IssuerIdentificationNumber { get; set; }
        public byte ContactlessKernel { get; set; }
        public byte[] ApplicationSelectionRegisteredProprietaryData { get; set; }

        public EmvProprietaryTemplate EmvProprietaryTemplate { get; set; }

        public ApplicationTemplate(Span<byte> span)
        {
            var rootSplitter = new BerSplitter(span);

            var details = rootSplitter.BerElements.Where(m => m.Tag == ApplicationIdentifierId).FirstOrDefault();
            if (details != null)
                ApplicationIdentifier = details.Data;

            details = rootSplitter.BerElements.Where(m => m.Tag == ApplicationLabelId).FirstOrDefault();
            if (details != null)
                ApplicationLabel = Encoding.Default.GetString(details.Data);

            details = rootSplitter.BerElements.Where(m => m.Tag == IssuerCountryCodeId).FirstOrDefault();
            if (details != null)
                IssuerCountryCode = Encoding.Default.GetString(details.Data);

            details = rootSplitter.BerElements.Where(m => m.Tag == ApplicationPriorityId).FirstOrDefault();
            if (details != null)
                ApplicationPriorityIndicator = details.Data[0];

            details = rootSplitter.BerElements.Where(m => m.Tag == ContactlessKernelId).FirstOrDefault();
            if (details != null)
                ContactlessKernel = details.Data[0];

            details = rootSplitter.BerElements.Where(m => m.Tag == AsrpdId).FirstOrDefault();
            if (details != null)
                ApplicationSelectionRegisteredProprietaryData = details.Data;


        }

        public void Update(ApplicationTemplate app)
        {
            if (app == null)
                return;
            if (!ApplicationIdentifier.SequenceEqual(app.ApplicationIdentifier))
                return;

            if ((app.ApplicationLabel != "") && (ApplicationLabel == ""))
                ApplicationLabel = app.ApplicationLabel;

            if ((app.IssuerCountryCode != "") && (IssuerCountryCode == ""))
                IssuerCountryCode = app.IssuerCountryCode;

            if ((app.IssuerIdentificationNumber != null) && (IssuerIdentificationNumber == null))
                IssuerIdentificationNumber = app.IssuerIdentificationNumber;

            if ((app.ContactlessKernel != 0) && (ContactlessKernel == 0))
                ContactlessKernel = app.ContactlessKernel;

            if ((app.ApplicationSelectionRegisteredProprietaryData != null) && (ApplicationSelectionRegisteredProprietaryData == null))
                ApplicationSelectionRegisteredProprietaryData = app.ApplicationSelectionRegisteredProprietaryData;
        }
    }
}
