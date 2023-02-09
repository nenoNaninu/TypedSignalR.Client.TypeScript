using System.Collections.Generic;
using Tapper;
using TypedSignalR.Client.TypeScript.CodeAnalysis;

namespace TypedSignalR.Client.TypeScript.Templates;

public partial class ApiTemplate
{
    internal string Header { get; init; } = default!;
    internal IReadOnlyList<TypeMetadata> HubTypes { get; init; } = default!;
    internal IReadOnlyList<TypeMetadata> ReceiverTypes { get; init; } = default!;
    internal SpecialSymbols SpecialSymbols { get; init; } = default!;
    internal ITypedSignalRTranspilationOptions Options { get; init; } = default!;
}
