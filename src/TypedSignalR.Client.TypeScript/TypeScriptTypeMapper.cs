using Microsoft.CodeAnalysis;
using Tapper;
using Tapper.TypeMappers;

namespace TypedSignalR.Client.TypeScript;

internal static class TypeScriptTypeMapper
{
    public static string MapTo(ITypeSymbol typeSymbol, SpecialSymbols specialSymbols, ITypedSignalRTranspilationOptions options)
    {
        if (TryMapNullableValueType(typeSymbol, specialSymbols, options, out var nullableValueType))
        {
            return nullableValueType;
        }

        if (TryMapGenericWrapper(typeSymbol, specialSymbols, options, out var genericWrapper))
        {
            return genericWrapper;
        }

        var mappedType = TypeMapper.MapTo(typeSymbol, options);

        if (!typeSymbol.IsValueType && typeSymbol.NullableAnnotation is NullableAnnotation.Annotated)
        {
            return $"({mappedType} | {options.GetNullableUnionLiteral()})";
        }

        return mappedType;
    }

    private static bool TryMapNullableValueType(ITypeSymbol typeSymbol, SpecialSymbols specialSymbols, ITypedSignalRTranspilationOptions options, out string mappedType)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol
            && namedTypeSymbol.IsGenericType
            && namedTypeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            mappedType = $"({MapTo(namedTypeSymbol.TypeArguments[0], specialSymbols, options)} | {options.GetNullableUnionLiteral()})";
            return true;
        }

        mappedType = string.Empty;
        return false;
    }

    private static bool TryMapGenericWrapper(ITypeSymbol typeSymbol, SpecialSymbols specialSymbols, ITypedSignalRTranspilationOptions options, out string mappedType)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol || !namedTypeSymbol.IsGenericType)
        {
            mappedType = string.Empty;
            return false;
        }

        if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, specialSymbols.GenericTaskSymbol))
        {
            mappedType = $"Promise<{MapTo(namedTypeSymbol.TypeArguments[0], specialSymbols, options)}>";
            return true;
        }

        if (specialSymbols.AsyncEnumerableSymbol is not null
            && SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol))
        {
            mappedType = $"Subject<{MapTo(namedTypeSymbol.TypeArguments[0], specialSymbols, options)}>";
            return true;
        }

        if (specialSymbols.ChannelReaderSymbol is not null
            && SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, specialSymbols.ChannelReaderSymbol))
        {
            mappedType = $"Subject<{MapTo(namedTypeSymbol.TypeArguments[0], specialSymbols, options)}>";
            return true;
        }

        mappedType = string.Empty;
        return false;
    }
}
