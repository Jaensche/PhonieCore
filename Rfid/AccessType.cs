﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Rfid.Mifare
{
    /// <summary>
    /// The type of access for the data sectors
    /// </summary>
    [Flags]
    public enum AccessType
    {        
        Never = 0b0000_0000,
        ReadKeyA = 0b0000_0001,
        ReadKeyB = 0b0000_0010,
        WriteKeyA = 0b0000_0100,
        WriteKeyB = 0b0000_1000,
        IncrementKeyA = 0b0001_0000,
        IncrementKeyB = 0b0010_0000,
        DecrementTransferRestoreKeyA = 0b0100_0000,
        DecrementTransferRestoreKeyB = 0b1000_0000,
    }
}
