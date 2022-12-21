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

        var hubAttributeSymbol = compilation.GetTypeByMetadataName("TypedSignalR.Client.HubAttribute");
        var receiverAttributeSymbol = compilation.GetTypeByMetadataName("TypedSignalR.Client.ReceiverAttribute");
        var transpilationSourceAttributeSymbol = compilation.GetTypeByMetadataName("Tapper.TranspilationSourceAttribute");

        HubAttributeSymbol = hubAttributeSymbol ?? throw new InvalidOperationException("TypedSignalR.Client.HubAttribute is not found");
        ReceiverAttributeSymbol = receiverAttributeSymbol ?? throw new InvalidOperationException("TypedSignalR.Client.ReceiverAttribute is not found");
        TranspilationSourceAttributeSymbol = transpilationSourceAttributeSymbol ?? throw new InvalidOperationException("Tapper.TranspilationSourceAttribute is not found");
    }
}
