using System;
using Microsoft.CodeAnalysis;
using Tapper;

namespace TypedSignalR.Client.TypeScript.TypeMappers;

internal sealed class TaskTypeMapper : ITypeMapper
{
    public ITypeSymbol Assign { get; }

    public TaskTypeMapper(Compilation compilation)
    {
        Assign = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")!;
    }

    public string MapTo(ITypeSymbol typeSymbol, ITranspilationOptions options)
    {
        if (SymbolEqualityComparer.Default.Equals(typeSymbol, Assign))
        {
            return "Promise<void>";
        }

        throw new InvalidOperationException($"TaskTypeMapper is not support {typeSymbol.ToDisplayString()}.");
    }
}

internal sealed class GenericTaskTypeMapper : ITypeMapper
{
    public ITypeSymbol Assign { get; }

    public GenericTaskTypeMapper(Compilation compilation)
    {
        Assign = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")!;
    }

    public string MapTo(ITypeSymbol typeSymbol, ITranspilationOptions options)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol
            && namedTypeSymbol.IsGenericType
            && SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, Assign))
        {
            var typeArgument = namedTypeSymbol.TypeArguments[0];
            var mapper = options.TypeMapperProvider.GetTypeMapper(typeArgument);
            return $"Promise<{mapper.MapTo(typeArgument, options)}>";
        }

        throw new InvalidOperationException($"GenericTaskTypeMapper is not support {typeSymbol.ToDisplayString()}.");
    }
}
