# Tests

Launch server.

```
$ dotnet run --project ./tests/TypedSignalR.Client.TypeScript.Tests.Server/TypedSignalR.Client.TypeScript.Tests.Server.csproj
```


Generate TypeScript code. From last generator code.
```
$  dotnet run --project ./src/TypedSignalR.Client.TypeScript.Generator/TypedSignalR.Client.TypeScript.Generator.csproj -- -p ./tests/TypedSignalR.Client.TypeScript.Tests.Shared/TypedSignalR.Client.TypeScript.Tests.Shared.csproj -o ./tests/TypeScriptTests/src/generated
```

Launch tests.

```
$ yarn --cwd ./tests/TypeScriptTests/ install
$ yarn --cwd ./tests/TypeScriptTests/ test
```


