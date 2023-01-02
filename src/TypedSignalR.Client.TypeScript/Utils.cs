using Microsoft.CodeAnalysis;
using System.Linq;

namespace TypedSignalR.Client.TypeScript;

internal static class MethodSymbolExtensions
{
    internal static HubMethodType SelectHubMethodType(this IMethodSymbol methodSymbol, SpecialSymbols specialSymbols)
    {
        // sever-to-client streaming method
        //     return type : IAsyncEnumerable<T>, Task<IAsyncEnumerable<T>>, Task<ChannelReader<T>>
        // client-to-server streaming method
        //     parameter type : IAsyncEnumerable<T>, ChannelReader<T>
        // other
        //     ordinary method

        var returnType = methodSymbol.ReturnType;

        // sever-to-client streaming
        // IAsyncEnumerable<T>
        if (SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol))
        {
            return HubMethodType.ServerToClientStreaming;
        }

        // Task<IAsyncEnumerable<T>>, Task<ChannelReader<T>>
        if (SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, specialSymbols.GenericTaskSymbol))
        {
            var typeArg = (returnType as INamedTypeSymbol)!.TypeArguments[0];

            if (SymbolEqualityComparer.Default.Equals(typeArg.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol)
                || SymbolEqualityComparer.Default.Equals(typeArg.OriginalDefinition, specialSymbols.ChannelReaderSymbol))
            {
                return HubMethodType.ServerToClientStreaming;
            }
        }

        // client-to-server streaming method
        var isClientToServerStreaming = methodSymbol.Parameters
            .Any(x =>
                SymbolEqualityComparer.Default.Equals(x.Type.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol)
                || SymbolEqualityComparer.Default.Equals(x.Type.OriginalDefinition, specialSymbols.ChannelReaderSymbol)
            );

        return isClientToServerStreaming ? HubMethodType.ClientToServerStreaming : HubMethodType.Unary;
    }
}

public enum HubMethodType
{
    Unary,
    ServerToClientStreaming,
    ClientToServerStreaming,
}
