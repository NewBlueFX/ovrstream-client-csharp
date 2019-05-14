using System;
using System.Collections.Generic;
using System.Text;

namespace ovrstream_client_csharp.Messages
{
    internal enum MessageTypes : int
    {
        Signal = 1,
        PropertyUpdate = 2,
        Init = 3,
        Idle = 4,
        Debug = 5,
        InvokeMethod = 6,
        ConnectToSignal = 7,
        DisconnectFromSignal = 8,
        SetProperty = 9,
        Response = 10,
    }
}
