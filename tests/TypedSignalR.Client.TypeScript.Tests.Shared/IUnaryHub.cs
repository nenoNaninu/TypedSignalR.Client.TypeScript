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

[TranspilationSource]
public class MyRequestItem
{
    public string? Text { get; init; }
}

[TranspilationSource]
public class MyResponseItem
{
    public required string Text { get; init; }
}

[TranspilationSource]
public class MyRequestItem2
{
    public required Guid Id { get; init; }
}

[TranspilationSource]
public class MyResponseItem2
{
    public Guid Id { get; init; }
}

[Hub]
public interface IUnaryHub
{
    Task<string> Get();
    Task<int> Add(int x, int y);
    Task<string> Cat(string x, string y);
    Task<UserDefinedType> Echo(UserDefinedType instance);
    Task<MyEnum> EchoMyEnum(MyEnum myEnum);
    Task<MyResponseItem[]> RequestArray(MyRequestItem[] array);
    Task<List<MyResponseItem2>> RequestList(List<MyRequestItem2> list);
}
