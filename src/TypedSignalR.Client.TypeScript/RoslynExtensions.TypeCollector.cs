using System.Collections.Generic;
using System.Collections.Immutable;
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

    public static IReadOnlyList<INamedTypeSymbol> GetAttributeAnnotatedTypes(
        this Compilation compilation,
        ImmutableArray<INamedTypeSymbol> attributeSymbols,
        bool includeReferencedAssemblies)
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

                foreach (var attributeSymbol in attributeSymbols)
                {
                    if (attributes.Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol)))
                    {
                        return true;
                    }
                }

                return false;
            })
            .ToArray();

        return types;
    }

    public static ITypeSymbol GetFeaturedType(this ITypeSymbol typeSymbol, SpecialSymbols specialSymbols)
    {
        if (typeSymbol.IsGenericType())
        {
            // IAsyncEnumerable<T> -> T
            // ChannelReader<T> -> T
            // Task<T> -> T
            // Task<IAsyncEnumerable<T>> -> T
            // Task<ChannelReader<T>> -> T
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                // IAsyncEnumerable<T>
                // ChannelReader<T>
                if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol)
                    || SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, specialSymbols.ChannelReaderSymbol))
                {
                    return namedTypeSymbol.TypeArguments[0];
                }

                // Task<T>
                if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, specialSymbols.GenericTaskSymbol))
                {
                    var typeArg = namedTypeSymbol.TypeArguments[0] as INamedTypeSymbol;

                    // Task<IAsyncEnumerable<T>> -> T
                    // NOTE:
                    //     SymbolEqualityComparer.Default.Equals(null, null) return true,
                    //     so verify that specialSymbols.AsyncEnumerableSymbol is not null first.
                    if (specialSymbols.AsyncEnumerableSymbol is not null
                        && SymbolEqualityComparer.Default.Equals(typeArg?.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol))
                    {
                        return typeArg.TypeArguments[0];
                    }

                    // Task<ChannelReader<T>> -> T
                    if (specialSymbols.ChannelReaderSymbol is not null
                        && SymbolEqualityComparer.Default.Equals(typeArg?.OriginalDefinition, specialSymbols.ChannelReaderSymbol))
                    {
                        return typeArg.TypeArguments[0];
                    }

                    // Task<T> -> T
                    return namedTypeSymbol.TypeArguments[0];
                }
            }
        }

        return typeSymbol;
    }
}
