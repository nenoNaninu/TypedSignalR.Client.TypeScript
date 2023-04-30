using System;
using System.Threading;
using System.Threading.Tasks;
using App.Interfaces.Chat;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/hubs/chathub")
    .Build();

var cancellationToken = Cancellation.CreateCompletionToken();

var hubProxy = connection.CreateHubProxy<IChatHub>(cancellationToken);
var subscription = connection.Register<IChatReceiver>(new ChatReceiver());


Console.WriteLine("Input username");
var username = Console.ReadLine();

if (string.IsNullOrEmpty(username))
{
    Console.WriteLine("The username must be not empty");
    return;
}

await connection.StartAsync(cancellationToken);

await hubProxy.Join(username);

var participants = await hubProxy.GetParticipants();

Console.WriteLine("Participants");

foreach (var participant in participants)
{
    Console.WriteLine(participant);
}

try
{
    while (!cancellationToken.IsCancellationRequested)
    {
        Console.WriteLine("Input Message! >");
        var message = Console.ReadLine();

        if (string.IsNullOrEmpty(message))
        {
            Console.WriteLine("The message must be not empty");
            continue;
        }

        await hubProxy.SendMessage(message);
    }
}
catch (OperationCanceledException)
{
    // ignore
}
finally
{
    Console.WriteLine("Dispose subscription...");
    subscription.Dispose();

    Console.WriteLine("Stop connection...");
    await connection.StopAsync();
}

class ChatReceiver : IChatReceiver
{
    public Task OnJoin(string username, DateTime dateTime)
    {
        Console.WriteLine("OnJoin");
        Console.WriteLine($"    Username: {username}");
        Console.WriteLine($"    DateTime: {dateTime}");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    public Task OnLeave(string username, DateTime dateTime)
    {
        Console.WriteLine("OnLeave");
        Console.WriteLine($"    Username: {username}");
        Console.WriteLine($"    DateTime: {dateTime}");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    public Task OnReceiveMessage(Message message)
    {
        Console.WriteLine("OnReceiveMessage");
        Console.WriteLine($"    Username: {message.Username}");
        Console.WriteLine($"    DateTime: {message.TimeStamp}");
        Console.WriteLine($"    Message : {message.Content}");
        Console.WriteLine();

        return Task.CompletedTask;
    }
}

static class Cancellation
{
    public static CancellationToken CreateCompletionToken()
    {
        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        return cts.Token;
    }
}
