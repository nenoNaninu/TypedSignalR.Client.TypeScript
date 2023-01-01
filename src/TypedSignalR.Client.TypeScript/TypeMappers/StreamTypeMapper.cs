using System;
using Microsoft.CodeAnalysis;
using Tapper;

namespace TypedSignalR.Client.TypeScript.TypeMappers;

internal sealed class AsyncEnumerableTypeMapper : ITypeMapper
{
    public ITypeSymbol Assign { get; }

    public AsyncEnumerableTypeMapper(Compilation compilation)
    {
        Assign = compilation.GetTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1")!;
    }

    public string MapTo(ITypeSymbol typeSymbol, ITranspilationOptions options)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol
            && namedTypeSymbol.IsGenericType
            && SymbolEqualityComparer.Default.Equals(namedTypeSymbol.ConstructedFrom, Assign))
        {
            var typeArgument = namedTypeSymbol.TypeArguments[0];
            var mapper = options.TypeMapperProvider.GetTypeMapper(typeArgument);
            return $"Subject<{mapper.MapTo(typeArgument, options)}>";
        }

        throw new InvalidOperationException($"GenericTaskTypeMapper is not support {typeSymbol.ToDisplayString()}.");
    }
}

internal sealed class ChannelReaderTypeMapper : ITypeMapper
{
    public ITypeSymbol Assign { get; }

    public ChannelReaderTypeMapper(Compilation compilation)
    {
        Assign = compilation.GetTypeByMetadataName("System.Threading.Channels.ChannelReader`1")!;
    }

    public string MapTo(ITypeSymbol typeSymbol, ITranspilationOptions options)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol
            && namedTypeSymbol.IsGenericType
            && SymbolEqualityComparer.Default.Equals(namedTypeSymbol.ConstructedFrom, Assign))
        {
            var typeArgument = namedTypeSymbol.TypeArguments[0];
            var mapper = options.TypeMapperProvider.GetTypeMapper(typeArgument);
            return $"Subject<{mapper.MapTo(typeArgument, options)}>";
        }

        throw new InvalidOperationException($"GenericTaskTypeMapper is not support {typeSymbol.ToDisplayString()}.");
    }
}
