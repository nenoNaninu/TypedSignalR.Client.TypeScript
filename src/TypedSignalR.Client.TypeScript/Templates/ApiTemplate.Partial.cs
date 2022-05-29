using System.Collections.Generic;
using Tapper;
using TypedSignalR.Client.TypeScript.CodeAnalysis;

namespace TypedSignalR.Client.TypeScript.Templates;

public partial class ApiTemplate
{
    internal string Header { get; init; } = default!;
    internal IReadOnlyList<TypeMetadata> HubTypes { get; init; } = default!;
    internal IReadOnlyList<TypeMetadata> ReceiverTypes { get; init; } = default!;
    internal ITranspilationOptions TranspilationOptions { get; init; } = default!;
}
