using Microsoft.AspNetCore.SignalR;
using TypedSignalR.Client.TypeScript.Tests.Shared;

namespace TypedSignalR.Client.TypeScript.Tests.Server.Hubs;

public class UnaryHub : Hub, IUnaryHub
{
    private readonly ILogger<UnaryHub> _logger;

    public UnaryHub(ILogger<UnaryHub> logger)
    {
        _logger = logger;
    }

    public Task<int> Add(int x, int y)
    {
        _logger.Log(LogLevel.Information, "UnaryHub.Add");

        return Task.FromResult(x + y);
    }

    public Task<string> Cat(string x, string y)
    {
        _logger.Log(LogLevel.Information, "UnaryHub.Cat");

        return Task.FromResult(x + y);
    }

    public Task<UserDefinedType> Echo(UserDefinedType instance)
    {
        _logger.Log(LogLevel.Information, "UnaryHub.Echo");

        return Task.FromResult(instance);
    }

    public Task<MyEnum> EchoMyEnum(MyEnum myEnum)
    {
        _logger.Log(LogLevel.Information, "UnaryHub.EchoMyEnum");
        return Task.FromResult(myEnum);
    }

    public Task<string> Get()
    {
        _logger.Log(LogLevel.Information, "UnaryHub.Get");

        return Task.FromResult("TypedSignalR.Client.TypeScript");
    }

    public Task<MyResponseItem[]> RequestArray(MyRequestItem[] array)
    {
        var buffer = new MyResponseItem[array.Length];

        for (int i = 0; i < array.Length; i++)
        {
            buffer[i] = new MyResponseItem
            {
                Text = $"{array[i].Text}{array[i].Text}"
            };
        }

        return Task.FromResult(buffer);
    }

    public Task<List<MyResponseItem2>> RequestList(List<MyRequestItem2> list)
    {
        var buffer = new List<MyResponseItem2>(list.Count);

        for (int i = 0; i < list.Count; i++)
        {
            buffer.Add(new MyResponseItem2
            {
                Id = list[list.Count - 1 - i].Id
            });
        }

        return Task.FromResult(buffer);
    }
}
