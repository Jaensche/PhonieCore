using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Iot.Device.Rfid.CreditCardProcessing
{
    public class EmvProprietaryTemplate
    {
        private const byte EmvId = 0x70;
        private const ushort CardHolderId = 0x5F20;
        private const ushort Track1DiscretionaryDataId = 0x9F1F;
        private const ushort ExtendedCardHolderId = 0x9F0B;
        private const byte IssuerPublicKeyId = 0x90;
        private const byte CreditCardNumberId = 0x5A;
        private const ushort ExpirationDateId = 0x5F24;
        private const ushort ApplicationDateId = 0x5F25;
        private const byte Track2DiscretionaryDataId = 0x57;
        private const ushort UK1 = 0x9F4A;
        private const ushort ApplicationUsageId = 0x9F07;
        private const ushort ApplicationVersionId = 0x9F08;
        private const ushort IssuerCountryCodeId = 0x5F28;
        private const ushort PanSequenceNumberId = 0x5F34;
        private const byte CardRiskManagementList1Id = 0x8C;
        private const byte CardRiskManagementList2Id = 0x8D;
        private const byte CardVerificationMethodListId = 0x8E;
        private const ushort UK5 = 0x008F;
        private const ushort UK6 = 0x0092;
        private const ushort UK7 = 0x9F32;
        private const ushort UK8 = 0x9F46;
        private const ushort UK9 = 0x9F47;
        private const ushort UK11 = 0x9F48;
        private const ushort ApplicationCurrencyCodeId = 0x9F42;

        public byte[] PublicKeyIssuer { get; set; }
        public string CardHolder { get; set; }
        public string CardHolderExtended { get; set; }
        public byte[] Track1DiscretionaryData { get; set; }
        public byte[] Track2DiscretionaryData { get; set; }
        public byte[] CreditCardNumberRaw { get; set; }
        public string CreditCardNumber
        {
            get
            {
                if (CreditCardNumberRaw == null)
                    return "";
                string ret = "";
                for (int i = 0; i < CreditCardNumberRaw.Length; i++)
                    ret += CreditCardNumberRaw[i].ToString("X2") + (((i & 1) == 1) ? " " : "");
                return ret;
            }
        }

        public DateTime ExpirarionDate { get; set; }
        public DateTime ApplicationDate { get; set; }
        public byte[] IssuerCountryCode { get; set; }
        public byte PanSequenceNumber { get; set; }
        public byte[] CardRiskManagementDataObjectList1 { get; set; }
        public byte[] CardRiskManagementDataObjectList2 { get; set; }
        public byte[] CardVerificationMethodList { get; set; }
        public byte[] ApplicationUsageControl { get; set; }
        public byte[] ApplicationVersionNumber { get; set; }
        public string ApplicationCurrencyCode { get; set; }

        public EmvProprietaryTemplate(Span<byte> span)
        {
            var rootSplitter = new BerSplitter(span);
            var root = rootSplitter.BerElements.Where(m => m.Tag == EmvId).FirstOrDefault();
            if (root == null)
                throw new Exception($"Not a valid EMV Template");

            var detailSplitter = new BerSplitter(root.Data);
            var details = detailSplitter.BerElements.Where(m => m.Tag == CardHolderId).FirstOrDefault();
            if (details != null)
                CardHolder = Encoding.Default.GetString(details.Data);

            details = detailSplitter.BerElements.Where(m => m.Tag == Track1DiscretionaryDataId).FirstOrDefault();
            if (details != null)
                Track1DiscretionaryData = details.Data;

            details = detailSplitter.BerElements.Where(m => m.Tag == Track2DiscretionaryDataId).FirstOrDefault();
            if (details != null)
                Track2DiscretionaryData = details.Data;

            details = detailSplitter.BerElements.Where(m => m.Tag == ExtendedCardHolderId).FirstOrDefault();
            if (details != null)
                CardHolderExtended = Encoding.Default.GetString(details.Data);

            details = detailSplitter.BerElements.Where(m => m.Tag == IssuerPublicKeyId).FirstOrDefault();
            if (details != null)
                PublicKeyIssuer = details.Data;

            details = detailSplitter.BerElements.Where(m => m.Tag == CreditCardNumberId).FirstOrDefault();
            if (details != null)
                CreditCardNumberRaw = details.Data;

            details = detailSplitter.BerElements.Where(m => m.Tag == ExpirationDateId).FirstOrDefault();
            if (details != null)
                ExpirarionDate = DateTime.Parse(details.Data[2].ToString("X2") + "/" + details.Data[1].ToString("X2") + "/" + "20" + details.Data[0].ToString("X2"));

            details = detailSplitter.BerElements.Where(m => m.Tag == ApplicationDateId).FirstOrDefault();
            if (details != null)
                ApplicationDate = DateTime.Parse(details.Data[2].ToString("X2") + "/" + details.Data[1].ToString("X2") + "/20" + details.Data[0].ToString("X2"));

            details = detailSplitter.BerElements.Where(m => m.Tag == ApplicationUsageId).FirstOrDefault();
            if (details != null)
                ApplicationUsageControl = details.Data;

            details = detailSplitter.BerElements.Where(m => m.Tag == ApplicationVersionId).FirstOrDefault();
            if (details != null)
                ApplicationVersionNumber = details.Data;

            details = detailSplitter.BerElements.Where(m => m.Tag == IssuerCountryCodeId).FirstOrDefault();
            if (details != null)
                IssuerCountryCode = details.Data;

            details = detailSplitter.BerElements.Where(m => m.Tag == ApplicationCurrencyCodeId).FirstOrDefault();
            if (details != null)
                ApplicationCurrencyCode = details.Data[0].ToString("X2")+ details.Data[1].ToString("X2");

            

        }

        public void Update(EmvProprietaryTemplate emv)
        {
            if ((CardHolder == "") && (emv.CardHolder != ""))
                CardHolder = emv.CardHolder;

            if ((PublicKeyIssuer == null) && (emv.PublicKeyIssuer != null))
                PublicKeyIssuer = emv.PublicKeyIssuer;

            if ((CardHolderExtended == "") && (emv.CardHolderExtended != null))
                CardHolderExtended = emv.CardHolderExtended;

            if ((Track1DiscretionaryData == null) && (emv.Track1DiscretionaryData != null))
                Track1DiscretionaryData = emv.Track1DiscretionaryData;

            if ((Track2DiscretionaryData == null) && (emv.Track2DiscretionaryData != null))
                Track2DiscretionaryData = emv.Track2DiscretionaryData;

            if ((CreditCardNumberRaw == null) && (emv.CreditCardNumberRaw != null))
                CreditCardNumberRaw = emv.CreditCardNumberRaw;

            if ((ExpirarionDate == DateTime.MinValue) && (emv.ExpirarionDate != DateTime.MinValue))
                ExpirarionDate = emv.ExpirarionDate;

            if ((ApplicationDate == DateTime.MinValue) && (emv.ApplicationDate != DateTime.MinValue))
                ApplicationDate = emv.ApplicationDate;

            if ((ApplicationUsageControl == null) && (emv.ApplicationUsageControl != null))
                ApplicationUsageControl = emv.ApplicationUsageControl;

            if ((ApplicationVersionNumber == null) && (emv.ApplicationVersionNumber != null))
                ApplicationVersionNumber = emv.ApplicationVersionNumber;

            if ((IssuerCountryCode == null) && (emv.IssuerCountryCode != null))
                IssuerCountryCode = emv.IssuerCountryCode;

            if ((ApplicationCurrencyCode == null) && (emv.ApplicationCurrencyCode != null))
                ApplicationCurrencyCode = emv.ApplicationCurrencyCode;
        }
    }
}
