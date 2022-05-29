# Tests

Launch server.

```
$ dotnet run --project ./tests/TypedSignalR.Client.TypeScript.Tests.Server/TypedSignalR.Client.TypeScript.Tests.Server.csproj
```


Generate TypeScript code.
```
$ dotnet tsrts --project ./tests/TypedSignalR.Client.TypeScript.Tests.Shared/TypedSignalR.Client.TypeScript.Tests.Shared.csproj --out ./tests/TypeScriptTests/src/generated
```

Launch tests.

```
$ yarn --cwd ./tests/TypeScriptTests/ install
$ yarn --cwd ./tests/TypeScriptTests/ test
```


