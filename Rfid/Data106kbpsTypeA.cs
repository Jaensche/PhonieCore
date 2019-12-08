﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Rfid
{
    /// <summary>
    /// Create a 106 kbpd card like a Mifare card
    /// </summary>
    public class Data106kbpsTypeA
    {
        /// <summary>
        /// The target number, should be 1 or 2 with PN532
        /// </summary>
        public byte TargetNumber { get; set; }

        /// <summary>
        /// Known as SENS_RES in the documentation
        /// Answer To reQuest, Type A
        /// </summary>
        public ushort Atqa { get; set; }

        /// <summary>
        /// Know as SEL_RES in the documentation
        /// Select AcKnowledge
        /// </summary>
        public byte Sak { get; set; }

        /// <summary>
        /// The unique NFC ID
        /// </summary>
        public byte[] NfcId { get; set; }

        /// <summary>
        /// Potential extra Answer To Select data
        /// </summary>
        public byte[] Ats { get; set; }
    }
}
