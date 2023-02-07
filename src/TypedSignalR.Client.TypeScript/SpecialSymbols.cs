using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.TypeScript;

internal class SpecialSymbols
{
    public readonly INamedTypeSymbol TaskSymbol;
    public readonly INamedTypeSymbol GenericTaskSymbol;
    public readonly INamedTypeSymbol AsyncEnumerableSymbol;
    public readonly INamedTypeSymbol ChannelReaderSymbol;
    public readonly INamedTypeSymbol CancellationTokenSymbol;
    public readonly ImmutableArray<INamedTypeSymbol> HubAttributeSymbols;
    public readonly ImmutableArray<INamedTypeSymbol> ReceiverAttributeSymbols;
    public readonly ImmutableArray<INamedTypeSymbol> TranspilationSourceAttributeSymbols;

    public SpecialSymbols(Compilation compilation)
    {
        TaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")!;
        GenericTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")!;
        AsyncEnumerableSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1")!;
        ChannelReaderSymbol = compilation.GetTypeByMetadataName("System.Threading.Channels.ChannelReader`1")!;
        CancellationTokenSymbol = compilation.GetTypeByMetadataName("System.Threading.CancellationToken")!;

        var hubAttributeSymbol = compilation.GetTypesByMetadataName("TypedSignalR.Client.HubAttribute");
        var receiverAttributeSymbol = compilation.GetTypesByMetadataName("TypedSignalR.Client.ReceiverAttribute");
        var transpilationSourceAttributeSymbol = compilation.GetTypesByMetadataName("Tapper.TranspilationSourceAttribute");

        if (hubAttributeSymbol.IsEmpty)
        {
            throw new InvalidOperationException("TypedSignalR.Client.HubAttribute is not found");
        }

        if (receiverAttributeSymbol.IsEmpty)
        {
            throw new InvalidOperationException("TypedSignalR.Client.ReceiverAttribute is not found");
        }

        if (transpilationSourceAttributeSymbol.IsEmpty)
        {
            throw new InvalidOperationException("Tapper.TranspilationSourceAttribute is not found");
        }

        HubAttributeSymbols = hubAttributeSymbol;
        ReceiverAttributeSymbols = receiverAttributeSymbol;
        TranspilationSourceAttributeSymbols = transpilationSourceAttributeSymbol;
    }
}
