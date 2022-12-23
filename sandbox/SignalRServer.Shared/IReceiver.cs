using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypedSignalR.Client;

namespace SignalRServer.Shared;

[Receiver]
public interface IReceiver
{
    Task ReceiveMessage(string message, int value);
    Task Notify();
    Task ReceiveCustomMessage(UserDefinedType userDefined);
}

[Hub]
public interface IReceiverTestHub
{
    Task Start();
}
