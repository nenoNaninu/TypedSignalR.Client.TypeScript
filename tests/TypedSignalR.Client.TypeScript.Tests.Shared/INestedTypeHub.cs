using Tapper;

namespace TypedSignalR.Client.TypeScript.Tests.Shared;

[Hub]
public interface INestedTypeHub
{
    Task<int> Set(NestedTypeParentRequest obj);
    Task<NestedTypeParentResponse> Get();
}

[TranspilationSource]
public record NestedTypeParentRequest
    (IReadOnlyList<NestedTypeParentRequest.NestedTypeNestedTypeParentRequestItem> Items)
{
    [TranspilationSource]
    public record NestedTypeNestedTypeParentRequestItem(int Value, string? Message);
}

[TranspilationSource]
public record NestedTypeParentResponse
    (IReadOnlyList<NestedTypeParentResponse.NestedTypeParentResponseItem> Items)
{
    [TranspilationSource]
    public record NestedTypeParentResponseItem(int Value, string? Message);
}
