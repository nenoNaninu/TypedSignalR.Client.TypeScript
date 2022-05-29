using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Interfaces.Chat;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Server.Hub;

public class ChatHub : Hub<IChatReceiver>, IChatHub
{
    private static ConcurrentDictionary<string, string> UserNames = new();
    private readonly ILogger _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<string>> GetParticipants()
    {
        _logger.Log(LogLevel.Information, "{id}: Invoke GetParticipants", this.Context.ConnectionId);
        return Task.FromResult(UserNames.Values as IEnumerable<string>);
    }

    public Task Join(string username)
    {
        _logger.Log(LogLevel.Information, "{id}: Invoke Join", this.Context.ConnectionId);

        UserNames[this.Context.ConnectionId] = username;
        this.Clients.All.OnJoin(username, DateTime.Now);
        return Task.CompletedTask;
    }

    public async Task Leave()
    {
        _logger.Log(LogLevel.Information, "{id}: Invoke Leave", this.Context.ConnectionId);

        var id = this.Context.ConnectionId;
        var username = UserNames[id];

        if (UserNames.TryRemove(id, out var _))
        {
            await this.Clients.All.OnLeave(username, DateTime.Now);
        }
    }

    public async Task SendMessage(string message)
    {
        _logger.Log(LogLevel.Information, "{id}: Invoke SendMessage", this.Context.ConnectionId);

        var username = UserNames[this.Context.ConnectionId];
        await this.Clients.All.OnReceiveMessage(new Message(username, message, DateTime.Now));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.Log(LogLevel.Information, "{id}: Invoke OnDisconnectedAsync", this.Context.ConnectionId);

        //var result = UserNames.TryRemove(this.Context.ConnectionId, out var username);

        if (UserNames.TryRemove(this.Context.ConnectionId, out var username))
        {
            await this.Clients.All.OnLeave(username, DateTime.Now);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
