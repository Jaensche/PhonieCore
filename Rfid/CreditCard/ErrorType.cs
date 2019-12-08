using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Rfid.CreditCardProcessing
{

    public enum ErrorType
    {
        ProcessCompletedNormal,
        ProcessCompletedWarning,
        ProcessAbordedExecution,
        ProcessAbordedChecking,
        StateNonVolatileMemoryUnchangedSelectedFileInvalidated,
        StateNonVolatileMemoryChangedAuthenticationFailed,
        StateNonVolatileMemoryChanged,
        CommandNotAllowedAuthenticationMethodBlocked,
        CommandNotAllowedReferenceDataInvalidated,
        CommandNotAllowedConditionsNotSatisfied,
        WrongParameterP1P2FunctionNotSupported,
        WrongParameterP1P2FileNotFound,
        WrongParameterP1P2RecordNotFound,
        ReferenceDataNotFound,
        WrongLength,
        BytesStillAvailable,
        Unknown
    }
}
