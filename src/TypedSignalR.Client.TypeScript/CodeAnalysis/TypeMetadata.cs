using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.TypeScript.CodeAnalysis;

internal class TypeMetadata
{
    public string Name { get; }
    public IReadOnlyList<IMethodSymbol> Methods { get; }

    public TypeMetadata(INamedTypeSymbol namedTypeSymbol)
    {
        Name = namedTypeSymbol.Name;
        Methods = namedTypeSymbol.GetMethods().ToArray();
    }
}
