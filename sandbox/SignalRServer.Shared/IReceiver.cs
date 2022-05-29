using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypedSignalR.Client.TypeScript;

namespace SignalRServer.Shared;

[Receiver]
public interface IReceiver
{
    Task ReceiveMessage(string message, int value);
    Task Nofity();
    Task ReceiveCustomMessage(UserDefinedType userDefined);
}

[Hub]
public interface IReceiverTestHub
{
    Task Start();
}
