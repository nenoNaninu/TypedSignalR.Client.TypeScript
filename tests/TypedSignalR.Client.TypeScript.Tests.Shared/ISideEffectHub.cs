using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypedSignalR.Client.TypeScript.Tests.Shared;

[Hub]
public interface ISideEffectHub
{
    Task Init();
    Task Increment();
    Task<int> Result();

    Task Post(UserDefinedType instance);
    Task<UserDefinedType[]> Fetch();
}
