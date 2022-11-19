using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tapper;
using TypedSignalR.Client;

namespace App.Interfaces.Chat;

[Hub]
public interface IChatHub
{
    Task Join(string username);
    Task Leave();
    Task<IEnumerable<string>> GetParticipants();
    Task SendMessage(string message);
}


[Receiver]
public interface IChatReceiver
{
    Task OnReceiveMessage(Message message);
    Task OnLeave(string username, DateTime dateTime);
    Task OnJoin(string username, DateTime dateTime);
}

[TranspilationSource]
public record Message(string Username, string Content, DateTime TimeStamp);
