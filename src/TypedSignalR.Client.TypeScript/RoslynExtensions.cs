using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.TypeScript;

internal static class RoslynExtensions
{
    public static IEnumerable<INamedTypeSymbol> GetNamedTypeSymbols(this Compilation compilation)
    {
        var namedTypeSymbols = compilation
            .SyntaxTrees
            .SelectMany(syntaxTree =>
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                return syntaxTree.GetRoot()
                    .DescendantNodes()
                    .Select(x => semanticModel.GetDeclaredSymbol(x))
                    .OfType<INamedTypeSymbol>();
            }).ToArray();

        return namedTypeSymbols;
    }

    public static INamedTypeSymbol[] GetAttributeAnnotatedTypes(this Compilation compilation, INamedTypeSymbol attributeSymbol)
    {
        var types = compilation.GetNamedTypeSymbols()
            .Where(x =>
            {
                var attributes = x.GetAttributes();

                if (attributes.IsEmpty)
                {
                    return false;
                }

                return attributes.Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attributeSymbol));
            })
            .ToArray();

        return types;
    }

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

    public static bool IsAttributeAnnotated(this INamedTypeSymbol source, INamedTypeSymbol attributeSymbol)
    {
        return source.GetAttributes()
            .Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attributeSymbol));
    }

    public static bool IsGenericType(this ITypeSymbol source)
    {
        var namedTypeSymbol = source as INamedTypeSymbol;

        return namedTypeSymbol is not null && namedTypeSymbol.IsGenericType;
    }
}
