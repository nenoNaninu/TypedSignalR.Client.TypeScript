using System.Threading.Channels;
using Tapper;

namespace TypedSignalR.Client.TypeScript.Tests.Shared;

[Hub]
public interface IStreamingHub
{
    // Server-to-Client streaming
    IAsyncEnumerable<Person> ZeroParameter();
    IAsyncEnumerable<Person> CancellationTokenOnly(CancellationToken cancellationToken);
    IAsyncEnumerable<Message> Counter(Person publisher, int init, int step, int count);
    IAsyncEnumerable<Message> CancelableCounter(Person publisher, int init, int step, int count, CancellationToken cancellationToken);
    Task<IAsyncEnumerable<Message>> TaskCancelableCounter(Person publisher, int init, int step, int count, CancellationToken cancellationToken);

    // Server-to-Client streaming
    Task<ChannelReader<Person>> ZeroParameterChannel();
    Task<ChannelReader<Person>> CancellationTokenOnlyChannel(CancellationToken cancellationToken);
    Task<ChannelReader<Message>> CounterChannel(Person publisher, int init, int step, int count);
    Task<ChannelReader<Message>> CancelableCounterChannel(Person publisher, int init, int step, int count, CancellationToken cancellationToken);

    // Client-to-Server streaming
    // TODO: HOW TO TEST?
    Task UploadStream(Person publisher, IAsyncEnumerable<Person> stream);
    Task UploadStreamAsChannel(Person publisher, ChannelReader<Person> stream);
}

[TranspilationSource]
public record Person(Guid Id, string Name, int Number);

[TranspilationSource]
public record Message(Person Publisher, int Value);

[Hub]
public interface IMyStreamingHub
{
    // Server-to-Client streaming
    // Return type : IAsyncEnumerable<T> or Task<IAsyncEnumerable<T>> or Task<ChannelReader<T>>
    // Parameter : CancellationToken can use.
    Task<ChannelReader<MyStreamItem>> ServerToClientStreaming(MyType instance, int init, CancellationToken cancellationToken);

    // Client-to-Server streaming
    // Return type : Task (not Task<T>)
    // Parameter : IAsyncEnumerable<T> and ChannelReader<T> can use as stream from client to server.
    Task ClientToServerStreaming(MyType instance, ChannelReader<MyStreamItem> stream);
}

[TranspilationSource]
public class MyType
{
}

[TranspilationSource]
public class MyStreamItem
{
}
