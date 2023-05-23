using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.TypeScript;

internal static partial class RoslynExtensions
{
    public static IEnumerable<ISymbol> GetPublicFieldsAndProperties(this INamedTypeSymbol source)
    {
        return source.GetMembers()
            .Where(static x =>
            {
                if (x.DeclaredAccessibility is not Accessibility.Public)
                {
                    return false;
                }

                if (x is IFieldSymbol fieldSymbol)
                {
                    return fieldSymbol.AssociatedSymbol is null;
                }

                return x is IPropertySymbol;
            });
    }

    public static IEnumerable<IMethodSymbol> GetMethods(this INamedTypeSymbol source)
    {
        return source.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(static x => x.MethodKind == MethodKind.Ordinary);
    }

    public static IEnumerable<ISymbol> IgnoreStatic(this IEnumerable<ISymbol> source)
    {
        return source
            .Where(static x =>
            {
                if (x.ContainingType.TypeKind is TypeKind.Enum)
                {
                    return true;
                }

                if (x is IFieldSymbol fieldSymbol)
                {
                    if (fieldSymbol.AssociatedSymbol is not null)
                    {
                        return false;
                    }

                    return !fieldSymbol.IsStatic;
                }

                if (x is IPropertySymbol propertySymbol)
                {
                    return !propertySymbol.IsStatic;
                }

                return false;
            });
    }

    public static IEnumerable<ITypeSymbol> GetRelevantTypes(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.IsGenericType)
            {
                yield return namedTypeSymbol;

                foreach (var typeArgument in namedTypeSymbol.TypeArguments)
                {
                    foreach (var it in GetRelevantTypes(typeArgument))
                    {
                        yield return it;
                    }
                }
            }
            else
            {
                yield return namedTypeSymbol;
            }
        }
        else if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            var elementType = arrayTypeSymbol.ElementType;

            foreach (var it in GetRelevantTypes(elementType))
            {
                yield return it;
            }
        }
        else
        {
            yield return typeSymbol;
        }
    }

    public static bool IsAttributeAnnotated(this INamedTypeSymbol source, INamedTypeSymbol attributeSymbol)
    {
        return source.GetAttributes()
            .Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attributeSymbol));
    }

    public static bool IsAttributeAnnotated(this INamedTypeSymbol source, ImmutableArray<INamedTypeSymbol> attributeSymbols)
    {
        foreach (var attribute in attributeSymbols)
        {
            if (source.IsAttributeAnnotated(attribute))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsGenericType(this ITypeSymbol source)
    {
        var namedTypeSymbol = source as INamedTypeSymbol;

        return namedTypeSymbol is not null && namedTypeSymbol.IsGenericType;
    }
}
