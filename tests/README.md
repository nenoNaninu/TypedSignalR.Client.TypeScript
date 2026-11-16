# Tests

Launch server.

```
$ dotnet run --project ./tests/TypedSignalR.Client.TypeScript.Tests.Server/TypedSignalR.Client.TypeScript.Tests.Server.csproj
```


Generate TypeScript code.
```
$ dotnet tsrts --project ./tests/TypedSignalR.Client.TypeScript.Tests.Shared/TypedSignalR.Client.TypeScript.Tests.Shared.csproj --output ./tests/TypeScriptTests/src/generated/json
$ dotnet tsrts --project ./tests/TypedSignalR.Client.TypeScript.Tests.Shared/TypedSignalR.Client.TypeScript.Tests.Shared.csproj --output ./tests/TypeScriptTests/src/generated/json-null --nullable null
$ dotnet tsrts --project ./tests/TypedSignalR.Client.TypeScript.Tests.Shared/TypedSignalR.Client.TypeScript.Tests.Shared.csproj --output ./tests/TypeScriptTests/src/generated/msgpack --serializer MessagePack --naming-style none
```

Launch tests.

```
$ yarn --cwd ./tests/TypeScriptTests/ install
$ yarn --cwd ./tests/TypeScriptTests/ test
```


