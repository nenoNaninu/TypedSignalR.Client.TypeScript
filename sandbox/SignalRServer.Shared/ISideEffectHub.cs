using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypedSignalR.Client.TypeScript;

namespace SignalRServer.Shared;

[Hub]
public interface ISideEffectHub
{
    Task Init();
    Task Increment();
    Task<int> Result();

    Task Post(UserDefinedType instance);
    Task<UserDefinedType[]> Fetch();
}
