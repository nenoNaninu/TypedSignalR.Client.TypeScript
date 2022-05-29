using Tapper;

namespace TypedSignalR.Client.TypeScript.Tests.Shared;

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
