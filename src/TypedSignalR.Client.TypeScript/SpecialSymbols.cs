using System;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.TypeScript;

internal class SpecialSymbols
{
    public readonly INamedTypeSymbol TaskSymbol;
    public readonly INamedTypeSymbol GenericTaskSymbol;
    public readonly INamedTypeSymbol AsyncEnumerableSymbol;
    public readonly INamedTypeSymbol ChannelReaderSymbol;
    public readonly INamedTypeSymbol CancellationTokenSymbol;
    public readonly INamedTypeSymbol HubAttributeSymbol;
    public readonly INamedTypeSymbol ReceiverAttributeSymbol;
    public readonly INamedTypeSymbol TranspilationSourceAttributeSymbol;

    public SpecialSymbols(Compilation compilation)
    {
        TaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")!;
        GenericTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")!;
        AsyncEnumerableSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1")!;
        ChannelReaderSymbol = compilation.GetTypeByMetadataName("System.Threading.Channels.ChannelReader`1")!;
        CancellationTokenSymbol = compilation.GetTypeByMetadataName("System.Threading.CancellationToken")!;

        var hubAttributeSymbol = compilation.GetTypeByMetadataName("TypedSignalR.Client.HubAttribute");
        var receiverAttributeSymbol = compilation.GetTypeByMetadataName("TypedSignalR.Client.ReceiverAttribute");
        var transpilationSourceAttributeSymbol = compilation.GetTypeByMetadataName("Tapper.TranspilationSourceAttribute");

        if (hubAttributeSymbol is null)
        {
            throw new InvalidOperationException("TypedSignalR.Client.HubAttribute is not found");
        }

        if (receiverAttributeSymbol is null)
        {
            throw new InvalidOperationException("TypedSignalR.Client.ReceiverAttribute is not found");
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
