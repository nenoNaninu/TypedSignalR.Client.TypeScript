using Cocona;
using Microsoft.Build.Locator;
using Microsoft.Extensions.Logging;
using TypedSignalR.Client.TypeScript;

MSBuildLocator.RegisterDefaults();

var builder = CoconaApp.CreateBuilder();

builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
});

var app = builder.Build();

app.AddCommands<App>();

await app.RunAsync();
