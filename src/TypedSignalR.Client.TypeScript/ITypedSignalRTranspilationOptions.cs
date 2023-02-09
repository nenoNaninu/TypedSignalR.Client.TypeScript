using Tapper;

namespace TypedSignalR.Client.TypeScript;

public interface ITypedSignalRTranspilationOptions : ITranspilationOptions
{
    MethodStyle MethodStyle { get; }
}
