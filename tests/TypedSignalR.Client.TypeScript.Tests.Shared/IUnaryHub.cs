using Tapper;

namespace TypedSignalR.Client.TypeScript.Tests.Shared;

[TranspilationSource]
public class UserDefinedType
{
    public DateTime DateTime { get; set; }
    public Guid Guid { get; set; }
}

[TranspilationSource]
public enum MyEnum
{
    None = 0,
    One = 1,
    Two = 2,
    Four = 4,
}

[Hub]
public interface IUnaryHub
{
    Task<string> Get();
    Task<int> Add(int x, int y);
    Task<string> Cat(string x, string y);
    Task<UserDefinedType> Echo(UserDefinedType instance);
    Task<MyEnum> EchoMyEnum(MyEnum myEnum);
}
