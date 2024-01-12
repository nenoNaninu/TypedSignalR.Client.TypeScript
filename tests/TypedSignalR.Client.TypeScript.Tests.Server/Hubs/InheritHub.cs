using Microsoft.AspNetCore.SignalR;
using TypedSignalR.Client.TypeScript.Tests.Shared;

namespace TypedSignalR.Client.TypeScript.Tests.Server.Hubs;

public class InheritHub : Hub<IInheritHubReceiver>, IInheritHub
{
    public Task<int> Add(int x, int y)
    {
        return Task.FromResult(x + y);
    }

    public Task<string> Cat(string x, string y)
    {
        return Task.FromResult(x + y);
    }

    public Task<UserDefinedType> Echo(UserDefinedType instance)
    {
        return Task.FromResult(instance);
    }

    public Task<string> Get()
    {
        return Task.FromResult("TypedSignalR.Client.TypeScript");
    }
}
