# TypedSignalR.Client.TypeScript

TypedSignalR.Client.TypeScript is a library/CLI tool that analyzes SignalR hub and receiver type definitions written in C# and generates TypeScript source code to provide strongly typed SignalR clients.

## Table of Contents

- [Why TypedSignalR.Client.TypeScript?](#why-typedsignalrclienttypescript)
- [Packages](#packages)
  - [Install Using .NET Tool](#install-using-net-tool)
- [Usage](#usage)
- [Supported Types](#supported-types)
  - [Built-in Supported Types](#built-in-supported-types)
  - [User Defined Types](#user-defined-types)
- [Analyzer](#analyzer)
- [Related Work](#related-work)


## Why TypedSignalR.Client.TypeScript?

Implementing SignalR Hubs (server-side) in C# can be strongly typed by using interfaces, but the [TypeScript SignalR client](https://github.com/dotnet/aspnetcore/tree/main/src/SignalR/clients/ts/signalr) is not strongly typed. To call Hub methods, we must specify the method defined in Hub using a string. We also have to determine the return type manually. Moreover, registering client methods called from a server also requires specifying the method name as a string, and we must set parameter types manually.

```ts
// TypeScript SignalR client

// Specify a hub method to invoke using string.
await connection.invoke("HubMethod1");

// Manually determine a return type.
// Parameters are cast to any type.
const value = await connection.invoke<number>("HubMethod2", "message", 99);

// Registering a client method requires a string, and parameter types must be set manually.
const func = (message: string, count: number) => {...};
connection.on("ClientMethod", func);
connection.off("ClientMethod", func);
```

These are very painful and cause bugs.

TypedSignalR.Client.TypeScript aims to **generates TypeScript** source code to provide strongly typed SignalR clients by **analyzing C#** interfaces in which the server and client methods are defined.

```ts
// TypedSignalR.Client.TypeScript
// Generated source code

export type HubProxyFactory<T> = {
    createHubProxy(connection: HubConnection): T;
}

export type ReceiverRegister<T> = {
    register(connection: HubConnection, receiver: T): Disposable;
}

// Overload function type
// Because string literal types are used, there is no need to worry about typos.
export type HubProxyFactoryProvider = {
    // In this example, IHub1 and IHub2 are transpiled from C# to TypeScript.
    (hubType: "IHub1"): HubProxyFactory<IHub1>;
    (hubType: "IHub2"): HubProxyFactory<IHub2>;
}

// Overload function type
export type ReceiverRegisterProvider = {
    // In this example, IReceiver1 and IReceiver2 are transpiled from C# to TypeScript.
    (receiverType: "IReceiver1"): ReceiverRegister<IReceiver1>;
    (receiverType: "IReceiver2"): ReceiverRegister<IReceiver2>;
}

export const getHubProxyFactory : HubProxyFactoryProvider = ...; 
export const getReceiverRegister : ReceiverRegisterProvider = ...;
```

```ts
// Usage of generated code.

const hubProxy = getHubProxyFactory("IHub1") // HubProxyFactory<IHub1>
    .createHubProxy(connection); // IHub1

const receiver : IReceiver1 = {...};

const subscription = getReceiverRegister("IReceiver1") // ReceiverRegister<IReceiver1>
    .register(connection, receiver); // Disposable

// We no longer need to specify the method using a string.
await hubProxy.hubMethod1();

// Both parameters and return types are strongly typed.
const value = await hubProxy.hubMethod2("message", 99); // Type inference works.

subscription.dispose();
```

The example of the actual generated code exists in [`/samples/console.typescript/generated`](/samples/console.typescript/generated/) so please have a look if you are interested.

## Packages

- [TypedSignalR.Client.TypeScript.Attributes](https://www.nuget.org/packages/TypedSignalR.Client.TypeScript.Attributes/)
- [TypedSignalR.Client.TypeScript.Analyzer](https://www.nuget.org/packages/TypedSignalR.Client.TypeScript.Analyzer/)
- [TypedSignalR.Client.TypeScript.Generator](https://www.nuget.org/packages/TypedSignalR.Client.TypeScript.Generator/)

### Install Using .NET Tool

Use `TypedSignalR.Client.TypeScript.Generatorr`(CLI Tool) to generate TypeScript source code to provide strongly typed SignalR clients.
`TypedSignalR.Client.TypeScript.Generatorr` can be easily installed using .NET Global Tools. You can use the installed tools with the command `dotnet tsts`(**T**yped**S**ignal**R**.Client.**T**ype**S**cript).

```
dotnet tool install --global TypedSignalR.Client.TypeScript.Generator
dotnet tsrts help
```

## Usage

First, add the following packages to your project. TypedSignalR.Client.TypeScript.Analyzer is optional, but recommended.

```
dotnet add package TypedSignalR.Client.TypeScript.Attributes
dotnet add package TypedSignalR.Client.TypeScript.Analyzer (optional, but recommended.)
dotnet add package Tapper.Analyzer (optional, but recommended.)
```

By adding `TypedSignalR.Client.TypeScript.Attributes` package, you can use three attributes.

- HubAttribute
- ReceiverAttribute
- TranspilationSourceAttribute

Then, annotate `HubAttribute` and `ReceiverAttribute` to each interface definitions of Hub and Receiver of SignalR.
Also, annotate `TranspilationSourceAttribute` to user-defined types used in the interface definition of Hub and Receiver.
Adding this attribute is relatively easy if you add the [TypedSignalR.Client.TypeScript.Analyzer](#analyzer) to your project.

```cs
using Tapper;
using TypedSignalR.Client.TypeScript;

namespace App.Interfaces.Chat;

[Hub] // <- Add Attribute
public interface IChatHub
{
    Task Join(string username);
    Task Leave();
    Task<IEnumerable<string>> GetParticipants();
    Task SendMessage(string message);
}

[Receiver] // <- Add Attribute
public interface IChatReceiver
{
    Task OnReceiveMessage(Message message);
    Task OnLeave(string username, DateTime dateTime);
    Task OnJoin(string username, DateTime dateTime);
}

[TranspilationSource] // <- Add Attribute
public record Message(string Username, string Content, DateTime TimeStamp);
```

Finally, enter the following command.
This command analyzes C# and generates TypeScript code.

```
dotnet tsrts --project path/to/Project.csproj --output generated
```

The generated code can be used as follows.
There are two important APIs that are generated.

- `getHubProxyFactory`
- `getReceiverRegister`

```ts
import { HubConnectionBuilder } from "@microsoft/signalr";
import { getHubProxyFactory, getReceiverRegister } from "./generated/TypedSignalR.Client";
import { IChatReceiver } from "./generated/TypedSignalR.Client/App.Interfaces.Chat";
import { Message } from "./generated/App.Interfaces.Chat";

const connection = new HubConnectionBuilder()
    .withUrl("https://example.com/realtime/chat")
    .build();

const receiver: IChatReceiver = {
    onReceiveMessage: (message: Message): Promise<void> => {...},
    onLeave: (username: string, dateTime: string | Date): Promise<void> => {...},
    onJoin: (username: string, dateTime: string | Date): Promise<void> => {...}
}

// The argument of getHubProxyFactory is a string literal type, not a string type.
// Therefore, there is no need to worry about typos.
const hubProxy = getHubProxyFactory("IChatHub")
    .createHubProxy(connection);

// Also, the argument of getReceiverRegister is a string literal type, not a string type.
// Therefore, again, there is no need to worry about typos.
const subscription = getReceiverRegister("IChatReceiver")
    .register(connection, receiver)

await connection.start()

await hubProxy.join(username)

const participants = await hubProxy.getParticipants()

// ...
```


## Supported Types

TypedSignalR.Client.TypeScript uses a library named [nenoNaninu/Tapper](https://github.com/nenoNaninu/Tapper) to convert C# types to TypeScript types.
Please read [Tapper's README](https://github.com/nenoNaninu/Tapper/blob/main/README.md#built-in-supported-types) for details on the correspondence between C# types and TypeScript types.
Here is a brief introduction of which types are supported.

### Built-in Supported Types

`bool` `byte` `sbyte` `char` `decimal` `double` `float` `int` `uint` `long` `ulong` `short` `ushort` `object` `string` `Uri` `Guid` `DateTime` `System.Nullable<T>` `byte[]` `T[]` `System.Array` `ArraySegment<T>` `List<T>` `LinkedList<T>` `Queue<T>` `Stack<T>` `HashSet<T>` `IEnumerable<T>` `IReadOnlyCollection<T>` `ICollection<T>` `IList<T>` `ISet<T>` `Dictionary<TKey, TValue>` `IDictionary<TKey, TValue>` `IReadOnlyDictionary<TKey, TValue>` `Tuple`

### User Defined Types

Of course, you can use user-defined types as well as Built-in Supported Types.
To transpile C# user-defined types to TypeScript types, annotate `TranspilationSourceAttribute`.

```cs
using Tapper;

[TranspilationSource] // <- Add attribute!
public class CustomType
{
    public List<int>? List { get; }
    public int Value { get; }
    public Guid Id { get; }
    public DateTime DateTime { get; }
}

[TranspilationSource] // <- Add attribute!
public enum MyEnum
{
    Zero = 0,
    One = 1,
    Two = 1 << 1,
    Four = 1 << 2,
}

[TranspilationSource] // <- Add attribute!
public record CustomType2(float Value, DateTime ReleaseDate);
```

## Analyzer
User-defined types used in parameters and return values of methods defined within interfaces annotated with `Hub` or `Receiver` must be annotated with `TranspilationSource`.
The Analyzer checks in real-time whether this rule is followed. If not, the IDE will tell you.

![analyzer](https://user-images.githubusercontent.com/27144255/170770137-28790bcf-08d1-403f-9625-2cdf6f390e76.gif)

## Related Work
- [nenoNaninu/TypedSignalR.Client](https://github.com/nenoNaninu/TypedSignalR.Client)
  - C# Source Generator to create strongly typed SignalR clients.
- [nenoNaninu/Tapper](https://github.com/nenoNaninu/Tapper)
  - A Tool Transpiling C# Type into TypeScript Type.

