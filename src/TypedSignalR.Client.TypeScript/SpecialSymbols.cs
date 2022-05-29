using System;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.TypeScript;

internal class SpecialSymbols
{
    public readonly INamedTypeSymbol TaskSymbol;
    public readonly INamedTypeSymbol GenericTaskSymbol;
    public readonly INamedTypeSymbol HubAttributeSymbol;
    public readonly INamedTypeSymbol ReceiverAttributeSymbol;
    public readonly INamedTypeSymbol TranspilationSourceAttributeSymbol;

    public SpecialSymbols(Compilation compilation)
    {
        TaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")!;
        GenericTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")!;

        var hubAttributeSymbol = compilation.GetTypeByMetadataName("TypedSignalR.Client.TypeScript.HubAttribute");
        var receiverAttributeSymbol = compilation.GetTypeByMetadataName("TypedSignalR.Client.TypeScript.ReceiverAttribute");
        var transpilationSourceAttributeSymbol = compilation.GetTypeByMetadataName("Tapper.TranspilationSourceAttribute");

        if (hubAttributeSymbol is null)
        {
            throw new InvalidOperationException("TypedSignalR.Client.TypeScript.HubAttribute is not found");
        }

        if (receiverAttributeSymbol is null)
        {
            throw new InvalidOperationException("TypedSignalR.Client.TypeScript.ReceiverAttribute is not found");
        }

        if (transpilationSourceAttributeSymbol is null)
        {
            throw new InvalidOperationException("Tapper.TranspilationSourceAttribute is not found");
        }

        HubAttributeSymbol = hubAttributeSymbol;
        ReceiverAttributeSymbol = receiverAttributeSymbol;
        TranspilationSourceAttributeSymbol = transpilationSourceAttributeSymbol;
    }
}
