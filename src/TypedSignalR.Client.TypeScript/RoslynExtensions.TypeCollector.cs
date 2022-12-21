using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.TypeScript;

internal static partial class RoslynExtensions
{
    private static INamedTypeSymbol[]? NamedTypeSymbols;

    /// <summary>
    /// Get NamedTypeSymbols from target project.
    /// </summary>
    public static IReadOnlyList<INamedTypeSymbol> GetNamedTypeSymbols(this Compilation compilation)
    {
        if (NamedTypeSymbols is not null)
        {
            return NamedTypeSymbols;
        }

        NamedTypeSymbols = compilation
            .SyntaxTrees
            .SelectMany(syntaxTree =>
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                return syntaxTree.GetRoot()
                    .DescendantNodes()
                    .Select(x => semanticModel.GetDeclaredSymbol(x))
                    .OfType<INamedTypeSymbol>();
            }).ToArray();

        return NamedTypeSymbols;
    }

    private static INamedTypeSymbol[]? GlobalNamedTypeSymbols;

    /// <summary>
    /// Get NamedTypeSymbols from target project and reference assemblies.
    /// </summary>
    public static IReadOnlyList<INamedTypeSymbol> GetGlobalNamedTypeSymbols(this Compilation compilation)
    {
        if (GlobalNamedTypeSymbols is not null)
        {
            return GlobalNamedTypeSymbols;
        }

        var typeCollector = new GlobalNamedTypeCollector();
        typeCollector.Visit(compilation.GlobalNamespace);

        GlobalNamedTypeSymbols = typeCollector.ToArray();
        return GlobalNamedTypeSymbols;
    }

    public static IReadOnlyList<INamedTypeSymbol> GetAttributeAnnotatedTypes(this Compilation compilation, INamedTypeSymbol attributeSymbol, bool includeReferencedAssemblies)
    {
        var namedTypes = includeReferencedAssemblies ? compilation.GetGlobalNamedTypeSymbols() : compilation.GetNamedTypeSymbols();

        var types = namedTypes
            .Where(t =>
            {
                var attributes = t.GetAttributes();

                if (attributes.IsEmpty)
                {
                    return false;
                }

                return attributes.Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
            })
            .ToArray();

        return types;
    }
}
