using Microsoft.AspNetCore.SignalR;
using TypedSignalR.Client.TypeScript.Tests.Shared;

namespace TypedSignalR.Client.TypeScript.Tests.Server.Hubs;

public class NestedTypeHub : Hub, INestedTypeHub
{
    private readonly ILogger<NestedTypeHub> _logger;

    public NestedTypeHub(ILogger<NestedTypeHub> logger)
    {
        _logger = logger;
    }

    public Task<NestedTypeParentResponse> Get()
    {
        var obj = new NestedTypeParentResponse(new NestedTypeParentResponse.NestedTypeParentResponseItem[]
        {
            new NestedTypeParentResponse.NestedTypeParentResponseItem(1, "KAREN AIJO"),
            new NestedTypeParentResponse.NestedTypeParentResponseItem(17, "MAHIRU TSUYUZAKI"),
            new NestedTypeParentResponse.NestedTypeParentResponseItem(29, "HIKARI KAGURA"),
        });

        return Task.FromResult(obj);
    }

    public async Task<int> Set(NestedTypeParentRequest obj)
    {
        await ValueTask.CompletedTask;

        if (obj is null)
        {
            return -1;
        }

        if (obj.Items.Count != 2)
        {
            return -2;
        }

        if (obj.Items[0].Value == 15 && obj.Items[0].Message == "NANA DAIBA"
            && obj.Items[1].Value == 18 && obj.Items[1].Message == "MAYA TENDO")
        {
            return 99;
        }

        return -3;
    }
}
