using System;
using System.Threading.Tasks;
using Tapper;
using TypedSignalR.Client.TypeScript;

namespace SignalRServer.Shared;

[TranspilationSource]
public class UserDefinedType
{
    public DateTime DateTime { get; set; }
    public Guid Guid { get; set; }
}

[Hub]
public interface IUnaryHub
{
    Task<string> Get();
    Task<int> Add(int x, int y);
    Task<string> Cat(string x, string y);
    Task<UserDefinedType> Echo(UserDefinedType instance);
}
