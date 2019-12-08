using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iot.Device.Rfid.CreditCardProcessing
{
    public class ProprietaryTemplate
    {

        private const byte ApplicationLabelId = 0x50;
        private const byte ApplicationPriorityId = 0x87;
        private const ushort PodlId = 0x9F38;
        private const ushort LanguagePreferenceId = 0x5F2D;
        private const ushort IssuerDiscretianaryId = 0xBF0C;
        private const byte ApplicationTemplateId = 0x61;

        public string ApplicationLabel { get; set; }
        public byte ApplicationPriorityIndicator { get; set; }
        public byte[] ProcessingOptionsDataObjectList { get; set; }
        public List<BerElement> ProcessOptionsDecoded { get; set; }
        public string LanguagePreference { get; set; }

        public List<ApplicationTemplate> ApplicationTemplates { get; set; }

        public ProprietaryTemplate(Span<byte> span)
        {
            var rootSplitter = new BerSplitter(span);

            var appLabel = rootSplitter.BerElements.Where(m => m.Tag == ApplicationLabelId).FirstOrDefault();
            if (appLabel != null)
                ApplicationLabel = Encoding.Default.GetString(appLabel.Data);

            var appLang = rootSplitter.BerElements.Where(m => m.Tag == LanguagePreferenceId).FirstOrDefault();
            if (appLang != null)
                LanguagePreference = Encoding.Default.GetString(appLang.Data);

            var appPrio = rootSplitter.BerElements.Where(m => m.Tag == ApplicationPriorityId).FirstOrDefault();
            if (appPrio != null)
                ApplicationPriorityIndicator = appPrio.Data[0];

            var appPodl = rootSplitter.BerElements.Where(m => m.Tag == PodlId).FirstOrDefault();
            if (appPodl != null)
            {
                ProcessingOptionsDataObjectList = appPodl.Data;
                ProcessOptionsDecoded = new List<BerElement>();
                int index = 0;
                while (index < ProcessingOptionsDataObjectList.Length)
                {
                    //Decode mono dimention (so 1 byte array) Ber elements but which can have ushort or byte tags
                    var elem = new BerElement();
                    elem.Data = new byte[1];
                    if ((ProcessingOptionsDataObjectList[index] & 0b0001_1111) == 0b0001_1111)
                    {
                        elem.Tag = BinaryPrimitives.ReadUInt16BigEndian(ProcessingOptionsDataObjectList.AsSpan().Slice(index, 2));
                        index += 2;
                        ProcessingOptionsDataObjectList.AsSpan().Slice(index++, 1).CopyTo(elem.Data);
                    }
                    else
                    {
                        elem.Tag = ProcessingOptionsDataObjectList[index++];
                        ProcessingOptionsDataObjectList.AsSpan().Slice(index++, 1).CopyTo(elem.Data);
                    }
                    ProcessOptionsDecoded.Add(elem);
                }
            }

            var constrol = rootSplitter.BerElements.Where(m => m.Tag == IssuerDiscretianaryId).FirstOrDefault();
            if (constrol == null)
                throw new Exception($"Missing File Control Information (FCI) Issuer Discretionary Data");
            var appsplitter = new BerSplitter(constrol.Data);
            var apps = appsplitter.BerElements.Where(m => m.Tag == ApplicationTemplateId);

            ApplicationTemplates = new List<ApplicationTemplate>();
            foreach (var app in apps)
            {
                var elem = new ApplicationTemplate(app.Data);
                ApplicationTemplates.Add(elem);
            }
        }

        public void Updagte(ProprietaryTemplate prop)
        {
            if ((prop.ApplicationLabel != "") && (ApplicationLabel == ""))
                ApplicationLabel = prop.ApplicationLabel;

            if ((prop.ApplicationPriorityIndicator != 0) && (ApplicationPriorityIndicator == 0))
                ApplicationPriorityIndicator = prop.ApplicationPriorityIndicator;

            if ((prop.ProcessingOptionsDataObjectList != null) && (ProcessingOptionsDataObjectList == null))
            {
                ProcessOptionsDecoded = prop.ProcessOptionsDecoded;
                ProcessingOptionsDataObjectList = prop.ProcessingOptionsDataObjectList;
            }

            if ((prop.LanguagePreference != "") && (LanguagePreference == ""))
                LanguagePreference = prop.LanguagePreference;

            if ((prop.ApplicationTemplates != null) && (ApplicationTemplates == null))
                ApplicationTemplates = prop.ApplicationTemplates;
            else
            {
                foreach(var app in ApplicationTemplates)
                {
                    app.Update(prop.ApplicationTemplates.Where(m => m.ApplicationPriorityIndicator == app.ApplicationPriorityIndicator).FirstOrDefault());
                }
            }
        }
    }
}

