using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypedSignalR.Client.TypeScript.Tests.Shared;

public interface IHubBaseBase
{
    Task<string> Get();
}

public interface IHubBase1 : IHubBaseBase
{
    Task<int> Add(int x, int y);
}

public interface IHubBase2 : IHubBaseBase
{
    Task<string> Cat(string x, string y);
}

[Hub]
public interface IInheritHub : IHubBase1, IHubBase2
{
    Task<UserDefinedType> Echo(UserDefinedType instance);
}


public interface IReceiverBaseBase
{
    Task ReceiveMessage(string message, int value);
}

public interface IReceiverBase1 : IReceiverBaseBase
{
    Task ReceiveCustomMessage(UserDefinedType userDefined);
}

public interface IReceiverBase2 : IReceiverBaseBase
{
    Task Notify();
}

[Receiver]
public interface IInheritHubReceiver : IReceiverBase1, IReceiverBase2
{
    Task ReceiveMessage2(string message, int value);
}
